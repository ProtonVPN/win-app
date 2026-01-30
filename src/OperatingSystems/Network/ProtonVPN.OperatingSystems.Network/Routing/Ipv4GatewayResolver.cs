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

using System.Diagnostics.CodeAnalysis;
using System.Net;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.NetworkLogs;
using ProtonVPN.OperatingSystems.Network.Contracts;
using ProtonVPN.OperatingSystems.Network.Contracts.Routing;

namespace ProtonVPN.OperatingSystems.Network.Routing;

public class Ipv4GatewayResolver : IIpv4GatewayResolver
{
    private readonly ILogger _logger;
    private readonly ISystemNetworkInterfaces _networkInterfaces;
    private readonly IRoutingTableHelper _routingTableHelper;

    public Ipv4GatewayResolver(
        ILogger logger,
        ISystemNetworkInterfaces networkInterfaces,
        IRoutingTableHelper routingTableHelper)
    {
        _logger = logger;
        _networkInterfaces = networkInterfaces;
        _routingTableHelper = routingTableHelper;
    }

    public bool TryGetBestIpv4Gateway(string excludedHardwareId, [NotNullWhen(true)] out Ipv4GatewayInfo? gatewayInfo)
    {
        INetworkInterface bestIpv4Interface = _networkInterfaces.GetBestInterfaceExcludingHardwareId(excludedHardwareId);
        uint? ipv4InterfaceMetric = _routingTableHelper.GetInterfaceMetric(bestIpv4Interface.Index, false);
        if (ipv4InterfaceMetric is null)
        {
            gatewayInfo = null;
            return false;
        }

        IPAddress ipv4Gateway = bestIpv4Interface.DefaultGateway;
        if (ipv4Gateway.Equals(IPAddress.Any))
        {
            _logger.Error<NetworkLog>("Failed to resolve IPv4 gateway because the gateway is missing.");
            gatewayInfo = null;
            return false;
        }

        if (!NetworkAddress.TryParse(ipv4Gateway.ToString(), out NetworkAddress ipv4GatewayAddress))
        {
            _logger.Error<NetworkLog>($"Failed to parse IPv4 gateway address '{ipv4Gateway}'.");
            gatewayInfo = null;
            return false;
        }

        gatewayInfo = new Ipv4GatewayInfo(bestIpv4Interface, ipv4GatewayAddress, ipv4InterfaceMetric.Value);
        return true;
    }
}