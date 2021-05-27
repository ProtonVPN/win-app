/*
 * Copyright (c) 2020 Proton Technologies AG
 *
 * This file is part of ProtonVPN.
 *
 * ProtonVPN is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * ProtonVPN is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with ProtonVPN.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProtonVPN.Common.OS.Net;
using ProtonVPN.Common.OS.Net.NetworkInterface;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Api;
using ProtonVPN.Core.Auth;
using ProtonVPN.Core.Profiles;
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Servers.Models;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.User;
using ProtonVPN.Core.Vpn;
using ProtonVPN.Core.Window.Popups;
using ProtonVPN.Vpn.Connectors;
using ProtonVPN.Windows.Popups;
using Sentry;
using Sentry.Protocol;

namespace ProtonVPN.Core.Service.Vpn
{
    public class VpnConnector : IVpnConnector, IVpnPlanAware, ILogoutAware
    {
        private readonly ProfileConnector _profileConnector;
        private readonly ProfileManager _profileManager;
        private readonly IAppSettings _appSettings;
        private readonly GuestHoleState _guestHoleState;
        private readonly IUserStorage _userStorage;
        private readonly INetworkInterfaceLoader _networkInterfaceLoader; 
        private readonly IPopupWindows _popupWindows;

        private bool _tunChangedToTap;

        public Profile LastProfile { get; private set; }
        public ServerCandidates LastServerCandidates { get; private set; }
        public Server LastServer { get; private set; } = Server.Empty();
        public VpnState State { get; private set; } = new VpnState(VpnStatus.Disconnected);
        public bool NetworkBlocked { get; private set; }

        public event EventHandler<VpnStateChangedEventArgs> VpnStateChanged;

        public VpnConnector(
            ProfileConnector profileConnector,
            ProfileManager profileManager,
            IAppSettings appSettings, 
            GuestHoleState guestHoleState, 
            IUserStorage userStorage,
            INetworkInterfaceLoader networkInterfaceLoader, 
            IPopupWindows popupWindows)
        {
            _profileConnector = profileConnector;
            _profileManager = profileManager;
            _appSettings = appSettings;
            _guestHoleState = guestHoleState;
            _userStorage = userStorage;
            _networkInterfaceLoader = networkInterfaceLoader;
            _popupWindows = popupWindows;
            LastServerCandidates = _profileConnector.ServerCandidates(null);
        }

        public async Task QuickConnectAsync(int? maxServers = null)
        {
            Profile profile = await GetQuickConnectProfileAsync();
            await ConnectToBestProfileAsync(profile, maxServers: maxServers);
        }

        private async Task<Profile> GetQuickConnectProfileAsync()
        {
            return await _profileManager.GetProfileById(_appSettings.QuickConnect) ??
                   await _profileManager.GetFastestProfile();
        }

        public async Task ConnectToBestProfileAsync(Profile profile, Profile fallbackProfile = null, int? maxServers = null)
        {
            await ValidateConnectionAsync(() => ExecuteConnectToBestProfileAsync(profile, fallbackProfile, maxServers));
        }

        private async Task ValidateConnectionAsync(Func<Task> connectionFunction)
        {
            INetworkInterface tunInterface = _networkInterfaceLoader.GetTunInterface();
            INetworkInterface tapInterface = _networkInterfaceLoader.GetTapInterface();
            if (tunInterface == null && tapInterface == null)
            {
                RaiseVpnStateChanged(new VpnStateChangedEventArgs(
                    new VpnState(VpnStatus.Disconnected), VpnError.NoTapAdaptersError, false));
                return;
            }

            if (tunInterface == null && _appSettings.UseTunAdapter)
            {
                _appSettings.UseTunAdapter = false;
                _tunChangedToTap = true;
            }

            await connectionFunction();
        }

        private async Task ExecuteConnectToBestProfileAsync(Profile profile, Profile fallbackProfile = null, int? maxServers = null)
        {
            IList<Profile> profiles = CreateProfilePreferenceList(profile, fallbackProfile);
            VpnManagerProfileCandidates profileCandidates = GetBestProfileCandidates(profiles);
            await ConnectToProfileCandidatesAsync(profileCandidates, maxServers: maxServers);
        }

        private IList<Profile> CreateProfilePreferenceList(Profile profile, Profile fallbackProfile = null)
        {
            IList<Profile> profiles = new List<Profile>();

            if (profile.Features == Features.None && _appSettings.IsPortForwardingEnabled())
            {
                Profile p2pProfile = profile.Clone();
                p2pProfile.Features = Features.P2P;
                profiles.Add(p2pProfile);
            }

            profiles.Add(profile);

            if (fallbackProfile != null)
            {
                profiles.Add(fallbackProfile);
            }

            return profiles;
        }

        private VpnManagerProfileCandidates GetBestProfileCandidates(IList<Profile> profiles)
        {
            VpnManagerProfileCandidates bestProfileCandidates = new();

            foreach (Profile profile in profiles)
            {
                bestProfileCandidates = GetProfileCandidates(profile);

                if (bestProfileCandidates.CanConnect)
                {
                    break;
                }
            }

            return bestProfileCandidates;
        }

        private VpnManagerProfileCandidates GetProfileCandidates(Profile profile)
        {
            VpnManagerProfileCandidates profileCandidates = new()
            {
                Profile = profile,
                Candidates = _profileConnector.ServerCandidates(profile)
            };
            profileCandidates.CanConnect = _profileConnector.CanConnect(profileCandidates.Candidates);

            return profileCandidates;
        }

        private async Task ConnectToProfileCandidatesAsync(VpnManagerProfileCandidates profileCandidates, Protocol? protocol = null, int? maxServers = null)
        {
            if (profileCandidates.CanConnect)
            {
                await ConnectAsync(profileCandidates.Profile, profileCandidates.Candidates, protocol, maxServers);
            }
            else
            {
                _profileConnector.HandleNoServersAvailable(profileCandidates.Candidates.Items, profileCandidates.Profile);
            }
        }

        private async Task ConnectAsync(Profile profile, ServerCandidates candidates, Protocol? protocol = null, int? maxServers = null)
        {
            if (profile.IsPredefined || profile.IsTemporary)
            {
                profile.Protocol = _appSettings.GetProtocol();
            }

            LastProfile = profile;
            LastServerCandidates = candidates;
            await _profileConnector.Connect(candidates, profile, protocol, maxServers);
        }

        public async Task ConnectToPreSortedCandidatesAsync(ServerCandidates sortedCandidates, Protocol protocol)
        {
            await ValidateConnectionAsync(() => ExecuteConnectToPreSortedCandidatesAsync(sortedCandidates, protocol));
        }

        private async Task ExecuteConnectToPreSortedCandidatesAsync(ServerCandidates sortedCandidates, Protocol protocol)
        {
            LastProfile = null;
            LastServerCandidates = sortedCandidates;
            await _profileConnector.ConnectWithPreSortedCandidates(sortedCandidates, protocol);
        }

        public async Task ConnectToProfileAsync(Profile profile, Protocol? protocol = null)
        {
            await ValidateConnectionAsync(() => ExecuteConnectToProfileAsync(profile, protocol));
        }

        private async Task ExecuteConnectToProfileAsync(Profile profile, Protocol? protocol = null)
        {
            VpnManagerProfileCandidates profileCandidates = GetProfileCandidates(profile);
            await ConnectToProfileCandidatesAsync(profileCandidates, protocol);
        }

        public void OnVpnStateChanged(VpnStateChangedEventArgs e)
        {
            if (_guestHoleState.Active || _userStorage.User().Empty())
            {
                return;
            }

            SetPropertiesOnVpnStateChanged(e);
            SetAdapterStatusOnVpnStateChanged(e);
        }

        private void SetPropertiesOnVpnStateChanged(VpnStateChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.State.EntryIp))
            {
                LastServer = LastServerCandidates.ServerByEntryIpAndLabel(e.State.EntryIp, e.State.Label);
            }

            State = new VpnState(e.State.Status, LastServer);
            NetworkBlocked = e.NetworkBlocked;

            RaiseVpnStateChanged(new VpnStateChangedEventArgs(State, e.Error, e.NetworkBlocked, e.Protocol));
        }

        private void SetAdapterStatusOnVpnStateChanged(VpnStateChangedEventArgs e)
        {
            switch (e.State.Status)
            {
                case VpnStatus.Connected when _tunChangedToTap:
                    _tunChangedToTap = false;
                    SendTunFallbackEvent();
                    _popupWindows.Show<TunFallbackPopupViewModel>();
                    break;
                case VpnStatus.Disconnected:
                    _tunChangedToTap = false;
                    break;
            }
        }

        private void SendTunFallbackEvent()
        {
            SentrySdk.CaptureEvent(new SentryEvent
            {
                Message = "Successful TAP connection after failed with TUN.",
                Level = SentryLevel.Info,
            });
        }

        private void RaiseVpnStateChanged(VpnStateChangedEventArgs e)
        {
            VpnStateChanged?.Invoke(this, e);
        }

        public void OnUserLoggedOut()
        {
            State = new VpnState(VpnStatus.Disconnected);
            RaiseVpnStateChanged(new VpnStateChangedEventArgs(State, VpnError.None, NetworkBlocked));
        }
        
        public async Task OnVpnPlanChangedAsync(VpnPlanChangedEventArgs e)
        {
            if (LastServer != null)
            {
                await UpdateServers();
            }
        }

        private async Task UpdateServers()
        {
            if (State.Status != VpnStatus.Disconnecting && 
                State.Status != VpnStatus.Disconnected && 
                LastProfile != null)
            {
                LastServerCandidates = _profileConnector.ServerCandidates(LastProfile);
                await _profileConnector.UpdateServers(LastServerCandidates, LastProfile);
            }
        }
    }
}
