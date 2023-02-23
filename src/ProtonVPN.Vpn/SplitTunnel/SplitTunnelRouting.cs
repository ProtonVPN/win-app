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

using ProtonVPN.Common;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.OS.Net;
using ProtonVPN.Common.OS.Net.NetworkInterface;
using ProtonVPN.Common.OS.Net.Routing;
using ProtonVPN.Common.Vpn;

namespace ProtonVPN.Vpn.SplitTunnel
{
    internal class SplitTunnelRouting
    {
        private const int ROUTE_METRIC = 32000;

        private readonly IConfiguration _config;
        private readonly INetworkInterfaces _networkInterfaces;
        private readonly INetworkInterfaceLoader _networkInterfaceLoader;

        public SplitTunnelRouting(IConfiguration config, INetworkInterfaces networkInterfaces, INetworkInterfaceLoader networkInterfaceLoader)
        {
            _config = config;
            _networkInterfaces = networkInterfaces;
            _networkInterfaceLoader = networkInterfaceLoader;
        }

        public void SetUpRoutingTable(VpnConfig vpnConfig, string localIp)
        {
            INetworkInterface adapter = _networkInterfaceLoader.GetByVpnProtocol(vpnConfig.VpnProtocol, vpnConfig.OpenVpnAdapter);
            switch (vpnConfig.SplitTunnelMode)
            {
                case SplitTunnelMode.Permit:
                    //Remove default wireguard route as it has metric 0, but instead we add the same route with low priority
                    //so that we still have the route for include mode apps to be routed through the tunnel.
                    RoutingTableHelper.DeleteRoute("0.0.0.0", "0.0.0.0", localIp);
                    RoutingTableHelper.CreateRoute("0.0.0.0", "0.0.0.0", localIp, adapter.Index, ROUTE_METRIC);
                    RoutingTableHelper.CreateRoute(_config.WireGuard.DefaultDnsServer, "255.255.255.255", localIp, adapter.Index, ROUTE_METRIC);

                    foreach (string ip in vpnConfig.SplitTunnelIPs)
                    {
                        NetworkAddress address = new(ip);
                        RoutingTableHelper.CreateRoute(address.Ip, address.Mask, localIp, adapter.Index, ROUTE_METRIC);
                    }
                    break;
                case SplitTunnelMode.Block:
                    INetworkInterface bestInterface = _networkInterfaces.GetBestInterface(_config.GetHardwareId(vpnConfig.OpenVpnAdapter));
                    int result = RoutingTableHelper.GetIpInterfaceEntry(bestInterface.Index, out MibIPInterfaceRow interfaceRow);
                    if (result == 0)
                    {
                        foreach (string ip in vpnConfig.SplitTunnelIPs)
                        {
                            NetworkAddress address = new(ip);
                            RoutingTableHelper.CreateRoute(
                                address.Ip,
                                address.Mask,
                                bestInterface.DefaultGateway.ToString(),
                                bestInterface.Index,
                                (int)interfaceRow.Metric);
                        }
                    }
                    break;
            }
        }

        public void DeleteRoutes(VpnConfig vpnConfig)
        {
            switch (vpnConfig.SplitTunnelMode)
            {
                case SplitTunnelMode.Block:
                    foreach (string ip in vpnConfig.SplitTunnelIPs)
                    {
                        RoutingTableHelper.DeleteRoute(new NetworkAddress(ip).Ip);
                    }
                    break;
            }
        }
    }
}