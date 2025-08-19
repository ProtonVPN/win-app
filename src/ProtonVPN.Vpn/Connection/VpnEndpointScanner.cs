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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Common.Legacy.Threading;
using ProtonVPN.Common.Legacy.Vpn;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.ConnectLogs;
using ProtonVPN.Vpn.Common;
using ProtonVPN.Vpn.PortScanning;

namespace ProtonVPN.Vpn.Connection;

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
        IReadOnlyDictionary<VpnProtocol, IReadOnlyCollection<int>> ports, IList<VpnProtocol> preferredProtocols,
        CancellationToken cancellationToken)
    {
        return await EnqueueAsync(() => ScanPortsAsync(endpoint, ports, preferredProtocols, cancellationToken), cancellationToken);
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
        IList<VpnProtocol> preferredProtocols,
        CancellationToken cancellationToken)
    {
        IList<Task<VpnEndpoint>> candidates = EndpointCandidates(endpoint, ports, preferredProtocols, cancellationToken);
        VpnEndpoint bestEndpoint = await BestEndpointAsync(candidates, preferredProtocols, cancellationToken);

        return HandleBestEndpoint(bestEndpoint, endpoint.Server);
    }

    private async Task<VpnEndpoint> BestEndpointAsync(IList<Task<VpnEndpoint>> candidates,
        IList<VpnProtocol> preferredProtocols, CancellationToken cancellationToken)
    {
        Dictionary<VpnProtocol, VpnEndpoint> endpointsByProtocol = GetEndpointsByProtocol(preferredProtocols);

        while (candidates.Any())
        {
            Task<VpnEndpoint> firstCompletedTask = await Task.WhenAny(candidates);
            candidates.Remove(firstCompletedTask);
            VpnEndpoint candidate = await firstCompletedTask;

            if (cancellationToken.IsCancellationRequested || candidate == null)
            {
                break;
            }

            if (candidate.Port != 0)
            {
                endpointsByProtocol[candidate.VpnProtocol] = candidate;
                if (candidate.VpnProtocol == preferredProtocols.First())
                {
                    break;
                }
            }
        }

        foreach (VpnProtocol preferredProtocol in preferredProtocols)
        {
            if (endpointsByProtocol[preferredProtocol] != null)
            {
                return endpointsByProtocol[preferredProtocol];
            }
        }

        return VpnEndpoint.Empty;
    }

    private Dictionary<VpnProtocol, VpnEndpoint> GetEndpointsByProtocol(IList<VpnProtocol> preferredProtocols)
    {
        Dictionary<VpnProtocol, VpnEndpoint> endpoints = new Dictionary<VpnProtocol, VpnEndpoint>();
        foreach (VpnProtocol protocol in preferredProtocols)
        {
            endpoints.Add(protocol, null);
        }

        return endpoints;
    }

    private IList<Task<VpnEndpoint>> EndpointCandidates(
        VpnEndpoint endpoint,
        IReadOnlyDictionary<VpnProtocol, IReadOnlyCollection<int>> ports,
        IList<VpnProtocol> preferredProtocols,
        CancellationToken cancellationToken)
    {
        List<Task<VpnEndpoint>> list = new List<Task<VpnEndpoint>>();
        foreach (VpnProtocol preferredProtocol in preferredProtocols)
        {
            if (!ports.ContainsKey(preferredProtocol) ||
                (endpoint.Server.X25519PublicKey == null && preferredProtocol is VpnProtocol.WireGuardUdp or VpnProtocol.OpenVpnUdp))
            {
                continue;
            }

            string ip = endpoint.Server.GetIp(preferredProtocol);

            if (string.IsNullOrWhiteSpace(ip))
            {
                _logger.Info<ConnectScanLog>($"There is no entry IP for {preferredProtocol} protocol.");
            }
            else
            {
                foreach (int port in ports[preferredProtocol])
                {
                    list.Add(GetPortAliveAsync(ip, endpoint.Server, preferredProtocol, port, cancellationToken));
                }
            }
        }

        return list;

    }

    private async Task<VpnEndpoint> GetPortAliveAsync(string ip, VpnHost server, VpnProtocol protocol, int port,
        CancellationToken cancellationToken)
    {
        _logger.Info<ConnectScanLog>($"Pinging VPN endpoint {ip}:{port} for {protocol} protocol.");

        bool isAlive = false;
        switch (protocol)
        {
            case VpnProtocol.OpenVpnTcp:
            case VpnProtocol.WireGuardTcp:
            case VpnProtocol.WireGuardTls:
                isAlive = await IsTcpEndpointAliveAsync(ip, port, cancellationToken);
                break;
            case VpnProtocol.OpenVpnUdp:
            case VpnProtocol.WireGuardUdp:
                isAlive = await IsUdpEndpointAliveAsync(ip, port, server.X25519PublicKey.Base64, cancellationToken);
                break;
        }

        return isAlive ? new VpnEndpoint(new VpnHost(server.Name, ip, server.Label, server.X25519PublicKey, server.Signature, server.IsIpv6Supported, null),
            protocol, port) : VpnEndpoint.Empty;
    }

    private async Task<bool> IsTcpEndpointAliveAsync(string ip, int port, CancellationToken cancellationToken)
    {
        return await IsEndpointAliveAsync(async timeoutTask =>
            await _tcpPortScanner.IsAliveAsync(ip, port, timeoutTask), cancellationToken);
    }

    private async Task<bool> IsUdpEndpointAliveAsync(string ip, int port, string serverKeyBase64,
        CancellationToken cancellationToken)
    {
        return await IsEndpointAliveAsync(async timeoutTask =>
            await _udpPingClient.PingAsync(ip, port, serverKeyBase64, timeoutTask), cancellationToken);
    }

    private async Task<bool> IsEndpointAliveAsync(Func<Task, Task<bool>> func, CancellationToken cancellationToken)
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
            _logger.Info<ConnectScanResultLog>($"The endpoint {bestEndpoint.Server.Ip}:{bestEndpoint.Port} " +
                $"with protocol {bestEndpoint.VpnProtocol} was the fastest to respond.");
            return bestEndpoint;
        }

        _logger.Info<ConnectScanFailLog>($"No VPN port has responded for {server.Ip}.");
        return VpnEndpoint.Empty;
    }
}