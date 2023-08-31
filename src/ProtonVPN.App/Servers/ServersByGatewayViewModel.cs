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

using System.Collections.Generic;
using ProtonVPN.Api.Contracts.Servers;
using ProtonVPN.Core.Abstract;
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Servers.Specs;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Vpn;
using ProtonVPN.Streaming;

namespace ProtonVPN.Servers
{
    public class ServersByGatewayViewModel : ServersByCountryViewModel
    {
        private readonly string _gatewayName;

        public override string Name => _gatewayName;

        public ServersByGatewayViewModel(string gatewayName,
            sbyte userTier,
            IAppSettings appSettings,
            ServerManager serverManager,
            VpnState vpnConnectionStatus,
            IStreamingServices streamingServices)
            : base(isB2B: true, gatewayName, userTier, appSettings, serverManager, vpnConnectionStatus, streamingServices)
        {
            _gatewayName = gatewayName;
        }

        protected override StreamingInfoPopupViewModel GetStreamingInfoPopupViewModel(sbyte tier)
        {
            IReadOnlyList<StreamingServiceViewModel> list = new List<StreamingServiceViewModel>();
            return new StreamingInfoPopupViewModel(_gatewayName, list);
        }

        protected override Specification<LogicalServerResponse> GetServerSpec(sbyte tier)
        {
            return new ServerByGateway(_gatewayName) && new B2BServer() && new ExactTierServer(tier);
        }

        public override bool HasAvailableServers()
        {
            if (!ServersAvailable.HasValue)
            {
                ServersAvailable = ServerManager.GatewayHasAvailableServers(_gatewayName, UserTier);
            }

            return ServersAvailable.Value;
        }

        public override bool Maintenance => ServerManager.GatewayUnderMaintenance(_gatewayName);
    }
}