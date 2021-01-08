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
using System.Net.Http;
using System.Threading.Tasks;
using ProtonVPN.Core.Api;

namespace ProtonVPN.Config
{
    public class VpnConfig : IVpnConfig
    {
        private readonly IApiClient _apiClient;

        public VpnConfig(IApiClient apiClient, Common.Configuration.Config config)
        {
            _apiClient = apiClient;

            TcpPorts = config.DefaultOpenVpnTcpPorts;
            UdpPorts = config.DefaultOpenVpnUdpPorts;
            BlackHoleIps = config.DefaultBlackHoleIps;
            MaintenanceTrackerEnabled = config.MaintenanceTrackerEnabled;
            MaintenanceCheckInterval = config.MaintenanceCheckInterval;
        }

        public int[] TcpPorts { get; private set; }

        public int[] UdpPorts { get; private set; }

        public IReadOnlyList<string> BlackHoleIps { get; private set; }

        public bool NetShieldEnabled { get; private set; }

        public bool MaintenanceTrackerEnabled { get; private set; }

        public TimeSpan MaintenanceCheckInterval { get; private set; }

        public bool PortForwardingEnabled { get; private set; }

        public async Task Update()
        {
            try
            {
                var response = await _apiClient.GetVpnConfig();
                if (response.Success)
                {
                    TcpPorts = response.Value.OpenVpnConfig.DefaultPorts.Tcp;
                    UdpPorts = response.Value.OpenVpnConfig.DefaultPorts.Udp;
                    NetShieldEnabled = response.Value.FeatureFlags.NetShield;

                    if (response.Value.FeatureFlags.ServerRefresh.HasValue)
                    {
                        MaintenanceTrackerEnabled = response.Value.FeatureFlags.ServerRefresh.Value;
                    }

                    if (response.Value.FeatureFlags.PortForwarding.HasValue)
                    {
                        PortForwardingEnabled = response.Value.FeatureFlags.PortForwarding.Value;
                    }

                    if (response.Value.ServerRefreshInterval.HasValue)
                    {
                        MaintenanceCheckInterval = TimeSpan.FromMinutes(response.Value.ServerRefreshInterval.Value);
                    }

                    if (response.Value.HolesIps != null)
                    {
                        BlackHoleIps = response.Value.HolesIps;
                    }
                }
            }
            catch (HttpRequestException)
            {
                // ignore
            }
        }
    }
}
