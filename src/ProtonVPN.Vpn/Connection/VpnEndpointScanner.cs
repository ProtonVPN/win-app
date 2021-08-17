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
using ProtonVPN.Vpn.LocalAgent;
using ProtonVPN.Vpn.OpenVpn;

namespace ProtonVPN.Vpn.Connection
{
    public class VpnEndpointScanner : IEndpointScanner
    {
        private static readonly TimeSpan PingTimeout = TimeSpan.FromSeconds(3);

        private readonly ILogger _logger;
        private readonly ITaskQueue _taskQueue;
        private readonly TcpPortScanner _tcpPortScanner;
        private readonly UdpPingClient _udpPingClient;

        public VpnEndpointScanner(
            ILogger logger,
            ITaskQueue taskQueue,
            TcpPortScanner tcpPortScanner,
            UdpPingClient udpPingClient)
        {
            _logger = logger;
            _taskQueue = taskQueue;
            _tcpPortScanner = tcpPortScanner;
            _udpPingClient = udpPingClient;
        }

        public async Task<VpnEndpoint> ScanForBestEndpointAsync(VpnEndpoint endpoint,
            IReadOnlyDictionary<VpnProtocol, IReadOnlyCollection<int>> ports, VpnProtocol preferredProtocol, CancellationToken cancellationToken)
        {
            return await EnqueueAsync(() => ScanPortsAsync(endpoint, ports, preferredProtocol, cancellationToken), cancellationToken);
        }

        private async Task<VpnEndpoint> EnqueueAsync(Func<Task<VpnEndpoint>> func, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return VpnEndpoint.Empty;
            }

            return await _taskQueue.Enqueue(async () =>
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return VpnEndpoint.Empty;
                }

                return await func();
            });
        }

        private async Task<VpnEndpoint> ScanPortsAsync(VpnEndpoint endpoint,
            IReadOnlyDictionary<VpnProtocol, IReadOnlyCollection<int>> ports,
            VpnProtocol preferredProtocol,
            CancellationToken cancellationToken)
        {
            VpnEndpoint bestEndpoint = await BestEndpoint(EndpointCandidates(endpoint, ports, cancellationToken), preferredProtocol, cancellationToken);

            return HandleBestEndpoint(bestEndpoint, endpoint.Server);
        }

        private async Task<VpnEndpoint> BestEndpoint(IList<Task<VpnEndpoint>> candidates, VpnProtocol preferredProtocol, CancellationToken cancellationToken)
        {
            VpnEndpoint firstCandidate = null;
            VpnEndpoint secondBestCandidate = null;
            VpnEndpoint thirdBestCandidate = null;

            while (candidates.Any())
            {
                Task<VpnEndpoint> firstCompletedTask = await Task.WhenAny(candidates);
                candidates.Remove(firstCompletedTask);
                firstCandidate = await firstCompletedTask;

                if (cancellationToken.IsCancellationRequested ||
                    firstCandidate == null ||
                    (firstCandidate.VpnProtocol == preferredProtocol && firstCandidate.Port != 0))
                {
                    break;
                }

                if (firstCandidate.Port != 0)
                {
                    if (firstCandidate.VpnProtocol == VpnProtocol.OpenVpnUdp)
                    {
                        secondBestCandidate = firstCandidate;
                    }
                    else
                    {
                        thirdBestCandidate = firstCandidate;
                    }
                }
            }

            if (firstCandidate == null || firstCandidate.Port == 0)
            {
                firstCandidate = secondBestCandidate ?? thirdBestCandidate;
            }

            return firstCandidate ?? VpnEndpoint.Empty;
        }

        private IList<Task<VpnEndpoint>> EndpointCandidates(VpnEndpoint endpoint,
            IReadOnlyDictionary<VpnProtocol, IReadOnlyCollection<int>> ports, CancellationToken cancellationToken)
        {
            List<Task<VpnEndpoint>> list = new List<Task<VpnEndpoint>>();
            foreach (KeyValuePair<VpnProtocol, IReadOnlyCollection<int>> pair in ports)
            {
                if (endpoint.Server.X25519PublicKey == null && (pair.Key == VpnProtocol.WireGuard || pair.Key == VpnProtocol.OpenVpnUdp))
                {
                    continue;
                }

                if (endpoint.VpnProtocol != VpnProtocol.Smart && endpoint.VpnProtocol != pair.Key)
                {
                    continue;
                }

                foreach (int port in pair.Value)
                {
                    list.Add(GetPortAlive(new VpnEndpoint(endpoint.Server, pair.Key, port), cancellationToken));
                }
            }

            return list;
        }

        private async Task<VpnEndpoint> GetPortAlive(VpnEndpoint endpoint, CancellationToken cancellationToken)
        {
            _logger.Info($"Pinging VPN endpoint {endpoint.Server.Ip}:{endpoint.Port} for {endpoint.VpnProtocol} protocol.");

            bool isAlive = false;
            switch (endpoint.VpnProtocol)
            {
                case VpnProtocol.OpenVpnTcp:
                    isAlive = await IsOpenVpnEndpointAlive(endpoint, cancellationToken);
                    break;
                case VpnProtocol.OpenVpnUdp:
                case VpnProtocol.WireGuard:
                    isAlive = await IsUdpEndpointAlive(endpoint, cancellationToken);
                    break;
            }

            return isAlive ? endpoint : VpnEndpoint.Empty;
        }

        private async Task<bool> IsOpenVpnEndpointAlive(VpnEndpoint endpoint, CancellationToken cancellationToken)
        {
            return await IsEndpointAlive(async timeoutTask => await _tcpPortScanner.Alive(endpoint, timeoutTask), cancellationToken);
        }

        private async Task<bool> IsUdpEndpointAlive(VpnEndpoint endpoint, CancellationToken cancellationToken)
        {
            return await IsEndpointAlive(async _ => await _udpPingClient.Ping(
                endpoint.Server.Ip,
                endpoint.Port,
                endpoint.Server.X25519PublicKey.Base64,
                cancellationToken), cancellationToken);
        }

        private async Task<bool> IsEndpointAlive(Func<Task, Task<bool>> func, CancellationToken cancellationToken)
        {
            Task timeoutTask = Task.Delay(PingTimeout, cancellationToken);
            bool isAlive = await func(timeoutTask);
            if (!isAlive)
            {
                await timeoutTask;
            }

            return isAlive;
        }

        private VpnEndpoint HandleBestEndpoint(VpnEndpoint bestEndpoint, VpnHost server)
        {
            bool isResponding = bestEndpoint.Port != 0;

            if (isResponding)
            {
                _logger.Info($"The endpoint {bestEndpoint.Server.Ip}:{bestEndpoint.Port} was the fastest to respond.");
                return bestEndpoint;
            }

            _logger.Info($"No VPN port has responded for {server.Ip}.");
            return VpnEndpoint.Empty;
        }
    }
}