/*
 * Copyright (c) 2025 Proton AG
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
using System.Linq;
using System.Net;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Common.Core.Networking.Routing;
using ProtonVPN.Common.Legacy;
using ProtonVPN.Common.Legacy.Vpn;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.NetworkLogs;
using ProtonVPN.OperatingSystems.Network.Contracts;
using Vanara.PInvoke;
using static Vanara.PInvoke.IpHlpApi;
using static Vanara.PInvoke.Ws2_32;

namespace ProtonVPN.Vpn.SplitTunnel;

internal class SplitTunnelRouting
{
    private const int PERMIT_ROUTE_METRIC = 32000;

    private readonly ILogger _logger;
    private readonly IStaticConfiguration _config;
    private readonly ISystemNetworkInterfaces _networkInterfaces;
    private readonly INetworkInterfaceLoader _networkInterfaceLoader;

    public SplitTunnelRouting(
        ILogger logger,
        IStaticConfiguration config,
        ISystemNetworkInterfaces networkInterfaces,
        INetworkInterfaceLoader networkInterfaceLoader)
    {
        _logger = logger;
        _config = config;
        _networkInterfaces = networkInterfaces;
        _networkInterfaceLoader = networkInterfaceLoader;
    }

    public void SetUpRoutingTable(VpnConfig vpnConfig, string localIp)
    {
        switch (vpnConfig.SplitTunnelMode)
        {
            case SplitTunnelMode.Permit:
                INetworkInterface tunnelInterface = _networkInterfaceLoader.GetByVpnProtocol(vpnConfig.VpnProtocol, vpnConfig.OpenVpnAdapter);
                SetUpPermitModeRoutes(vpnConfig.SplitTunnelIPs, tunnelInterface, localIp);
                break;
            case SplitTunnelMode.Block:
                SetUpBlockModeRoutes(vpnConfig);
                break;
        }
    }

    private void SetUpPermitModeRoutes(IReadOnlyCollection<string> splitTunnelIps, INetworkInterface tunnelInterface, string localIpv4Address)
    {
        NetworkAddress.TryParse("0.0.0.0/0", out NetworkAddress defaultIpv4NetworkAddress);
        NetworkAddress.TryParse("::/0", out NetworkAddress defaultIpv6NetworkAddress);
        NetworkAddress.TryParse(localIpv4Address, out NetworkAddress localNetworkIpv4Address);
        NetworkAddress.TryParse(_config.WireGuard.DefaultServerGatewayIpv4Address, out NetworkAddress dnsNetworkIpv4Address);
        NetworkAddress.TryParse(_config.WireGuard.DefaultServerGatewayIpv6Address, out NetworkAddress serverGatewayIpv6Address);

        //Remove default WireGuard route as it has metric 0, but instead we add the same route with low priority
        //so that we still have the route for include mode apps to be routed through the tunnel.
        RoutingTableHelper.DeleteRoute(new()
        {
            Destination = defaultIpv4NetworkAddress,
            Gateway = localNetworkIpv4Address,
            InterfaceIndex = tunnelInterface.Index,
            IsIpv6 = false,
        });

        RoutingTableHelper.CreateRoute(new()
        {
            Destination = defaultIpv4NetworkAddress,
            Gateway = localNetworkIpv4Address,
            InterfaceIndex = tunnelInterface.Index,
            Metric = PERMIT_ROUTE_METRIC,
            IsIpv6 = false,
        });

        RoutingTableHelper.DeleteRoute(new()
        {
            Destination = defaultIpv6NetworkAddress,
            Gateway = defaultIpv6NetworkAddress,
            InterfaceIndex = tunnelInterface.Index,
            IsIpv6 = true,
        });

        RoutingTableHelper.CreateRoute(new()
        {
            Destination = dnsNetworkIpv4Address,
            Gateway = localNetworkIpv4Address,
            InterfaceIndex = tunnelInterface.Index,
            Metric = PERMIT_ROUTE_METRIC,
            IsIpv6 = false,
        });

        foreach (string ip in splitTunnelIps)
        {
            if (NetworkAddress.TryParse(ip, out NetworkAddress address))
            {
                RoutingTableHelper.CreateRoute(new()
                {
                    Destination = address,
                    Gateway = address.IsIpV6 ? serverGatewayIpv6Address : localNetworkIpv4Address,
                    InterfaceIndex = tunnelInterface.Index,
                    Metric = PERMIT_ROUTE_METRIC,
                    IsIpv6 = address.IsIpV6,
                });
            }
        }
    }

    private void SetUpBlockModeRoutes(VpnConfig vpnConfig)
    {
        INetworkInterface bestIpv4Interface = _networkInterfaces.GetBestInterface(hardwareIdToExclude: _config.GetHardwareId(vpnConfig.OpenVpnAdapter));
        uint? ipv4InterfaceMetric = RoutingTableHelper.GetInterfaceMetric(bestIpv4Interface.Index, false);
        if (ipv4InterfaceMetric is null)
        {
            return;
        }

        if (!NetworkAddress.TryParse(bestIpv4Interface.DefaultGateway.ToString(), out NetworkAddress ipv4GatewayAddress))
        {
            return;
        }

        NetworkAddress? ipv6GatewayAddress = GetDefaultIpv6Gateway(vpnConfig);
        uint? loopbackInterfaceIndex = RoutingTableHelper.GetLoopbackInterfaceIndex();

        foreach (string ip in vpnConfig.SplitTunnelIPs)
        {
            if (NetworkAddress.TryParse(ip, out NetworkAddress address))
            {
                NetworkAddress? gateway = address.IsIpV6
                    ? ipv6GatewayAddress
                    : ipv4GatewayAddress;

                uint? interfaceIndex = gateway is null
                    ? loopbackInterfaceIndex
                    : bestIpv4Interface.Index;

                if (interfaceIndex is null)
                {
                    _logger.Error<NetworkLog>($"Ignoring route create with IP {address} address due to a missing interface index.");
                    continue;
                }

                RoutingTableHelper.CreateRoute(new()
                {
                    Destination = address,
                    Gateway = gateway,
                    InterfaceIndex = interfaceIndex.Value,
                    Metric = ipv4InterfaceMetric.Value,
                    IsIpv6 = address.IsIpV6,
                });
            }
        }
    }

    private NetworkAddress? GetDefaultIpv6Gateway(VpnConfig vpnConfig)
    {
        List<uint> interfacesWithGlobalUnicastAddresses = GetInterfaceIndexesWithGlobalUnicastAddress(vpnConfig);
        if (interfacesWithGlobalUnicastAddresses.Count == 0)
        {
            _logger.Warn<NetworkLog>("No interface found with global unicast address.");
            return null;
        }

        List<MIB_IPFORWARD_ROW2> ipForwardRows = GetIpv6DefaultRoutes(interfacesWithGlobalUnicastAddresses);
        if (ipForwardRows.Count == 0)
        {
            _logger.Error<NetworkLog>("No IPv6 route found.");
            return null;
        }

        Dictionary<uint, uint> interfaceMetrics = GetIpv6InterfaceMetrics();

        byte[] nextHop = null;
        uint bestEffectiveMetric = uint.MaxValue;

        foreach (MIB_IPFORWARD_ROW2 row in ipForwardRows)
        {
            if (!interfaceMetrics.TryGetValue(row.InterfaceIndex, out uint interfaceMetric))
            {
                continue;
            }

            uint effective = row.Metric + interfaceMetric;
            if (effective < bestEffectiveMetric)
            {
                bestEffectiveMetric = effective;
                nextHop = row.NextHop.Ipv6.sin6_addr.bytes;
            }
        }

        if (nextHop is not null)
        {
            if (NetworkAddress.TryParse(new IPAddress(nextHop).ToString(), out NetworkAddress ipv6DefaultRoute))
            {
                return ipv6DefaultRoute;
            }
        }

        return null;
    }

    private List<uint> GetInterfaceIndexesWithGlobalUnicastAddress(VpnConfig vpnConfig)
    {
        INetworkInterface tunnelInterface = _networkInterfaceLoader.GetByVpnProtocol(vpnConfig.VpnProtocol, vpnConfig.OpenVpnAdapter);

        return _networkInterfaces
            .GetInterfaces()
            .Where(i => !i.Equals(tunnelInterface))
            .Where(i => i.GetUnicastAddresses().Any(a => a.IsGlobalUnicastAddress()))
            .Select(i => i.Index)
            .ToList();
    }

    private List<MIB_IPFORWARD_ROW2> GetIpv6DefaultRoutes(List<uint> interfacesWithGlobalUnicastAddresses)
    {
        Win32Error result = GetIpForwardTable2(ADDRESS_FAMILY.AF_INET6, out MIB_IPFORWARD_TABLE2 interfaces);
        if (result.Failed)
        {
            _logger.Error<NetworkLog>("Failed to retrieve IP forward table.", result.GetException());
            return null;
        }

        return interfaces.Table.Where(row => IsDefaultIpv6Route(row, interfacesWithGlobalUnicastAddresses)).ToList();
    }

    private bool IsDefaultIpv6Route(MIB_IPFORWARD_ROW2 row, List<uint> interfacesWithGlobalUnicastAddresses)
    {
        return row.DestinationPrefix.PrefixLength == 0 &&
            new IPAddress(row.DestinationPrefix.Prefix.Ipv6.sin6_addr.bytes).Equals(IPAddress.IPv6None) &&
            interfacesWithGlobalUnicastAddresses.Contains(row.InterfaceIndex);
    }

    private Dictionary<uint, uint> GetIpv6InterfaceMetrics()
    {
        Win32Error result = GetIpInterfaceTable(ADDRESS_FAMILY.AF_INET6, out MIB_IPINTERFACE_TABLE interfaces);
        if (result.Failed)
        {
            _logger.Error<NetworkLog>("Failed to retrieve IP interface table.", result.GetException());

            return [];
        }

        Dictionary<uint, uint> metrics = [];

        foreach (MIB_IPINTERFACE_ROW interfaceRow in interfaces.Table)
        {
            metrics.Add(interfaceRow.InterfaceIndex, interfaceRow.Metric);
        }

        return metrics;
    }

    public void DeleteRoutes(VpnConfig vpnConfig)
    {
        switch (vpnConfig.SplitTunnelMode)
        {
            case SplitTunnelMode.Block:
                foreach (string ip in vpnConfig.SplitTunnelIPs)
                {
                    if (NetworkAddress.TryParse(ip, out NetworkAddress address))
                    {
                        RoutingTableHelper.DeleteRoute(address.Ip.ToString(), address.IsIpV6);
                    }
                }
                break;
        }
    }
}