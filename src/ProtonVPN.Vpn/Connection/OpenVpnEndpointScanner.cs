/*
 * Copyright (c) 2021 Proton Technologies AG
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
using System.Threading;
using System.Threading.Tasks;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Networking;
using ProtonVPN.Common.Threading;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Vpn.Common;
using ProtonVPN.Vpn.OpenVpn;

namespace ProtonVPN.Vpn.Connection
{
    public class OpenVpnEndpointScanner : IEndpointScanner
    {
        private static readonly TimeSpan PingTimeout = TimeSpan.FromSeconds(3);

        private readonly ILogger _logger;
        private readonly ITaskQueue _taskQueue;
        private readonly PingableOpenVpnPort _pingableOpenVpnPort;

        public OpenVpnEndpointScanner(
            ILogger logger,
            ITaskQueue taskQueue,
            PingableOpenVpnPort pingableOpenVpnPort)
        {
            _logger = logger;
            _pingableOpenVpnPort = pingableOpenVpnPort;
            _taskQueue = taskQueue;
        }

        public async Task<IVpnEndpoint> ScanForBestEndpointAsync(IVpnEndpoint endpoint,
            IReadOnlyDictionary<VpnProtocol, IReadOnlyCollection<int>> ports, CancellationToken cancellationToken)
        {
            return await EnqueueAsync(() => ScanPortsAsync(endpoint as OpenVpnEndpoint, ports, cancellationToken), cancellationToken);
        }

        private async Task<OpenVpnEndpoint> EnqueueAsync(Func<Task<OpenVpnEndpoint>> func, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return OpenVpnEndpoint.Empty;
            }

            return await _taskQueue.Enqueue(async () =>
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return OpenVpnEndpoint.Empty;
                }

                return await func();
            });
        }

        private async Task<OpenVpnEndpoint> ScanPortsAsync(OpenVpnEndpoint endpoint,
            IReadOnlyDictionary<VpnProtocol, IReadOnlyCollection<int>> ports,
            CancellationToken cancellationToken)
        {
            OpenVpnEndpoint bestEndpoint = await BestEndpoint(EndpointCandidates(endpoint, ports, cancellationToken), cancellationToken);

            return HandleBestEndpoint(bestEndpoint, endpoint.Server);
        }

        private async Task<OpenVpnEndpoint> BestEndpoint(IList<Task<OpenVpnEndpoint>> candidates, CancellationToken cancellationToken)
        {
            OpenVpnEndpoint firstCandidate = null;
            OpenVpnEndpoint secondBestCandidate = null;

            while (candidates.Any())
            {
                Task<OpenVpnEndpoint> firstCompletedTask = await Task.WhenAny(candidates);
                candidates.Remove(firstCompletedTask);
                firstCandidate = await firstCompletedTask;

                if (cancellationToken.IsCancellationRequested ||
                    firstCandidate == null ||
                    (firstCandidate.VpnProtocol == VpnProtocol.OpenVpnUdp && firstCandidate.Port != 0))
                {
                    break;
                }

                if (firstCandidate.Port != 0)
                {
                    secondBestCandidate = firstCandidate;
                }
            }

            if (firstCandidate == null || firstCandidate.Port == 0)
            {
                firstCandidate = secondBestCandidate;
            }

            return firstCandidate ?? OpenVpnEndpoint.Empty;
        }

        private IList<Task<OpenVpnEndpoint>> EndpointCandidates(OpenVpnEndpoint endpoint,
            IReadOnlyDictionary<VpnProtocol, IReadOnlyCollection<int>> ports, CancellationToken cancellationToken)
        {
            return (from pair in ports
                where endpoint.VpnProtocol == VpnProtocol.Smart || endpoint.VpnProtocol == pair.Key
                from port in pair.Value
                select GetPortAlive(new OpenVpnEndpoint(endpoint.Server, pair.Key, port), cancellationToken)).ToList();
        }

        private async Task<OpenVpnEndpoint> GetPortAlive(OpenVpnEndpoint endpoint, CancellationToken cancellationToken)
        {
            _logger.Info($"Pinging VPN endpoint {endpoint.Server.Ip}:{endpoint.Port} for {endpoint.VpnProtocol} protocol.");
            Task timeoutTask = Task.Delay(PingTimeout, cancellationToken);
            bool isAlive = await _pingableOpenVpnPort.Alive(endpoint, timeoutTask);
            if (!isAlive)
            {
                await timeoutTask;
            }
            return isAlive ? endpoint : OpenVpnEndpoint.Empty;
        }

        private OpenVpnEndpoint HandleBestEndpoint(OpenVpnEndpoint bestEndpoint, VpnHost server)
        {
            bool isResponding = bestEndpoint.Port != 0;

            if (isResponding)
            {
                _logger.Info($"The endpoint {bestEndpoint.Server.Ip}:{bestEndpoint.Port} was the fastest to respond.");
                return bestEndpoint;
            }

            _logger.Info($"No VPN port has responded for {server.Ip}.");
            return OpenVpnEndpoint.Empty;
        }
    }
}