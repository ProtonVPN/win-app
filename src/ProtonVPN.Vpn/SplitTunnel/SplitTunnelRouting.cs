/*
 * Copyright (c) 2026 Proton AG
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

using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Common.Legacy;
using ProtonVPN.Common.Legacy.Vpn;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.NetworkLogs;
using ProtonVPN.OperatingSystems.Network.Contracts;
using ProtonVPN.OperatingSystems.Network.Contracts.Routing;
using ProtonVPN.Vpn.Gateways;

namespace ProtonVPN.Vpn.SplitTunnel;

public class SplitTunnelRouting : ISplitTunnelRouting
{
    private const int PERMIT_ROUTE_METRIC = 32000;

    private readonly ILogger _logger;
    private readonly IStaticConfiguration _config;
    private readonly IGatewayCache _gatewayCache;
    private readonly IIpv4GatewayResolver _ipv4GatewayResolver;
    private readonly IRoutingTableHelper _routingTableHelper;
    private readonly INetworkUtilities _networkUtilities;
    private readonly ISystemNetworkInterfaces _networkInterfaces;
    private readonly INetworkInterfaceLoader _networkInterfaceLoader;

    public SplitTunnelRouting(
        ILogger logger,
        IStaticConfiguration config,
        IGatewayCache gatewayCache,
        IIpv4GatewayResolver ipv4GatewayResolver,
        IRoutingTableHelper routingTableHelper,
        INetworkUtilities networkUtilities,
        ISystemNetworkInterfaces networkInterfaces,
        INetworkInterfaceLoader networkInterfaceLoader)
    {
        _logger = logger;
        _config = config;
        _gatewayCache = gatewayCache;
        _ipv4GatewayResolver = ipv4GatewayResolver;
        _routingTableHelper = routingTableHelper;
        _networkUtilities = networkUtilities;
        _networkInterfaces = networkInterfaces;
        _networkInterfaceLoader = networkInterfaceLoader;
    }

    public void SetUpRoutingTable(VpnConfig vpnConfig, string localIp, bool isIpv6Supported)
    {
        INetworkInterface tunnelInterface = _networkInterfaceLoader.GetByVpnProtocol(vpnConfig.VpnProtocol, vpnConfig.OpenVpnAdapter);
        INetworkInterface[] networkInterfaces = _networkInterfaces.GetInterfaces();

        switch (vpnConfig.SplitTunnelMode)
        {
            case SplitTunnelMode.Permit:
                SetUpPermitModeRoutes(vpnConfig, localIp, isIpv6Supported, tunnelInterface, networkInterfaces);
                break;
            case SplitTunnelMode.Block:
                SetUpBlockModeRoutes(vpnConfig, tunnelInterface, networkInterfaces);
                break;
        }
    }

    private void SetUpPermitModeRoutes(
        VpnConfig vpnConfig,
        string localIpv4Address,
        bool isIpv6Supported,
        INetworkInterface tunnelInterface,
        INetworkInterface[] networkInterfaces)
    {
        NetworkAddress.TryParse("0.0.0.0/0", out NetworkAddress defaultIpv4NetworkAddress);
        NetworkAddress.TryParse("::/0", out NetworkAddress defaultIpv6NetworkAddress);
        NetworkAddress.TryParse(localIpv4Address, out NetworkAddress localNetworkIpv4Address);
        NetworkAddress serverGatewayIpv4Address = new(_gatewayCache.Get());

        // Normally, this should only work for WireGuard, but seems to be working fine for OpenVPN as well
        NetworkAddress.TryParse(_config.WireGuard.DefaultServerGatewayIpv6Address, out NetworkAddress serverGatewayIpv6Address);

        //Remove default WireGuard route as it has metric 0, but instead we add the same route with low priority
        //so that we still have the route for include mode apps to be routed through the tunnel.
        _routingTableHelper.DeleteRoute(new()
        {
            Destination = defaultIpv4NetworkAddress,
            Gateway = localNetworkIpv4Address,
            InterfaceIndex = tunnelInterface.Index,
            IsIpv6 = false,
        });

        _routingTableHelper.CreateRoute(new()
        {
            Destination = defaultIpv4NetworkAddress,
            Gateway = localNetworkIpv4Address,
            InterfaceIndex = tunnelInterface.Index,
            Metric = PERMIT_ROUTE_METRIC,
            IsIpv6 = false,
        });

        _routingTableHelper.CreateRoute(new()
        {
            Destination = serverGatewayIpv4Address,
            Gateway = localNetworkIpv4Address,
            InterfaceIndex = tunnelInterface.Index,
            Metric = PERMIT_ROUTE_METRIC,
            IsIpv6 = false,
        });

        if (isIpv6Supported)
        {
            _routingTableHelper.DeleteRoute(new()
            {
                Destination = defaultIpv6NetworkAddress,
                Gateway = defaultIpv6NetworkAddress,
                InterfaceIndex = tunnelInterface.Index,
                IsIpv6 = true,
            });

            _routingTableHelper.CreateRoute(new()
            {
                Destination = defaultIpv6NetworkAddress,
                Gateway = defaultIpv6NetworkAddress,
                InterfaceIndex = tunnelInterface.Index,
                Metric = PERMIT_ROUTE_METRIC,
                IsIpv6 = true,
            });

            NetworkAddress? ipv6GatewayAddress = _networkUtilities.GetDefaultIpv6Gateway(tunnelInterface, networkInterfaces);
            if (ipv6GatewayAddress is null)
            {
                // If we cannot find a default gateway, we add a route to the loopback interface
                // to prevent not included traffic from being routed via the tunnel.
                _routingTableHelper.CreateRoute(GetIpv6LoopbackRoute(defaultIpv6NetworkAddress));
            }
        }

        foreach (string ip in vpnConfig.SplitTunnelIPs)
        {
            if (NetworkAddress.TryParse(ip, out NetworkAddress address))
            {
                _routingTableHelper.CreateRoute(new()
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

    private RouteConfiguration GetIpv6LoopbackRoute(NetworkAddress destination)
    {
        return new()
        {
            Destination = destination,
            Gateway = null,
            InterfaceIndex = _routingTableHelper.GetLoopbackInterfaceIndex().Value,
            Metric = 0,
            IsIpv6 = true,
        };
    }

    private void SetUpBlockModeRoutes(VpnConfig vpnConfig, INetworkInterface tunnelInterface, INetworkInterface[] networkInterfaces)
    {
        if (!_ipv4GatewayResolver.TryGetBestIpv4Gateway(
                _config.GetHardwareId(vpnConfig.OpenVpnAdapter),
                out Ipv4GatewayInfo ipv4GatewayInfo))
        {
            return;
        }

        INetworkInterface bestIpv4Interface = ipv4GatewayInfo.Interface;
        NetworkAddress ipv4GatewayAddress = ipv4GatewayInfo.GatewayAddress;
        uint ipv4InterfaceMetric = ipv4GatewayInfo.InterfaceMetric;

        NetworkAddress? ipv6GatewayAddress = _networkUtilities.GetDefaultIpv6Gateway(tunnelInterface, networkInterfaces);
        uint? loopbackInterfaceIndex = _routingTableHelper.GetLoopbackInterfaceIndex();

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

                _routingTableHelper.CreateRoute(new()
                {
                    Destination = address,
                    Gateway = gateway,
                    InterfaceIndex = interfaceIndex.Value,
                    Metric = ipv4InterfaceMetric,
                    IsIpv6 = address.IsIpV6,
                });
            }
        }
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
                        _routingTableHelper.DeleteRoute(address.Ip.ToString(), address.IsIpV6);
                    }
                }
                break;
            case SplitTunnelMode.Permit:
                if (NetworkAddress.TryParse("::/0", out NetworkAddress defaultIpv6NetworkAddress))
                {
                    _routingTableHelper.DeleteRoute(GetIpv6LoopbackRoute(defaultIpv6NetworkAddress));
                }
                break;
        }
    }
}