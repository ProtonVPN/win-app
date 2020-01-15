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

using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Servers.Models;
using ProtonVPN.Core.Servers.Specs;
using ProtonVPN.Core.Vpn;
using System.Collections.ObjectModel;

namespace ProtonVPN.Servers
{
    internal class ServersByExitNodeViewModel : BaseServerCollection
    {
        private readonly sbyte _userTier;
        private readonly ServerManager _serverManager;

        public ServersByExitNodeViewModel(string countryCode, sbyte userTier, ServerManager serverManager)
        {
            CountryCode = countryCode;
            _userTier = userTier;
            _serverManager = serverManager;
        }

        public override void LoadServers()
        {
            var list = _serverManager.GetServers(new SecureCoreServer() && new ExitCountryServer(CountryCode));
            var collection = new ObservableCollection<IServerListItem>();
            foreach (var server in list)
            {
                collection.Add(new SecureCoreItemViewModel(server, _userTier));
            }

            Servers = collection;
        }

        public override void OnVpnStateChanged(VpnState state)
        {
            Connected = state.Status.Equals(VpnStatus.Connected)
                        && state.Server is Server server
                        && server.IsSecureCore()
                        && server.ExitCountry.Equals(CountryCode);

            foreach (var s in Servers)
            {
                s.OnVpnStateChanged(state);
            }
        }

        protected override bool HasAvailableServers()
        {
            if (!ServersAvailable.HasValue)
            {
                ServersAvailable = _serverManager.CountryHasAvailableSecureCoreServers(CountryCode, _userTier);
            }

            return ServersAvailable.Value;
        }
    }
}
