﻿/*
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
using System.Threading;
using System.Threading.Tasks;
using ProtonVPN.Common;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.ConnectLogs;
using ProtonVPN.Common.Networking;
using ProtonVPN.Common.Threading;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Vpn.Common;

namespace ProtonVPN.Vpn.Connection
{
    public class BestPortWrapper : ISingleVpnConnection
    {
        private static readonly TimeSpan DisconnectDelay = TimeSpan.FromSeconds(5);

        private readonly ILogger _logger;
        private readonly ITaskQueue _taskQueue;
        private readonly IEndpointScanner _endpointScanner;
        private readonly ISingleVpnConnection _origin;
        private readonly CancellationHandle _cancellationHandle = new();

        private VpnEndpoint _vpnEndpoint = new();
        private VpnCredentials _vpnCredentials;
        private VpnConfig _config;
        private Task _disconnectDelay = Task.CompletedTask;

        public BestPortWrapper(
            ILogger logger,
            ITaskQueue taskQueue,
            IEndpointScanner endpointScanner,
            ISingleVpnConnection origin)
        {
            _logger = logger;
            _taskQueue = taskQueue;
            _endpointScanner = endpointScanner;
            _origin = origin;
            _origin.StateChanged += Origin_StateChanged;
        }

        public event EventHandler<EventArgs<VpnState>> StateChanged;
        public event EventHandler<ConnectionDetails> ConnectionDetailsChanged
        {
            add => _origin.ConnectionDetailsChanged += value;
            remove => _origin.ConnectionDetailsChanged -= value;
        }

        public InOutBytes Total => _origin.Total;

        public void Connect(VpnEndpoint endpoint, VpnCredentials credentials, VpnConfig config)
        {
            _vpnEndpoint = endpoint;
            _vpnCredentials = credentials;
            _config = config;

            _cancellationHandle.Cancel();
            CancellationToken cancellationToken = _cancellationHandle.Token;
            _disconnectDelay = Task.Delay(DisconnectDelay, cancellationToken);

            Queued(ScanPorts, cancellationToken);
        }

        public void Disconnect(VpnError error)
        {
            _cancellationHandle.Cancel();
            _origin.Disconnect(error);
        }

        public void SetFeatures(VpnFeatures vpnFeatures)
        {
            _origin.SetFeatures(vpnFeatures);
        }

        public void UpdateAuthCertificate(string certificate)
        {
            _origin.UpdateAuthCertificate(certificate);
        }

        public void RequestNetShieldStats()
        {
            _origin.RequestNetShieldStats();
        }

        private async void ScanPorts(CancellationToken cancellationToken)
        {
            _logger.Info<ConnectScanLog>($"Starting port scanning of endpoint {_vpnEndpoint.Server.Ip} before connection.");
            VpnEndpoint bestEndpoint = await _endpointScanner.ScanForBestEndpointAsync(
                _vpnEndpoint, _config.Ports, _config.PreferredProtocols, _cancellationHandle.Token);

            Queued(ct => HandleBestEndpoint(bestEndpoint, ct), cancellationToken);
        }

        private void HandleBestEndpoint(VpnEndpoint endpoint, CancellationToken cancellationToken)
        {
            if (endpoint.Port != 0)
            {
                _vpnEndpoint = endpoint;
                _logger.Info<ConnectScanResultLog>($"Connecting to {endpoint.Server.Ip}:{endpoint.Port} " +
                    $"with protocol {endpoint.VpnProtocol} as it responded fastest.");
                _origin.Connect(endpoint, GetCredentials(endpoint), GetConfig(endpoint.VpnProtocol));
            }
            else
            {
                _logger.Info<ConnectScanFailLog>("Disconnecting, as none of the VPN ports responded.");
                DelayedDisconnect(cancellationToken);
            }
        }

        private VpnConfig GetConfig(VpnProtocol vpnProtocol)
        {
            return new(new VpnConfigParameters
            {
                CustomDns = _config.CustomDns,
                NetShieldMode = _config.NetShieldMode,
                OpenVpnAdapter = _config.OpenVpnAdapter,
                ModerateNat = _config.ModerateNat,
                Ports = _config.Ports,
                SplitTcp = _config.SplitTcp,
                SplitTunnelIPs = _config.SplitTunnelIPs,
                SplitTunnelMode = _config.SplitTunnelMode,
                VpnProtocol = vpnProtocol,
                AllowNonStandardPorts = _config.AllowNonStandardPorts,
                PortForwarding = _config.PortForwarding,
            });
        }

        private VpnCredentials GetCredentials(VpnEndpoint endpoint)
        {
            if (string.IsNullOrEmpty(endpoint.Server.Label))
            {
                return _vpnCredentials;
            }

            string username = $"{_vpnCredentials.Username}+b:{endpoint.Server.Label}";

            return _vpnCredentials.ClientCertPem.IsNullOrEmpty() || _vpnCredentials.ClientKeyPair == null
                ? new VpnCredentials(username, _vpnCredentials.Password)
                : new VpnCredentials(_vpnCredentials.ClientCertPem, _vpnCredentials.ClientKeyPair);
        }

        private async void DelayedDisconnect(CancellationToken cancellationToken)
        {
            try
            {
                // Delay invocation of StateChanged(Disconnected) at least for DisconnectDelay duration after Connect
                // request.
                await _disconnectDelay;
            }
            catch (TaskCanceledException)
            {
                return;
            }

            Queued(_ => InvokeDisconnected(), cancellationToken);
        }

        private void InvokeDisconnected()
        {
            StateChanged?.Invoke(this,
                new EventArgs<VpnState>(new VpnState(
                    VpnStatus.Disconnected,
                    VpnError.PingTimeoutError,
                    string.Empty,
                    _vpnEndpoint.Server.Ip,
                    _config.VpnProtocol,
                    portForwarding: false,
                    _config.OpenVpnAdapter,
                    _vpnEndpoint.Server.Label)));
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
            StateChanged?.Invoke(this, e);
        }
    }
}