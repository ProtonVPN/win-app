/*
 * Copyright (c) 2023 Proton AG
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
using System.Linq;
using System.Threading.Tasks;
using ProtonVPN.Api;
using ProtonVPN.Common.Networking;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Auth;
using ProtonVPN.Core.Profiles;
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Servers.Models;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Vpn;
using ProtonVPN.Vpn.Connectors;

namespace ProtonVPN.Core.Service.Vpn
{
    public class VpnConnector : IVpnConnector, ILogoutAware
    {
        private readonly ProfileConnector _profileConnector;
        private readonly ProfileManager _profileManager;
        private readonly IAppSettings _appSettings;
        private readonly GuestHoleState _guestHoleState;
        private readonly IUserStorage _userStorage;
        private readonly ServerManager _serverManager;

        public Profile LastProfile { get; private set; }
        public Server LastServer { get; private set; } = Server.Empty();
        public VpnState State { get; private set; } = new(VpnStatus.Disconnected);
        public bool NetworkBlocked { get; private set; }

        public event EventHandler<VpnStateChangedEventArgs> VpnStateChanged;

        public VpnConnector(
            ProfileConnector profileConnector,
            ProfileManager profileManager,
            IAppSettings appSettings,
            GuestHoleState guestHoleState,
            IUserStorage userStorage,
            ServerManager serverManager)
        {
            _profileConnector = profileConnector;
            _profileManager = profileManager;
            _appSettings = appSettings;
            _guestHoleState = guestHoleState;
            _userStorage = userStorage;
            _serverManager = serverManager;
        }

        public async Task<IList<Server>> GetSortedAndValidQuickConnectServersAsync(int? maxServers = null)
        {
            Profile profile = await GetQuickConnectProfileAsync();
            IList<Profile> profiles = CreateProfilePreferenceList(profile);
            VpnManagerProfileCandidates profileCandidates = GetBestProfileCandidates(profiles);

            IEnumerable<Server> sortedServers = _profileConnector.SortServers(
                _profileConnector.GetValidServers(profileCandidates.Candidates),
                profileCandidates.Profile.ProfileType);
            if (maxServers.HasValue)
            {
                sortedServers = sortedServers.Take(maxServers.Value);
            }

            return sortedServers.ToList();
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

        private async Task ConnectToProfileCandidatesAsync(VpnManagerProfileCandidates profileCandidates, VpnProtocol? vpnProtocol = null, int? maxServers = null)
        {
            if (profileCandidates.CanConnect)
            {
                await ConnectAsync(profileCandidates, vpnProtocol, maxServers);
            }
            else
            {
                _profileConnector.HandleNoServersAvailable(profileCandidates);
            }
        }

        private async Task ConnectAsync(VpnManagerProfileCandidates profileCandidates, VpnProtocol? vpnProtocol = null, int? maxServers = null)
        {
            if (profileCandidates.Profile.IsPredefined || profileCandidates.Profile.IsTemporary)
            {
                profileCandidates.Profile.VpnProtocol = _appSettings.GetProtocol();
            }

            LastProfile = profileCandidates.Profile;
            await _profileConnector.Connect(profileCandidates, vpnProtocol, maxServers);
        }

        public async Task ConnectToPreSortedCandidatesAsync(IReadOnlyCollection<Server> sortedCandidates, VpnProtocol vpnProtocol)
        {
            await _profileConnector.ConnectWithPreSortedCandidates(sortedCandidates, vpnProtocol);
        }

        public async Task ConnectToProfileAsync(Profile profile, VpnProtocol? vpnProtocol = null)
        {
            VpnManagerProfileCandidates profileCandidates = GetProfileCandidates(profile);
            await ConnectToProfileCandidatesAsync(profileCandidates, vpnProtocol);
        }

        public void OnVpnStateChanged(VpnStateChangedEventArgs e)
        {
            if (_guestHoleState.Active || _userStorage.GetUser().Empty())
            {
                return;
            }

            SetPropertiesOnVpnStateChanged(e);
        }

        private void SetPropertiesOnVpnStateChanged(VpnStateChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.State.EntryIp))
            {
                LastServer = _serverManager.GetServerByEntryIpAndLabel(e.State.EntryIp, e.State.Label);
            }

            State = new VpnState(e.State.Status, LastServer, e.State.VpnProtocol, e.State.NetworkAdapterType);
            NetworkBlocked = e.NetworkBlocked;

            RaiseVpnStateChanged(new VpnStateChangedEventArgs(State, e.Error, e.NetworkBlocked));
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
    }
}