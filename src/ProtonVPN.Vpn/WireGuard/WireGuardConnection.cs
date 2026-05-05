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

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Timers;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Common.Legacy.Vpn;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.ConnectLogs;
using ProtonVPN.Logging.Contracts.Events.DisconnectLogs;
using ProtonVPN.Logging.Contracts.Events.ProtocolLogs;
using ProtonVPN.OperatingSystems.Network.Contracts;
using ProtonVPN.OperatingSystems.Network.Contracts.Monitors;
using ProtonVPN.Vpn.Common;
using ProtonVPN.Vpn.Gateways;
using Timer = System.Timers.Timer;

namespace ProtonVPN.Vpn.WireGuard;

public class WireGuardConnection: IWireGuardConnection
{
    private const int MIN_CONNECTION_TIMEOUT = 5000;
    private const int MAX_CONNECTION_TIMEOUT = 30000;

    private readonly ILogger _logger;
    private readonly IConfiguration _config;
    private readonly IGatewayCache _gatewayCache;
    private readonly IWireGuardService _wireGuardService;
    private readonly IWireGuardConfigGenerator _wireGuardConfigGenerator;
    private readonly INtTrafficManager _ntTrafficManager;
    private readonly IWintunTrafficManager _wintunTrafficManager;
    private readonly IWireGuardStateMonitor _wireGuardStateMonitor;
    private readonly IRouteChangeMonitor _routeChangeMonitor;
    private readonly ISystemNetworkInterfaces _networkInterfaces;
    private readonly IInterfaceForwardingMonitor _interfaceForwardingMonitor;
    private readonly INetworkInterfacePolicyManager _interfacePolicyManager;
    private readonly IWireGuardServerRouteManager _serverRouteManager;
    private readonly SemaphoreSlim _serviceSemaphore = new(1, 1);

    private readonly Channel<VpnState> _stateChannel = Channel.CreateUnbounded<VpnState>();

    private CancellationTokenSource _cts = new();
    public VpnError LastError { get; private set; }
    public string LocalIpv4Address => _config.WireGuard.DefaultClientIpv4Address;

    public NetworkTraffic NetworkTraffic { get; private set; } = NetworkTraffic.Zero;

    private TaskCompletionSource<bool>? _connectionTaskCompletionSource;
    private volatile bool _isConnected;
    private VpnCredentials _credentials;
    private VpnEndpoint? _endpoint;
    private VpnConfig? _vpnConfig;
    private INetworkInterfacePolicyLease? _interfacePolicyLease;

    private readonly Timer _serviceHealthCheckTimer = new();

    private bool IsWireGuardServerRouteEnabled => _vpnConfig?.IsWireGuardServerRouteEnabled == true;

    public WireGuardConnection(
        ILogger logger,
        IConfiguration config,
        IGatewayCache gatewayCache,
        IWireGuardService wireGuardService,
        IWireGuardConfigGenerator wireGuardConfigGenerator,
        INtTrafficManager ntTrafficManager,
        IWintunTrafficManager wintunTrafficManager,
        IWireGuardStateMonitor wireGuardStateMonitor,
        IRouteChangeMonitor routeChangeMonitor,
        ISystemNetworkInterfaces networkInterfaces,
        IInterfaceForwardingMonitor interfaceForwardingMonitor,
        INetworkInterfacePolicyManager interfacePolicyManager,
        IWireGuardServerRouteManager serverRouteManager)
    {
        _logger = logger;
        _config = config;
        _gatewayCache = gatewayCache;
        _wireGuardService = wireGuardService;
        _wireGuardConfigGenerator = wireGuardConfigGenerator;
        _ntTrafficManager = ntTrafficManager;
        _wintunTrafficManager = wintunTrafficManager;
        _wireGuardStateMonitor = wireGuardStateMonitor;
        _routeChangeMonitor = routeChangeMonitor;
        _networkInterfaces = networkInterfaces;
        _interfaceForwardingMonitor = interfaceForwardingMonitor;
        _interfacePolicyManager = interfacePolicyManager;
        _serverRouteManager = serverRouteManager;

        _routeChangeMonitor.RouteChanged += OnRouteChanged;
        _interfaceForwardingMonitor.ForwardingEnabled += OnInterfaceForwardingEnabledAsync;
        _serviceHealthCheckTimer.Interval = config.ServiceCheckInterval.TotalMilliseconds;
        _serviceHealthCheckTimer.Elapsed += CheckIfServiceIsRunningAsync;
    }

    public async Task<VpnError> ConnectAsync(VpnEndpoint endpoint, VpnCredentials credentials,
        VpnConfig config, CancellationToken cancellationToken)
    {
        _credentials = credentials;
        _endpoint = endpoint;
        _vpnConfig = config;

        _connectionTaskCompletionSource = new();
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _isConnected = false;

        NetworkTraffic = NetworkTraffic.Zero;
        LastError = VpnError.None;

        bool isWireGuardServerRouteEnabled = IsWireGuardServerRouteEnabled;
        INetworkInterface bestInterface = GetBestInterface();

        if (!isWireGuardServerRouteEnabled)
        {
            if (bestInterface.IsIPv4ForwardingEnabled)
            {
                _logger.Warn<ConnectLog>($"Triggering disconnect due to active interface forwarding " +
                $"on interface {bestInterface.Name} with index {bestInterface.Index}.");

                return VpnError.InterfaceHasForwardingEnabled;
            }
        }

        WriteConfig();
        UpdateGatewayCache();

        if (isWireGuardServerRouteEnabled)
        {
            _serverRouteManager.CleanupPersistedRoutes();
            _serverRouteManager.CreateServerRoute(_endpoint, _vpnConfig);
        }
        else
        {
            ApplyInterfacePolicy(bestInterface);
        }

        await RunWithServiceLockAsync(_wireGuardService.StopAsync, _cts.Token);

        StartMonitoringVpnStateAsync(_cts.Token);

        await RunWithServiceLockAsync(() => _wireGuardService.StartAsync(_cts.Token, _vpnConfig.VpnProtocol), _cts.Token);

        int timeout = Math.Clamp((int)_vpnConfig.WireGuardConnectionTimeout.TotalMilliseconds, MIN_CONNECTION_TIMEOUT, MAX_CONNECTION_TIMEOUT);
        // cancellationToken instead of _cts.Token to avoid cancelling the delay when disconnecting
        Task timeoutTask = Task.Delay(timeout, cancellationToken);

        Task completedTask = await Task.WhenAny(timeoutTask, _connectionTaskCompletionSource.Task);
        cancellationToken.ThrowIfCancellationRequested();

        if (completedTask == timeoutTask)
        {
            _logger.Warn<ConnectLog>($"{timeout}ms timeout reached, disconnecting.");
            return VpnError.AdapterTimeoutError;
        }

        if (!_connectionTaskCompletionSource.Task.IsCompleted || !_connectionTaskCompletionSource.Task.Result)
        {
            return LastError;
        }

        StartMonitoringNetworkTrafficAsync(_cts.Token);

        return VpnError.None;
    }

    private INetworkInterface GetBestInterface()
    {
        return _vpnConfig is null
            ? new NullNetworkInterface()
            : _networkInterfaces.GetBestInterfaceExcludingHardwareId(_config.GetWireGuardHardwareId());
    }

    private void ApplyInterfacePolicy(INetworkInterface bestInterface)
    {
        ReleaseInterfacePolicy();

        if (_vpnConfig is null || !_vpnConfig.ShouldDisableWeakHostSetting)
        {
            return;
        }

        if (bestInterface.Index == 0)
        {
            _logger.Warn<ConnectLog>("Skipping interface policy application because no active interface was resolved.");
            return;
        }

        try
        {
            _interfacePolicyLease = _interfacePolicyManager.Apply(bestInterface);
        }
        catch (Exception ex)
        {
            _logger.Warn<ConnectLog>("Failed to apply interface policy.", ex);
        }
    }

    private void ReleaseInterfacePolicy()
    {
        try
        {
            _interfacePolicyLease?.Dispose();
            _interfacePolicyLease = null;
        }
        catch (Exception e)
        {
            _logger.Warn<ConnectLog>("Failed to dispose interface policy lease.", e);
        }
    }

    private void UpdateGatewayCache()
    {
        _gatewayCache.Save(IPAddress.Parse(_config.WireGuard.DefaultServerGatewayIpv4Address));
    }

    public async Task DisconnectAsync()
    {
        _isConnected = false;

        ReleaseInterfacePolicy();
        _serviceHealthCheckTimer.Stop();
        await RunWithServiceLockAsync(_wireGuardService.StopAsync);

        if (IsWireGuardServerRouteEnabled)
        {
            if (_endpoint is not null)
            {
                _serverRouteManager.DeleteServerRoutes(_endpoint);
            }
        }
        else
        {
            _interfaceForwardingMonitor.Stop();
        }

        SetConnectionTaskResult(false);
    }

    public async IAsyncEnumerable<VpnState> ObserveStatesAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            yield return await _stateChannel.Reader.ReadAsync(cancellationToken);
        }
    }

    private void SetConnectionTaskResult(bool result)
    {
        if (_connectionTaskCompletionSource?.Task.IsCompletedSuccessfully == false)
        {
            _connectionTaskCompletionSource?.SetResult(result);
        }
    }

    private void WriteConfig()
    {
        if (_endpoint is null || _vpnConfig is null)
        {
            return;
        }

        CreateConfigDirectoryPathIfNotExists();
        string configContent = _wireGuardConfigGenerator.GenerateConfig(_endpoint, _credentials, _vpnConfig);
        File.WriteAllText(_config.WireGuard.ConfigFilePath, configContent);
    }

    private void CreateConfigDirectoryPathIfNotExists()
    {
        string? directoryPath = Path.GetDirectoryName(_config.WireGuard.ConfigFilePath);
        if (!string.IsNullOrEmpty(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
    }

    private void StartMonitoringVpnStateAsync(CancellationToken cancellationToken)
    {
        _ = Task.Run(async () => await MonitorVpnStateAsync(cancellationToken), cancellationToken);
    }

    private async Task MonitorVpnStateAsync(CancellationToken cancellationToken)
    {
        try
        {
            await foreach (VpnState state in _wireGuardStateMonitor.WatchStatesAsync(cancellationToken))
            {
                if (state.Status == VpnStatus.Connected)
                {
                    _isConnected = true;
                    SetConnectionTaskResult(true);
                    UpdateGatewayCache();
                    _serviceHealthCheckTimer.Start();

                    if (IsWireGuardServerRouteEnabled)
                    {
                        _routeChangeMonitor.Start();
                    }
                    else
                    {
                        _interfaceForwardingMonitor.Start();
                    }
                }
                else
                {
                    if (state.Error != VpnError.None)
                    {
                        LastError = state.Error;
                        SetConnectionTaskResult(false);

                        if (!_isConnected)
                        {
                            _cts.Cancel();
                            return;
                        }
                    }

                    await _stateChannel.Writer.WriteAsync(state, cancellationToken);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // expected on cancellation
        }
        catch (Exception ex)
        {
            _logger.Error<WireGuardProtocolLog>("Status monitor failed.", ex);
        }
    }

    private void StartMonitoringNetworkTrafficAsync(CancellationToken cancellationToken)
    {
        _ = Task.Run(async () => await MonitorNetworkTrafficAsync(cancellationToken), cancellationToken);
    }

    private async Task MonitorNetworkTrafficAsync(CancellationToken cancellationToken)
    {
        try
        {
            IAsyncEnumerable<NetworkTraffic> trafficStream =
                _vpnConfig?.VpnProtocol == VpnProtocol.WireGuardUdp
                    ? _ntTrafficManager.WatchTrafficAsync(cancellationToken)
                    : _wintunTrafficManager.WatchTrafficAsync(cancellationToken);

            await foreach (NetworkTraffic traffic in trafficStream.WithCancellation(cancellationToken))
            {
                NetworkTraffic = traffic;
            }
        }
        catch (OperationCanceledException)
        {
            // expected on cancellation
        }
        catch (Exception ex)
        {
            _logger.Error<WireGuardProtocolLog>("Traffic monitor failed.", ex);
        }
    }

    private async Task RunWithServiceLockAsync(Func<Task> action, CancellationToken cancellationToken = default)
    {
        await _serviceSemaphore.WaitAsync(cancellationToken);

        try
        {
            await action();
        }
        finally
        {
            _serviceSemaphore.Release();
        }
    }

    private async void OnInterfaceForwardingEnabledAsync(object? sender, InterfaceForwardingEventArgs e)
    {
        if (IsWireGuardServerRouteEnabled || !_isConnected || _endpoint is null)
        {
            return;
        }

        try
        {
            INetworkInterface bestInterface = GetBestInterface();
            if (bestInterface.Index != e.InterfaceIndex)
            {
                return;
            }

            _logger.Warn<DisconnectTriggerLog>(
                $"Detected active interface forwarding on interface {bestInterface.Name} with index {e.InterfaceIndex}.");

            await _stateChannel.Writer.WriteAsync(new VpnState(VpnStatus.Connected, VpnError.InterfaceHasForwardingEnabled, _endpoint.VpnProtocol), _cts.Token);
        }
        catch (Exception ex)
        {
            _logger.Warn<ConnectLog>("Failed to handle interface forwarding notification.", ex);
        }
    }

    private void OnRouteChanged(object? sender, RouteChangeEventArgs e)
    {
        if (!IsWireGuardServerRouteEnabled || !_isConnected || _endpoint is null || _vpnConfig is null)
        {
            return;
        }

        _serverRouteManager.CreateServerRoute(_endpoint, _vpnConfig);
    }

    private async void CheckIfServiceIsRunningAsync(object? sender, ElapsedEventArgs e)
    {
        if (_isConnected && !_wireGuardService.Running() && !_cts.IsCancellationRequested && _endpoint is not null)
        {
            _logger.Info<DisconnectTriggerLog>($"The service {_wireGuardService.Name} is not running. " +
                         "Sending VpnError.Unknown to get reconnected.");

            await _stateChannel.Writer.WriteAsync(new VpnState(VpnStatus.Connected, VpnError.Unknown, _endpoint.VpnProtocol), _cts.Token);
        }
    }
}
