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
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ProtonVPN.Common;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Networking;
using ProtonVPN.Common.OS.Services;
using ProtonVPN.Common.Threading;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Crypto;
using ProtonVPN.Vpn.Common;

namespace ProtonVPN.Vpn.WireGuard
{
    internal class WireGuardConnection : ISingleVpnConnection
    {
        private const string DefaultDnsServer = "10.2.0.1";
        private const string DefaultClientAddress = "10.2.0.2";

        private readonly ILogger _logger;
        private readonly ProtonVPN.Common.Configuration.Config _config;
        private readonly IService _wireGuardService;
        private readonly TrafficManager _trafficManager;
        private readonly StatusManager _statusManager;
        private readonly IX25519KeyGenerator _x25519KeyGenerator;
        private readonly SingleAction _connectAction;
        private readonly SingleAction _disconnectAction;

        private VpnError _lastVpnError;
        private VpnCredentials _credentials;
        private IVpnEndpoint _endpoint;
        private VpnConfig _vpnConfig;
        private bool _connected;
        private bool _isServiceStopPending;

        public WireGuardConnection(
            ILogger logger,
            ProtonVPN.Common.Configuration.Config config,
            IService wireGuardService,
            TrafficManager trafficManager,
            StatusManager statusManager,
            IX25519KeyGenerator x25519KeyGenerator)
        {
            _logger = logger;
            _config = config;
            _wireGuardService = wireGuardService;
            _trafficManager = trafficManager;
            _statusManager = statusManager;
            _x25519KeyGenerator = x25519KeyGenerator;
            _trafficManager.TrafficSent += OnTrafficSent;
            _statusManager.StateChanged += OnStateChanged;
            _connectAction = new SingleAction(ConnectAction);
            _connectAction.Completed += OnConnectActionCompleted;
            _disconnectAction = new SingleAction(DisconnectAction);
            _disconnectAction.Completed += OnDisconnectActionCompleted;
        }

        public event EventHandler<EventArgs<VpnState>> StateChanged;
        public InOutBytes Total { get; private set; } = InOutBytes.Zero;

        public void Connect(IVpnEndpoint endpoint, VpnCredentials credentials, VpnConfig config)
        {
            if (endpoint.Server.X25519PublicKey.IsNullOrEmpty())
            {
                InvokeStateChange(VpnStatus.Disconnected, VpnError.MissingServerPublicKey);
                return;
            }

            _credentials = credentials;
            _endpoint = endpoint;
            _vpnConfig = config;

            _connectAction.Run();
        }

        private async Task ConnectAction(CancellationToken cancellationToken)
        {
            _logger.Info("[WireGuardConnection] connect action started.");
            WriteConfig();
            InvokeStateChange(VpnStatus.Connecting);
            await EnsureServiceIsStopped(cancellationToken);
            _statusManager.Start();
            _trafficManager.Start();
            await StartWireGuardService(cancellationToken);
        }

        public void Disconnect(VpnError error)
        {
            _lastVpnError = error;
            _disconnectAction.Run();
        }

        private async Task StartWireGuardService(CancellationToken cancellationToken)
        {
            _logger.Info("[WireGuardConnection] starting service.");
            try
            {
                await _wireGuardService.StartAsync(cancellationToken);
            }
            catch (InvalidOperationException e)
            {
                _logger.Info("[WireGuardConnection] Failed to start WireGuard service: " + e.CombinedMessage());
            }
        }

        private async Task DisconnectAction(CancellationToken cancellationToken)
        {
            _logger.Info("[WireGuardConnection] Disconnect action started");
            InvokeStateChange(VpnStatus.Disconnecting, _lastVpnError);

            Task connectTask = _connectAction.Task;
            if (!connectTask.IsCompleted)
            {
                _connectAction.Cancel();
            }

            StopServiceDependencies();
            await EnsureServiceIsStopped(cancellationToken);
            _connected = false;
        }

        private void OnConnectActionCompleted(object sender, TaskCompletedEventArgs e)
        {
            _logger.Info("[WireGuardConnection] Connect action completed");
        }

        private void OnDisconnectActionCompleted(object sender, TaskCompletedEventArgs e)
        {
            _logger.Info("[WireGuardConnection] Disconnect action completed");
            InvokeStateChange(VpnStatus.Disconnected, _lastVpnError);
            _lastVpnError = VpnError.None;
        }

        public void SetFeatures(VpnFeatures vpnFeatures) => throw new NotSupportedException();

        public void UpdateAuthCertificate(string certificate) => throw new NotSupportedException();

        private void OnTrafficSent(object sender, InOutBytes total)
        {
            Total = total;
        }

        private async Task EnsureServiceIsStopped(CancellationToken cancellationToken)
        {
            while (_wireGuardService.Exists() && !_wireGuardService.IsStopped())
            {
                if (_isServiceStopPending)
                {
                    _logger.Info("[WireGuardConnection] waiting for service to stop.");
                    await Task.Delay(500, cancellationToken);
                }
                else
                {
                    _logger.Info("[WireGuardConnection] service is running, trying to stop.");
                    await _wireGuardService.StopAsync(cancellationToken);
                    _isServiceStopPending = true;
                }
            }

            if (_isServiceStopPending)
            {
                _logger.Info("[WireGuardConnection] service is stopped.");
                _isServiceStopPending = false;
            }
        }

        private void OnStateChanged(object sender, EventArgs<VpnState> state)
        {
            if (_connected && state.Data.Status == VpnStatus.Connected)
            {
                return;
            }

            if (state.Data.Status == VpnStatus.Disconnected)
            {
                StopServiceDependencies();
            }

            _connected = state.Data.Status == VpnStatus.Connected;
            InvokeStateChange(state.Data.Status, state.Data.Error);
        }

        private void StopServiceDependencies()
        {
            _trafficManager.Stop();
            _statusManager.Stop();
        }

        private void WriteConfig()
        {
            CreateConfigDirectoryPathIfNotExists();
            string template = CreateConfigString();
            File.WriteAllText(_config.WireGuard.ConfigFilePath, template);
        }

        private void CreateConfigDirectoryPathIfNotExists()
        {
            string directoryPath = Path.GetDirectoryName(_config.WireGuard.ConfigFilePath);
            if (directoryPath != null)
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        private string CreateConfigString()
        {
            SecretKey x25519SecretKey = GetX25519SecretKey();
            return
                $"[Interface]\n" +
                $"PrivateKey = {x25519SecretKey.Base64}\n" +
                $"Address = {DefaultClientAddress}/32\n" +
                $"DNS = {GetDnsServers()}\n" +
                $"[Peer]\n" +
                $"PublicKey = {_endpoint.Server.X25519PublicKey}\n" +
                $"AllowedIPs = 0.0.0.0/0\n" +
                $"Endpoint = {_endpoint.Server.Ip}:{_endpoint.Port}\n";
        }

        private SecretKey GetX25519SecretKey()
        {
            return _x25519KeyGenerator.FromEd25519SecretKey(_credentials.ClientKeyPair.SecretKey);
        }

        private void InvokeStateChange(VpnStatus status, VpnError error = VpnError.None)
        {
            StateChanged?.Invoke(this,
                new EventArgs<VpnState>(
                    new VpnState(status, error, DefaultClientAddress, _endpoint?.Server.Ip ?? string.Empty,
                        VpnProtocol.WireGuard, null, _endpoint?.Server.Label ?? string.Empty)));
        }

        private string GetDnsServers()
        {
            return _vpnConfig.CustomDns.Count > 0 ? string.Join(",", _vpnConfig.CustomDns) : DefaultDnsServer;
        }
    }
}