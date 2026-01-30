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

using System;
using System.Collections.Generic;
using System.Linq;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Common.Legacy.Vpn;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Files.Contracts;
using ProtonVPN.OperatingSystems.Network.Contracts.Routing;
using ProtonVPN.Serialization.Contracts;
using ProtonVPN.Vpn.Common;

namespace ProtonVPN.Vpn.WireGuard;

public class WireGuardServerRouteManager : IWireGuardServerRouteManager
{
    private const Serializers ROUTE_FILE_SERIALIZER = Serializers.Json;

    private readonly IStaticConfiguration _config;
    private readonly IRoutingTableHelper _routingTableHelper;
    private readonly IIpv4GatewayResolver _ipv4GatewayResolver;
    private readonly IFileReaderWriter _fileReaderWriter;
    private readonly object _routesFileLock = new();

    private string WireGuardServerRoutesFilePath => _config.WireGuardServerRoutesFilePath;

    public WireGuardServerRouteManager(
        IStaticConfiguration config,
        IRoutingTableHelper routingTableHelper,
        IIpv4GatewayResolver ipv4GatewayResolver,
        IFileReaderWriter fileReaderWriter)
    {
        _config = config;
        _routingTableHelper = routingTableHelper;
        _ipv4GatewayResolver = ipv4GatewayResolver;
        _fileReaderWriter = fileReaderWriter;
    }

    public void CleanupPersistedRoutes()
    {
        lock (_routesFileLock)
        {
            List<PersistedServerRoute> routes = ReadPersistedServerRoutes();
            if (routes.Count == 0)
            {
                return;
            }

            List<PersistedServerRoute> remainingRoutes = DeletePersistedRoutes(routes);
            WritePersistedServerRoutes(remainingRoutes);
        }
    }

    public void CreateServerRoute(VpnEndpoint endpoint, VpnConfig vpnConfig)
    {
        if (!TryBuildRoute(endpoint, vpnConfig, out RouteConfiguration route, out NetworkAddress ipAddress))
        {
            return;
        }

        if (_routingTableHelper.RouteExists(route))
        {
            TrackServerRoute(ipAddress);
            return;
        }

        _routingTableHelper.DeleteRoute(ipAddress.Ip.ToString(), ipAddress.IsIpV6);
        _routingTableHelper.CreateRoute(route);
        TrackServerRoute(ipAddress);
    }

    public void DeleteServerRoutes(VpnEndpoint endpoint)
    {
        DeleteCurrentServerRoute(endpoint);

        lock (_routesFileLock)
        {
            List<PersistedServerRoute> routes = ReadPersistedServerRoutes();
            if (routes.Count > 0)
            {
                List<PersistedServerRoute> remainingRoutes = DeletePersistedRoutes(routes);
                WritePersistedServerRoutes(remainingRoutes);
            }
        }
    }

    private bool TryBuildRoute(VpnEndpoint endpoint, VpnConfig vpnConfig, out RouteConfiguration route, out NetworkAddress ipAddress)
    {
        if (endpoint is null ||
            vpnConfig is null ||
            !NetworkAddress.TryParse(endpoint.Server.Ip, out ipAddress) ||
            !_ipv4GatewayResolver.TryGetBestIpv4Gateway(
                _config.GetHardwareId(vpnConfig.OpenVpnAdapter),
                out Ipv4GatewayInfo ipv4GatewayInfo))
        {
            route = null;
            ipAddress = NetworkAddress.None;

            return false;
        }

        route = new RouteConfiguration
        {
            Destination = ipAddress,
            Gateway = ipv4GatewayInfo.GatewayAddress,
            InterfaceIndex = ipv4GatewayInfo.Interface.Index,
            Metric = ipv4GatewayInfo.InterfaceMetric,
            IsIpv6 = ipAddress.IsIpV6,
        };

        return true;
    }

    private void TrackServerRoute(NetworkAddress ipAddress)
    {
        lock (_routesFileLock)
        {
            List<PersistedServerRoute> routes = ReadPersistedServerRoutes();
            if (ContainsRoute(routes, ipAddress))
            {
                return;
            }

            routes.Add(new PersistedServerRoute
            {
                DestinationIpAddress = ipAddress.Ip.ToString(),
                IsIpv6 = ipAddress.IsIpV6,
            });

            WritePersistedServerRoutes(routes);
        }
    }

    private List<PersistedServerRoute> ReadPersistedServerRoutes()
    {
        return _fileReaderWriter
            .ReadOrNew<List<PersistedServerRoute>>(WireGuardServerRoutesFilePath, ROUTE_FILE_SERIALIZER)
            .Distinct()
            .ToList();
    }

    private void WritePersistedServerRoutes(List<PersistedServerRoute> routes)
    {
        _fileReaderWriter.Write(routes.Distinct(), WireGuardServerRoutesFilePath, ROUTE_FILE_SERIALIZER);
    }

    private List<PersistedServerRoute> DeletePersistedRoutes(IEnumerable<PersistedServerRoute> routes)
    {
        List<PersistedServerRoute> remainingRoutes = [];
        foreach (PersistedServerRoute route in routes)
        {
            if (!TryDeletePersistedRoute(route))
            {
                remainingRoutes.Add(route);
            }
        }

        return remainingRoutes;
    }

    private static bool ContainsRoute(IEnumerable<PersistedServerRoute> routes, NetworkAddress ipAddress)
    {
        PersistedServerRoute target = new()
        {
            DestinationIpAddress = ipAddress.Ip.ToString(),
            IsIpv6 = ipAddress.IsIpV6,
        };

        return routes.Contains(target);
    }

    private bool TryDeletePersistedRoute(PersistedServerRoute route)
    {
        if (string.IsNullOrWhiteSpace(route.DestinationIpAddress))
        {
            return false;
        }

        return _routingTableHelper.DeleteRoute(route.DestinationIpAddress, route.IsIpv6);
    }

    private void DeleteCurrentServerRoute(VpnEndpoint endpoint)
    {
        if (endpoint is null ||
            !NetworkAddress.TryParse(endpoint.Server.Ip, out NetworkAddress ipAddress))
        {
            return;
        }

        bool isDeleted = _routingTableHelper.DeleteRoute(ipAddress.Ip.ToString(), ipAddress.IsIpV6);
        if (!isDeleted)
        {
            TrackServerRoute(ipAddress);
        }
    }
}
