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

using System.Net;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.SplitTunnelLogs;
using ProtonVPN.NetworkFilter;

namespace ProtonVPN.Service.SplitTunneling;

internal class SplitTunnelClient : ISplitTunnelClient
{
    private readonly ILogger _logger;
    private readonly SplitTunnelNetworkFilters _filters;

    public SplitTunnelClient(
        ILogger logger,
        SplitTunnelNetworkFilters filters)
    {
        _logger = logger;
        _filters = filters;
    }

    public void EnableExcludeMode(string[] appPaths, IPAddress localIpv4Address, IPAddress localIpv6Address)
    {
        if ((appPaths == null || appPaths.Length == 0))
        {
            return;
        }

        EnsureSucceeded(
            () => _filters.EnableExcludeMode(appPaths, localIpv4Address, localIpv6Address),
            "SplitTunnel: Enabling exclude mode");
    }

    public void EnableIncludeMode(string[] appPaths, IPAddress serverIpv4Address, IPAddress serverIpv6Address)
    {
        if ((appPaths == null || appPaths.Length == 0))
        {
            return;
        }

        EnsureSucceeded(() => _filters.EnableIncludeMode(
            appPaths,
            serverIpv4Address,
            serverIpv6Address),
            "SplitTunnel: Enabling include mode");
    }

    public void Disable()
    {
        EnsureSucceeded(_filters.Disable, "SplitTunnel: Disabling");
    }

    private void EnsureSucceeded(System.Action action, string actionMessage)
    {
        try
        {
            action();
            _logger.Info<SplitTunnelLog>($"{actionMessage} succeeded");
        }
        catch (NetworkFilterException e)
        {
            _logger.Error<SplitTunnelLog>($"{actionMessage} failed. Error code: {e.Code}");
        }
    }
}