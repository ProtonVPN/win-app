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

using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using ProtonVPN.Core.Api;

namespace ProtonVPN.Config
{
    public class VpnConfig : IVpnConfig
    {
        private int[] _tcpPorts;
        private int[] _udpPorts;
        private IReadOnlyList<string> _blackHoleIps;
        private readonly IApiClient _apiClient;

        public VpnConfig(IApiClient apiClient, int[] defaultTcpPorts, int[] defaultUdpPorts, IReadOnlyList<string> defaultBlackHoleIps)
        {
            _apiClient = apiClient;
            _tcpPorts = defaultTcpPorts;
            _udpPorts = defaultUdpPorts;
            _blackHoleIps = defaultBlackHoleIps;
        }

        public async Task Update()
        {
            try
            {
                var response = await _apiClient.GetVpnConfig();
                if (response.Success)
                {
                    _tcpPorts = response.Value.OpenVpnConfig.DefaultPorts.Tcp;
                    _udpPorts = response.Value.OpenVpnConfig.DefaultPorts.Udp;
                    if (response.Value.HolesIps != null)
                    {
                        _blackHoleIps = response.Value.HolesIps;
                    }
                }
            }
            catch (HttpRequestException)
            {
                // ignore
            }
        }

        public int[] TcpPorts()
        {
            return _tcpPorts;
        }

        public int[] UdpPorts()
        {
            return _udpPorts;
        }

        public IReadOnlyList<string> BlackHoleIps()
        {
            return _blackHoleIps;
        }
    }
}
