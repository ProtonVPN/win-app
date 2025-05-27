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
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using ProtonVPN.Common.Legacy;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Common.Legacy.Threading;
using ProtonVPN.Common.Legacy.Vpn;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Crypto.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppServiceLogs;
using ProtonVPN.Logging.Contracts.Events.ConnectLogs;
using ProtonVPN.Logging.Contracts.Events.DisconnectLogs;
using ProtonVPN.Vpn.Common;
using ProtonVPN.Vpn.Gateways;
using Timer = System.Timers.Timer;

namespace ProtonVPN.Vpn.WireGuard;

public class WireGuardConnection : IAdapterSingleVpnConnection
{
    private const int MIN_CONNECTION_TIMEOUT = 5000;
    private const int MAX_CONNECTION_TIMEOUT = 30000;

    private readonly ILogger _logger;
    private readonly IConfiguration _config;
    private readonly IGatewayCache _gatewayCache;
    private readonly Timer _serviceHealthCheckTimer = new();
    private readonly IWireGuardService _wireGuardService;
    private readonly NtTrafficManager _ntTrafficManager;
    private readonly WintunTrafficManager _wintunTrafficManager;
    private readonly StatusManager _statusManager;
    private readonly IX25519KeyGenerator _x25519KeyGenerator;
    private readonly SingleAction _connectAction;
    private readonly SingleAction _disconnectAction;

    private VpnError _lastVpnError;
    private VpnCredentials _credentials;
    private VpnEndpoint _endpoint;
    private VpnConfig _vpnConfig;
    private bool _isConnected;
    private bool _isServiceStopPending;
    private VpnStatus _vpnStatus;
    private CancellationTokenSource _disconnectCancellationTokenSource;

    public WireGuardConnection(
        ILogger logger,
        IConfiguration config,
        IGatewayCache gatewayCache,
        IWireGuardService wireGuardService,
        NtTrafficManager ntTrafficManager,
        WintunTrafficManager wintunTrafficManager,
        StatusManager statusManager,
        IX25519KeyGenerator x25519KeyGenerator)
    {
        _logger = logger;
        _config = config;
        _gatewayCache = gatewayCache;
        _wireGuardService = wireGuardService;
        _ntTrafficManager = ntTrafficManager;
        _wintunTrafficManager = wintunTrafficManager;
        _statusManager = statusManager;
        _x25519KeyGenerator = x25519KeyGenerator;

        _ntTrafficManager.TrafficSent += OnTrafficSent;
        _wintunTrafficManager.TrafficSent += OnTrafficSent;
        _statusManager.StateChanged += OnStateChanged;
        _connectAction = new SingleAction(ConnectAction);
        _connectAction.Completed += OnConnectActionCompleted;
        _disconnectAction = new SingleAction(DisconnectAction);
        _disconnectAction.Completed += OnDisconnectActionCompleted;
        _serviceHealthCheckTimer.Interval = config.ServiceCheckInterval.TotalMilliseconds;
        _serviceHealthCheckTimer.Elapsed += CheckIfServiceIsRunning;
    }

    public event EventHandler<EventArgs<VpnState>> StateChanged;
    public event EventHandler<ConnectionDetails> ConnectionDetailsChanged;
    public NetworkTraffic NetworkTraffic { get; private set; } = NetworkTraffic.Zero;

    public void Connect(VpnEndpoint endpoint, VpnCredentials credentials, VpnConfig config)
    {
        _credentials = credentials;
        _endpoint = endpoint;
        _vpnConfig = config;

        _connectAction.Run();
    }

    private async Task ConnectAction(CancellationToken cancellationToken)
    {
        _logger.Info<ConnectStartLog>("Connect action started.");
        WriteConfig();
        UpdateGatewayCache();
        InvokeStateChange(VpnStatus.Connecting);
        await EnsureServiceIsStopped(cancellationToken);
        _statusManager.Start();
        await StartWireGuardService(cancellationToken);

        CancellationToken linkedCancellationToken = CreateLinkedCancellationToken(cancellationToken);
        int timeout = Math.Clamp((int)_vpnConfig.WireGuardConnectionTimeout.TotalMilliseconds, MIN_CONNECTION_TIMEOUT, MAX_CONNECTION_TIMEOUT);
        await Task.Delay(timeout, linkedCancellationToken);
        if (!_isConnected)
        {
            _logger.Warn<ConnectLog>($"{timeout}ms timeout reached, disconnecting.");
            Disconnect(VpnError.AdapterTimeoutError);
        }
    }

    private CancellationToken CreateLinkedCancellationToken(CancellationToken cancellationToken)
    {
        CancelDisconnectCancellationToken();
        _disconnectCancellationTokenSource = new CancellationTokenSource();
        CancellationTokenSource childCancellationTokenSource =
            CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _disconnectCancellationTokenSource.Token);
        return childCancellationTokenSource.Token;
    }

    private void CancelDisconnectCancellationToken()
    {
        _disconnectCancellationTokenSource?.Cancel();
    }

    private void UpdateGatewayCache()
    {
        _gatewayCache.Save(IPAddress.Parse("10.2.0.1"));
    }

    public void Disconnect(VpnError error)
    {
        _lastVpnError = error;
        _disconnectAction.Run();
    }

    private async Task StartWireGuardService(CancellationToken cancellationToken)
    {
        _logger.Info<AppServiceStartLog>("Starting service.");
        try
        {
            await _wireGuardService.StartAsync(cancellationToken, _vpnConfig.VpnProtocol);
        }
        catch (InvalidOperationException e)
        {
            _logger.Error<AppServiceStartFailedLog>("Failed to start WireGuard service: ", e);
        }
    }

    private async Task DisconnectAction(CancellationToken cancellationToken)
    {
        _logger.Info<DisconnectLog>("Disconnect action started.");
        if (_vpnStatus is not VpnStatus.Disconnected)
        {
            InvokeStateChange(VpnStatus.Disconnecting, _lastVpnError);
        }

        Task connectTask = _connectAction.Task;
        if (!connectTask.IsCompleted)
        {
            await _connectAction.Task;
        }

        _serviceHealthCheckTimer.Stop();
        StopServiceDependencies();
        await EnsureServiceIsStopped(cancellationToken);
        _isConnected = false;
        CancelDisconnectCancellationToken();
    }

    private void OnConnectActionCompleted(object sender, TaskCompletedEventArgs e)
    {
        _logger.Info<ConnectLog>("Connect action completed.");
    }

    private void OnDisconnectActionCompleted(object sender, TaskCompletedEventArgs e)
    {
        _logger.Info<DisconnectLog>("Disconnect action completed.");
        InvokeStateChange(VpnStatus.Disconnected, _lastVpnError);
        _lastVpnError = VpnError.None;
    }

    private void OnTrafficSent(object sender, NetworkTraffic total)
    {
        NetworkTraffic = total;
    }

    private async Task EnsureServiceIsStopped(CancellationToken cancellationToken)
    {
        while (_wireGuardService.Exists() && !_wireGuardService.IsStopped())
        {
            if (_isServiceStopPending)
            {
                _logger.Debug<AppServiceStopLog>("Waiting for WireGuard service to stop.");
                await Task.Delay(100, cancellationToken);
            }
            else
            {
                _logger.Info<AppServiceStopLog>("WireGuard service is running, trying to stop.");
                await _wireGuardService.StopAsync(cancellationToken);
                _isServiceStopPending = true;
            }
        }

        if (_isServiceStopPending)
        {
            _logger.Info<AppServiceStopLog>("WireGuard service is stopped.");
            _isServiceStopPending = false;
        }
    }

    private void OnStateChanged(object sender, EventArgs<VpnState> state)
    {
        switch (state.Data.Status)
        {
            case VpnStatus.Connected:
                OnVpnConnected(state);
                break;
            case VpnStatus.Disconnected:
                OnVpnDisconnected(state);
                NetworkTraffic = NetworkTraffic.Zero;
                break;
        }
    }

    private void OnVpnConnected(EventArgs<VpnState> state)
    {
        if (!_isConnected)
        {
            _isConnected = true;
            StartTrafficManager();
            _serviceHealthCheckTimer.Start();
            UpdateGatewayCache();
            _logger.Info<ConnectConnectedLog>("Connected state received and decorated by WireGuard.");
            InvokeStateChange(VpnStatus.Connected, state.Data.Error);
        }
    }

    private void StartTrafficManager()
    {
        if (_vpnConfig.VpnProtocol == VpnProtocol.WireGuardUdp)
        {
            _ntTrafficManager.Start();
        }
        else
        {
            _wintunTrafficManager.Start();
        }
    }

    private void OnVpnDisconnected(EventArgs<VpnState> state)
    {
        if (state.Data.Error is VpnError.Unknown or VpnError.InterfaceHasForwardingEnabled)
        {
            Disconnect(state.Data.Error);
            return;
        }

        _isConnected = false;
        _serviceHealthCheckTimer.Stop();
        StopServiceDependencies();
        InvokeStateChange(VpnStatus.Disconnected, state.Data.Error);
        CancelDisconnectCancellationToken();
    }

    private void StopServiceDependencies()
    {
        _ntTrafficManager.Stop();
        _wintunTrafficManager.Stop();
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
            $"Address = {_config.WireGuard.DefaultClientAddress}/32\n" +
            $"DNS = {GetDnsServers()}\n" +
            $"[Peer]\n" +
            $"PublicKey = {_endpoint.Server.X25519PublicKey.Base64}\n" +
            $"AllowedIPs = 0.0.0.0/0\n" +
            $"Endpoint = {_endpoint.Server.Ip}:{_endpoint.Port}\n";
    }

    private SecretKey GetX25519SecretKey()
    {
        return _x25519KeyGenerator.FromEd25519SecretKey(_credentials.ClientKeyPair.SecretKey);
    }

    private void InvokeStateChange(VpnStatus status, VpnError error = VpnError.None)
    {
        _vpnStatus = status;
        VpnState vpnState = CreateVpnState(status, error);
        StateChanged?.Invoke(this, new EventArgs<VpnState>(vpnState));
    }

    private VpnState CreateVpnState(VpnStatus status, VpnError error)
    {
        if (_vpnConfig is null)
        {
            return new VpnState(status, error, _config.WireGuard.DefaultClientAddress,
                _endpoint?.Server.Ip ?? string.Empty, _endpoint?.Port ?? 0, VpnProtocol.WireGuardUdp,
                openVpnAdapter: null, label: _endpoint?.Server.Label ?? string.Empty);
        }

        return new VpnState(status, error, _config.WireGuard.DefaultClientAddress,
            _endpoint?.Server.Ip ?? string.Empty, _endpoint?.Port ?? 0, _vpnConfig.VpnProtocol,
            _vpnConfig.PortForwarding, null, _endpoint?.Server.Label ?? string.Empty);
    }

    private string GetDnsServers()
    {
        return _vpnConfig.CustomDns.Count > 0
            ? string.Join(",", _vpnConfig.CustomDns)
            : _config.WireGuard.DefaultDnsServer;
    }

    private void CheckIfServiceIsRunning(object sender, ElapsedEventArgs e)
    {
        if (_isConnected && !_wireGuardService.Running() && !_disconnectAction.IsRunning)
        {
            _logger.Info<DisconnectTriggerLog>($"The service {_wireGuardService.Name} is not running. " +
                         "Disconnecting with VpnError.Unknown to get reconnected.");
            Disconnect(VpnError.Unknown);
        }
    }
}