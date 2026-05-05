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

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Common.Legacy.Vpn;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.ConnectLogs;
using ProtonVPN.Vpn.Common;
using ProtonVPN.Vpn.Connection;
using ProtonVPN.Vpn.ServerValidation;

namespace ProtonVPN.Service.Vpn;

internal class ConnectionProbe : IConnectionProbe
{
    private readonly ILogger _logger;
    private readonly IEndpointScanner _endpointScanner;
    private readonly IServerValidator _serverValidator;

    public ConnectionProbe(
        ILogger logger,
        IEndpointScanner endpointScanner,
        IServerValidator serverValidator)
    {
        _logger = logger;
        _endpointScanner = endpointScanner;
        _serverValidator = serverValidator;
    }

    public async Task<ProbeAvailabilityResult> ProbeAvailabilityAsync(
        IVpnEndpointCandidates candidates,
        VpnConfig config,
        CancellationToken cancellationToken)
    {
        VpnError lastError = VpnError.None;

        while (!cancellationToken.IsCancellationRequested)
        {
            VpnEndpoint endpoint = candidates.NextIp(config);
            if (endpoint.IsEmpty)
            {
                break;
            }

            VpnError validationError = _serverValidator.Validate(endpoint.Server);
            if (validationError != VpnError.None)
            {
                lastError = validationError;
                _logger.Warn<ConnectLog>($"Server validation failed for IP {endpoint.Server.Ip} and Label ({endpoint.Server.Label}).");
                continue;
            }

            VpnEndpoint bestEndpoint = await _endpointScanner.ScanForBestEndpointAsync(
                endpoint,
                config.Ports,
                config.PreferredProtocols.ToList(),
                cancellationToken);

            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            if (bestEndpoint.Port != 0)
            {
                _logger.Info<ConnectLog>($"At least one server has responded to a ping. Attempting connections. ({bestEndpoint.Server.Ip}:{bestEndpoint.Port})");
                return new ProbeAvailabilityResult(true, VpnError.None);
            }
        }

        return new ProbeAvailabilityResult(false, lastError == VpnError.None ? VpnError.PingTimeoutError : lastError);
    }

    public async Task<VpnEndpoint> SelectEndpointAsync(
        IVpnEndpointCandidates candidates,
        VpnConfig config,
        CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            VpnEndpoint endpoint = candidates.NextIp(config);
            if (endpoint.IsEmpty)
            {
                break;
            }

            VpnEndpoint bestEndpoint = await _endpointScanner.ScanForBestEndpointAsync(
                endpoint,
                config.Ports,
                config.PreferredProtocols.ToList(),
                cancellationToken);

            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            if (bestEndpoint.Port != 0)
            {
                return bestEndpoint;
            }
        }

        return VpnEndpoint.Empty;
    }
}