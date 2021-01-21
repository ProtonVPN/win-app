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
using System.ComponentModel;
using System.Threading.Tasks;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Threading;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Api;
using ProtonVPN.Core.Auth;
using ProtonVPN.Core.Profiles;
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Servers.Models;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.User;
using ProtonVPN.Core.Vpn;
using ProtonVPN.Vpn.Connectors;

namespace ProtonVPN.Core.Service.Vpn
{
    public class VpnManager : IVpnManager, IVpnPlanAware, ILogoutAware, ISettingsAware
    {
        private readonly ILogger _logger;
        private readonly ProfileConnector _profileConnector;
        private readonly ProfileManager _profileManager;
        private readonly IVpnServiceManager _vpnServiceManager;
        private readonly IAppSettings _appSettings;
        private readonly ITaskQueue _taskQueue = new SerialTaskQueue();
        private readonly GuestHoleState _guestHoleState;
        private readonly IUserStorage _userStorage;

        private Profile _lastProfile;
        private ServerCandidates _lastServerCandidates;
        private Server _lastServer = Server.Empty();
        private bool _networkBlocked;

        public VpnManager(
            ILogger logger,
            ProfileConnector profileConnector,
            ProfileManager profileManager,
            IVpnServiceManager vpnServiceManager,
            IAppSettings appSettings,
            GuestHoleState guestHoleState,
            IUserStorage userStorage)
        {
            _logger = logger;
            _profileConnector = profileConnector;
            _profileManager = profileManager;
            _appSettings = appSettings;
            _vpnServiceManager = vpnServiceManager;
            _guestHoleState = guestHoleState;
            _userStorage = userStorage;
            _lastServerCandidates = _profileConnector.ServerCandidates(null);
        }

        public event EventHandler<VpnStateChangedEventArgs> VpnStateChanged;

        private VpnState _state = new VpnState(VpnStatus.Disconnected);

        public async Task Connect(Profile profile, Profile fallbackProfile = null)
        {
            await Queued(() => ConnectToBestProfileAsync(profile, fallbackProfile));
        }

        public async Task QuickConnect()
        {
            Profile profile = await _profileManager.GetProfileById(_appSettings.QuickConnect) ??
                              await _profileManager.GetFastestProfile();

            await Connect(profile);
        }

        public async Task Reconnect()
        {
            if (_lastProfile != null)
            {
                await Connect(_lastProfile);
            }
        }

        public async Task Disconnect(VpnError vpnError = VpnError.None)
        {
            await Queued(() => _vpnServiceManager.Disconnect(vpnError));
        }

        public async Task GetState()
        {
            await _vpnServiceManager.RepeatState();
        }

        public void OnVpnStateChanged(VpnStateChangedEventArgs e)
        {
            if (_guestHoleState.Active || _userStorage.User().Empty())
            {
                return;
            }

            VpnState state = e.State;

            if (!string.IsNullOrEmpty(state.EntryIp))
            {
                _lastServer = _lastServerCandidates.ServerByEntryIp(state.EntryIp);
            }

            _state = new VpnState(state.Status, _lastServer);
            _networkBlocked = e.NetworkBlocked;

            RaiseVpnStateChanged(new VpnStateChangedEventArgs(_state, e.Error, e.NetworkBlocked, e.Protocol));
        }

        public async Task OnVpnPlanChangedAsync(string plan)
        {
            if (_lastServer != null)
            {
                await UpdateServersOrReconnect();
            }
        }

        public void OnUserLoggedOut()
        {
            _state = new VpnState(VpnStatus.Disconnected);
            RaiseVpnStateChanged(new VpnStateChangedEventArgs(_state, VpnError.None, _networkBlocked));
        }

        private async Task ConnectToBestProfileAsync(Profile profile, Profile fallbackProfile = null)
        {
            IList<Profile> profiles = this.CreateProfilePreferenceList(profile, fallbackProfile);
            VpnManagerProfileCandidates profileCandidates = this.GetBestProfileCandidates(profiles);

            if (profileCandidates.CanConnect)
            {
                await ConnectAsync(profileCandidates.Profile, profileCandidates.Candidates);
            }
            else
            {
                _profileConnector.HandleNoServersAvailable(profileCandidates.Candidates.Items, profileCandidates.Profile);
            }
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
            VpnManagerProfileCandidates bestProfileCandidates = new VpnManagerProfileCandidates();

            foreach (Profile profile in profiles)
            {
                bestProfileCandidates.Profile = profile;
                bestProfileCandidates.Candidates = _profileConnector.ServerCandidates(profile);
                bestProfileCandidates.CanConnect = _profileConnector.CanConnect(bestProfileCandidates.Candidates, profile);

                if (bestProfileCandidates.CanConnect)
                {
                    break;
                }
            }

            return bestProfileCandidates;
        }

        private async Task ConnectAsync(Profile profile, ServerCandidates candidates)
        {
            if (profile.IsPredefined || profile.IsTemporary)
            {
                profile.Protocol = ToProtocol(_appSettings.OvpnProtocol);
            }

            _lastProfile = profile;
            _lastServerCandidates = candidates;
            await _profileConnector.Connect(candidates, profile);
        }

        private async Task UpdateServersOrReconnect()
        {
            if (_state.Status == VpnStatus.Disconnecting ||
                _state.Status == VpnStatus.Disconnected ||
                _lastProfile == null)
            {
                return;
            }

            _lastServerCandidates = _profileConnector.ServerCandidates(_lastProfile);

            if (!await _profileConnector.UpdateServers(_lastServerCandidates, _lastProfile))
            {
                Profile profile = await _profileManager.GetFastestProfile();
                await Connect(profile);
            }
        }

        public async void OnAppSettingsChanged(PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IAppSettings.KillSwitch)
                && !_appSettings.KillSwitch)
            {
                if (_networkBlocked &&
                    (_state.Status == VpnStatus.Disconnecting ||
                     _state.Status == VpnStatus.Disconnected))
                {
                    _logger.Info("Settings changed, Kill Switch disabled: Disconnecting to disable leak protection");
                    await _vpnServiceManager.Disconnect(VpnError.None);
                }
            }
        }

        private void RaiseVpnStateChanged(VpnStateChangedEventArgs e)
        {
            VpnStateChanged?.Invoke(this, e);
        }

        private Task Queued(Func<Task> function)
        {
            return _taskQueue.Enqueue(function);
        }

        private static Protocol ToProtocol(string protocol)
        {
            return protocol.EqualsIgnoringCase("auto") ? Protocol.Auto :
                protocol.EqualsIgnoringCase("udp") ? Protocol.OpenVpnUdp :
                protocol.EqualsIgnoringCase("tcp") ? Protocol.OpenVpnTcp :
                throw new ArgumentException();
        }
    }
}
