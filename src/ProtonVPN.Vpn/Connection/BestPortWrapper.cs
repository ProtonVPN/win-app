/*
 * Copyright (c) 2020 Proton Technologies AG
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
using ProtonVPN.Common;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Threading;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Vpn.Common;
using ProtonVPN.Vpn.OpenVpn;

namespace ProtonVPN.Vpn.Connection
{
    internal class BestPortWrapper : ISingleVpnConnection
    {
        private static readonly TimeSpan EndpointTimeout = TimeSpan.FromSeconds(3);
        private static readonly TimeSpan DisconnectDelay = TimeSpan.FromSeconds(5);

        private readonly ISingleVpnConnection _origin;
        private readonly ILogger _logger;
        private readonly PingableOpenVpnPort _pingableOpenVpnPort;
        private readonly ITaskQueue _taskQueue;
        private readonly CancellationHandle _cancellationHandle = new CancellationHandle();

        private VpnEndpoint _vpnEndpoint = VpnEndpoint.EmptyEndpoint;
        private VpnCredentials _vpnCredentials;
        private VpnConfig _config;
        private Task _disconnectDelay = Task.CompletedTask;

        public BestPortWrapper(
            ILogger logger,
            ITaskQueue taskQueue,
            PingableOpenVpnPort pingableOpenVpnPort,
            ISingleVpnConnection origin)
        {
            _logger = logger;
            _pingableOpenVpnPort = pingableOpenVpnPort;
            _origin = origin;
            _taskQueue = taskQueue;
            _origin.StateChanged += Origin_StateChanged;
        }

        public event EventHandler<EventArgs<VpnState>> StateChanged;

        public InOutBytes Total => _origin.Total;

        public void Connect(VpnEndpoint endpoint, VpnCredentials credentials, VpnConfig config)
        {
            _vpnEndpoint = endpoint;
            _vpnCredentials = credentials;
            _config = config;

            _cancellationHandle.Cancel();
            var cancellationToken = _cancellationHandle.Token;
            _disconnectDelay = Task.Delay(DisconnectDelay, cancellationToken);

            Queued(ScanPorts, cancellationToken);
        }

        public void Disconnect(VpnError error)
        {
            _cancellationHandle.Cancel();
            _origin.Disconnect(error);
        }

        private async void ScanPorts(CancellationToken cancellationToken)
        {
            InvokeConnecting();

            var endpoint = await BestEndpoint(_vpnEndpoint.Server, _vpnEndpoint.Protocol, cancellationToken);

            Queued(ct => HandleBestEndpoint(endpoint, ct), cancellationToken);
        }

        private void HandleBestEndpoint(VpnEndpoint endpoint, CancellationToken cancellationToken)
        {
            if (endpoint.Port != 0)
            {
                _vpnEndpoint = endpoint;
                _origin.Connect(endpoint, _vpnCredentials, _config);
            }
            else
            {
                DelayedDisconnect(cancellationToken);
            }
        }

        private async void DelayedDisconnect(CancellationToken cancellationToken)
        {
            try
            {
                // Delay invocation of StateChanged(Disconnected) at least for DisconnectDelay duration after Connect request.
                await _disconnectDelay;
            }
            catch (TaskCanceledException)
            {
                return;
            }

            Queued(_ => InvokeDisconnected(), cancellationToken);
        }

        private void InvokeConnecting()
        {
            StateChanged?.Invoke(this,
                new EventArgs<VpnState>(new VpnState(
                    VpnStatus.Connecting,
                    VpnError.None,
                    string.Empty,
                    _vpnEndpoint.Server.Ip)));
        }

        private void InvokeDisconnected()
        {
            StateChanged?.Invoke(this,
                new EventArgs<VpnState>(new VpnState(
                    VpnStatus.Disconnected,
                    VpnError.TimeoutError,
                    string.Empty,
                    _vpnEndpoint.Server.Ip)));
        }

        private void Queued(Action<CancellationToken> action, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            _taskQueue.Enqueue(() =>
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                action(cancellationToken);
            });
        }

        private void Origin_StateChanged(object sender, EventArgs<VpnState> e)
        {
            var state = new VpnState(
                e.Data.Status,
                e.Data.Error,
                e.Data.LocalIp,
                e.Data.RemoteIp,
                _vpnEndpoint.Protocol);

            StateChanged?.Invoke(this, new EventArgs<VpnState>(state));
        }

        private async Task<VpnEndpoint> BestEndpoint(VpnHost server, VpnProtocol protocol, CancellationToken cancellationToken)
        {
            switch (protocol)
            {
                case VpnProtocol.Auto:
                    var bestEndpoint = await BestEndpoint(EndpointCandidates(server, VpnProtocol.OpenVpnUdp));
                    if (bestEndpoint.Port == 0 && !cancellationToken.IsCancellationRequested)
                    {
                        bestEndpoint = await BestEndpoint(EndpointCandidates(server, VpnProtocol.OpenVpnTcp));
                    }
                    return bestEndpoint;
                default:
                    return await BestEndpoint(EndpointCandidates(server, protocol));
            }
        }

        private async Task<VpnEndpoint> BestEndpoint(IReadOnlyList<Task<VpnEndpoint>> candidates)
        {
            var result = await Task.WhenAll(candidates);
            var aliveCandidates = result.Where(c => c.Port > 0).ToList();
            if (aliveCandidates.Count > 0)
            {
                var rnd = new Random();
                return aliveCandidates[rnd.Next(aliveCandidates.Count)];
            }

            return VpnEndpoint.EmptyEndpoint;
        }

        private IReadOnlyList<Task<VpnEndpoint>> EndpointCandidates(VpnHost server, VpnProtocol protocol)
        {
            _logger.Info($"Pinging VPN server {server.Ip} for {protocol} protocol");

            var timeoutTask = Task.Delay(EndpointTimeout);

            return (from pair in _config.Ports
                where protocol == VpnProtocol.Auto || protocol == pair.Key
                from port in pair.Value
                select GetPortAlive(new VpnEndpoint(server, pair.Key, port), timeoutTask)).ToList();
        }

        private async Task<VpnEndpoint> GetPortAlive(VpnEndpoint endpoint, Task timeoutTask)
        {
            var alive = await _pingableOpenVpnPort.Alive(endpoint, timeoutTask);
            return alive ? endpoint : VpnEndpoint.EmptyEndpoint;
        }
    }
}
