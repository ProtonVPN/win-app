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
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Common.Legacy.Vpn;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Crypto.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.ConnectionLogs;
using ProtonVPN.Logging.Contracts.Events.ConnectLogs;
using ProtonVPN.Logging.Contracts.Events.DisconnectLogs;
using ProtonVPN.Logging.Contracts.Events.NetworkLogs;
using ProtonVPN.OperatingSystems.Network.Contracts;
using ProtonVPN.Vpn.Common;
using ProtonVPN.Vpn.Management;
using ProtonVPN.Vpn.NetworkAdapters;
using ProtonVPN.Vpn.OpenVpn;
using ProtonVPN.Vpn.Wintun;

namespace ProtonVPN.Vpn.Connection;

internal class OpenVpnConnection : IOpenVpnConnection
{
    private static readonly TimeSpan _waitForConnectionTaskToFinishAfterClose = TimeSpan.FromSeconds(3);
    private static readonly TimeSpan _waitForConnectionTaskToFinishAfterCancellation = TimeSpan.FromSeconds(3);
    private const int MANAGEMENT_PASSWORD_LENGTH = 16;

    private readonly ILogger _logger;
    private readonly IStaticConfiguration _config;
    private readonly INetworkInterfaceLoader _networkInterfaceLoader;
    private readonly IOpenVpnProcess _process;
    private readonly IManagementClient _managementClient;
    private readonly IWintunAdapter _winTunAdapter;
    private readonly ITapAdapter _tapAdapter;
    private readonly INetworkUtilities _networkUtilities;
    private readonly IWintunRegistryFixer _wintunRegistryFixer;

    private readonly OpenVpnManagementPorts _managementPorts;
    private readonly IRandomStringGenerator _randomStringGenerator;

    private readonly Channel<VpnState> _stateChannel = Channel.CreateUnbounded<VpnState>();
    private Channel<VpnState> _managementStateChannel = Channel.CreateUnbounded<VpnState>();

    private CancellationTokenSource _connectionCts = new();
    private Task? _connectTask;
    private TaskCompletionSource<bool>? _connectionTaskCompletionSource;
    private volatile bool _isConnected;
    private volatile bool _disconnectRequested;
    private string? _localIpv4Address;

    private VpnEndpoint? _endpoint;
    private VpnCredentials _credentials;
    private VpnError _disconnectError = VpnError.None;
    private VpnConfig? _vpnConfig;

    public OpenVpnConnection(
        ILogger logger,
        IStaticConfiguration config,
        INetworkInterfaceLoader networkInterfaceLoader,
        IOpenVpnProcess process,
        IRandomStringGenerator randomStringGenerator,
        IManagementClient managementClient,
        IWintunAdapter winTunAdapter,
        ITapAdapter tapAdapter,
        INetworkUtilities networkUtilities,
        IWintunRegistryFixer wintunRegistryFixer)
    {
        _logger = logger;
        _config = config;
        _networkInterfaceLoader = networkInterfaceLoader;
        _process = process;
        _randomStringGenerator = randomStringGenerator;
        _managementClient = managementClient;
        _winTunAdapter = winTunAdapter;
        _tapAdapter = tapAdapter;
        _wintunRegistryFixer = wintunRegistryFixer;

        _managementPorts = new OpenVpnManagementPorts();
        _networkUtilities = networkUtilities;
    }

    public string? LocalIpv4Address => _localIpv4Address;

    public NetworkTraffic NetworkTraffic => _managementClient.NetworkTraffic;

    public async Task<VpnError> ConnectAsync(
        VpnEndpoint endpoint,
        VpnCredentials credentials,
        VpnConfig vpnConfig,
        CancellationToken cancellationToken)
    {
        _vpnConfig = vpnConfig;
        _endpoint = endpoint;
        _credentials = credentials;

        _connectionTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        _localIpv4Address = null;
        _isConnected = false;
        _disconnectRequested = false;

        ResetConnectionCancellation(cancellationToken);
        StartMonitoringStateChannel(_connectionCts.Token);
        StartMonitoringManagementChannelStates(_connectionCts.Token);

        if (_vpnConfig.OpenVpnAdapter == OpenVpnAdapter.Tun)
        {
            _winTunAdapter.Create();
        }
        else
        {
            _tapAdapter.Create();
        }

        _connectTask = Task.Run(() => ConnectActionAsync(_connectionCts.Token), _connectionCts.Token);

        await WaitForConnectionResultAsync(_connectTask, _connectionTaskCompletionSource.Task, _connectionCts.Token);

        if (_connectionCts.IsCancellationRequested)
        {
            _connectionCts.Token.ThrowIfCancellationRequested();
        }

        bool isConnected = _connectionTaskCompletionSource.Task.Result;
        if (!isConnected)
        {
            return _disconnectError;
        }

        return VpnError.None;
    }

    public async Task DisconnectAsync()
    {
        _disconnectRequested = true;
        _isConnected = false;

        _connectionCts.Cancel();

        _logger.Info<DisconnectLog>("Disconnect action started");
        OnStateChanged(VpnStatus.Disconnecting);

        try
        {
            await CloseVpnConnectionAsync();
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            _managementClient.Disconnect();
            _process.Stop();
        }

        _winTunAdapter.Close();
        RestoreNetworkSettings();

        _logger.Info<DisconnectLog>("Disconnect action completed");
        OnStateChanged(VpnStatus.Disconnected);
    }

    public async IAsyncEnumerable<VpnState> ObserveStatesAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            yield return await _stateChannel.Reader.ReadAsync(cancellationToken);
        }
    }

    private void ResetConnectionCancellation(CancellationToken cancellationToken)
    {
        CancellationTokenSource next = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        CancellationTokenSource? previous = Interlocked.Exchange(ref _connectionCts, next);
        previous?.Cancel();
        previous?.Dispose();

        _managementStateChannel = Channel.CreateUnbounded<VpnState>();
        _managementClient.ResetState();
    }

    private void StartMonitoringStateChannel(CancellationToken cancellationToken)
    {
        _ = Task.Run(() => MonitorStateChannelAsync(cancellationToken), cancellationToken);
    }

    private void StartMonitoringManagementChannelStates(CancellationToken cancellationToken)
    {
        _ = Task.Run(() => MonitorManagementChannelStatesAsync(cancellationToken), cancellationToken);
    }

    private async Task MonitorStateChannelAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                VpnState state = await _managementStateChannel.Reader.ReadAsync(cancellationToken);
                ProcessState(state);
                await _stateChannel.Writer.WriteAsync(state, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            _logger.Error<ConnectionLog>("State monitor failed.", ex);
        }
    }

    private void ProcessState(VpnState state)
    {
        if (!string.IsNullOrEmpty(state.LocalIp))
        {
            _localIpv4Address = state.LocalIp;
        }

        if (state.Error != VpnError.None)
        {
            _disconnectError = state.Error;
            if (!_isConnected)
            {
                SetConnectionTaskResult(false);
            }
        }

        if (state.Status == VpnStatus.Connected)
        {
            _isConnected = true;
            SetConnectionTaskResult(true);
            return;
        }

        if (state.Status is VpnStatus.Disconnecting or VpnStatus.Disconnected)
        {
            if (!_isConnected)
            {
                SetConnectionTaskResult(false);
            }

            _isConnected = false;
        }
    }

    private async Task MonitorManagementChannelStatesAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                VpnState state = await _managementClient.StateChannel.Reader.ReadAsync(cancellationToken);
                HandleManagementState(state);
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            _logger.Error<ConnectionLog>("Management state monitor failed.", ex);
        }
    }

    private void HandleManagementState(VpnState managementState)
    {
        if (_endpoint is null || _vpnConfig is null)
        {
            return;
        }

        _logger.Info<ConnectionStateChangeLog>($"ManagementClient: State changed to {managementState.Status}");

        VpnState state = new(
            managementState.Status,
            managementState.Error,
            managementState.LocalIp ?? string.Empty,
            managementState.RemoteIp,
            _endpoint.Port,
            _endpoint.VpnProtocol,
            _vpnConfig.PortForwarding,
            _vpnConfig.OpenVpnAdapter,
            managementState.Label);

        if ((state.Status == VpnStatus.Pinging || state.Status == VpnStatus.Connecting || state.Status == VpnStatus.Reconnecting) &&
            string.IsNullOrEmpty(state.RemoteIp))
        {
            state = new VpnState(
                state.Status,
                VpnError.None,
                string.Empty,
                _endpoint.Server.Ip,
                _endpoint.Port,
                _endpoint.VpnProtocol,
                _vpnConfig.PortForwarding,
                state.OpenVpnAdapter,
                _endpoint.Server.Label);
        }

        if (state.Status == VpnStatus.Disconnecting && !_disconnectRequested)
        {
            _disconnectError = state.Error;
        }

        OnStateChanged(state);
    }

    private void SetConnectionTaskResult(bool result)
    {
        if (_connectionTaskCompletionSource?.Task.IsCompletedSuccessfully == false)
        {
            _connectionTaskCompletionSource.SetResult(result);
        }
    }

    private async Task WaitForConnectionResultAsync(Task connectTask, Task<bool> completionTask, CancellationToken cancellationToken)
    {
        Task cancellationTask = Task.Delay(Timeout.Infinite, cancellationToken);
        Task completed = await Task.WhenAny(completionTask, connectTask, cancellationTask);

        if (completed == cancellationTask)
        {
            return;
        }

        if (completed == connectTask && !completionTask.IsCompleted)
        {
            SetConnectionTaskResult(false);
        }

        if (connectTask.IsCompleted && connectTask.IsFaulted)
        {
            _logger.Error<ConnectionLog>("An OpenVpnConnection task threw an exception.", connectTask.Exception?.InnerException);
        }
    }

    private async Task ConnectActionAsync(CancellationToken cancellationToken)
    {
        if (_endpoint is null || _vpnConfig is null)
        {
            _disconnectError = VpnError.Unknown;
            _logger.Error<ConnectionLog>("Trying to connect, but either _endpoint or _vpnConfig is null");
            return;
        }

        _logger.Info<ConnectStartLog>("Connect action started");

        try
        {
            OnStateChanged(VpnStatus.Connecting);
            if (!WriteConfig())
            {
                _disconnectError = VpnError.Unknown;
                return;
            }

            ApplyNetworkSettings();
            _wintunRegistryFixer.EnsureTunAdapterRegistryIsCorrect();

            int port = _managementPorts.Port();
            string password = ManagementPassword();

            OpenVpnProcessParams processParams = new(
                _endpoint,
                port,
                password,
                GetCustomDnsServers(_vpnConfig),
                _vpnConfig.SplitTunnelMode,
                _vpnConfig.OpenVpnAdapter,
                GetNetworkInterfaceIdOrEmpty());

            cancellationToken.ThrowIfCancellationRequested();

            if (!await _process.Start(processParams))
            {
                _disconnectError = VpnError.Unknown;
            }
            else
            {
                await _managementClient.ConnectAsync(port, password, cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                {
                    await _managementClient.CloseVpnConnectionAsync();
                    cancellationToken.ThrowIfCancellationRequested();
                }

                await _managementClient.StartVpnConnectionAsync(_credentials, _endpoint, cancellationToken);
            }

            cancellationToken.ThrowIfCancellationRequested();
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            _logger.Info<ConnectLog>("Connect action completed");

            if (!cancellationToken.IsCancellationRequested && !_disconnectRequested)
            {
                OnStateChanged(VpnStatus.Disconnecting);
            }
        }
    }

    private static List<string> GetCustomDnsServers(VpnConfig config)
    {
        return config.CustomDns
            .Where(dns => NetworkAddress.TryParse(dns, out NetworkAddress networkAddress) &&
                          networkAddress.IsIpV4 || (networkAddress.IsIpV6 && config.IsIpv6Enabled)).ToList();
    }

    private bool WriteConfig()
    {
        try
        {
            bool isIpv6Enabled = _vpnConfig?.IsIpv6Enabled == true && _endpoint?.Server.IsIpv6Supported == true;
            ConfigTemplate template = new();
            string content = template.GetConfig(_credentials, isIpv6Enabled);
            File.WriteAllText(_config.OpenVpn.ConfigPath, content);
            return true;
        }
        catch (Exception e)
        {
            _logger.Error<ConnectionErrorLog>("Failed to update OpenVPN config file.", e);
            return false;
        }
    }

    private string GetNetworkInterfaceIdOrEmpty()
    {
        if (_vpnConfig is null)
        {
            return string.Empty;
        }

        return _networkInterfaceLoader.GetByVpnProtocol(_vpnConfig.VpnProtocol, _vpnConfig.OpenVpnAdapter)?.Id ?? string.Empty;
    }

    private string ManagementPassword()
    {
        return _randomStringGenerator.Generate(MANAGEMENT_PASSWORD_LENGTH);
    }

    private async Task CloseVpnConnectionAsync()
    {
        Task? connectTask = _connectTask;
        if (connectTask is null)
        {
            return;
        }

        if (!connectTask.IsCompleted)
        {
            await TryCloseVpnConnectionAndWaitAsync(connectTask);
        }

        if (!connectTask.IsCompleted)
        {
            await CancelVpnConnectionAndWaitAsync(connectTask);
        }
    }

    private async Task TryCloseVpnConnectionAndWaitAsync(Task connectTask)
    {
        try
        {
            await _managementClient.CloseVpnConnectionAsync();
        }
        catch (Exception ex)
        {
            _logger.Warn<DisconnectLog>($"Failed writing to management channel: {ex.Message}");
        }

        try
        {
            _logger.Info<DisconnectLog>("Waiting for Connection task to finish...");
            if (await Task.WhenAny(connectTask, Task.Delay(_waitForConnectionTaskToFinishAfterClose)) != connectTask)
            {
                _logger.Warn<DisconnectLog>(
                    $"Connection task has not finished in {_waitForConnectionTaskToFinishAfterClose}");
                return;
            }

            await connectTask;
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            _logger.Error<DisconnectLog>($"Connection task failed with exception: {ex}");
        }
    }

    private async Task CancelVpnConnectionAndWaitAsync(Task connectTask)
    {
        try
        {
            _logger.Info<DisconnectLog>("Cancelling Connection task");
            _connectionCts?.Cancel();

            _logger.Info<DisconnectLog>("Waiting for Connection task to finish...");
            if (await Task.WhenAny(connectTask,
                    Task.Delay(_waitForConnectionTaskToFinishAfterCancellation)) != connectTask)
            {
                _logger.Warn<DisconnectLog>(
                    $"Connection task has not finished in {_waitForConnectionTaskToFinishAfterCancellation}");
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            _logger.Error<DisconnectLog>($"Connection task failed: {ex}");
        }
    }

    private void OnStateChanged(VpnStatus status)
    {
        if (_endpoint is null || _vpnConfig is null)
        {
            return;
        }

        VpnState state;
        switch (status)
        {
            case VpnStatus.Pinging:
            case VpnStatus.Connecting:
                state = new VpnState(status, VpnError.None, string.Empty, _endpoint.Server.Ip, _endpoint.Port,
                    _endpoint.VpnProtocol, _vpnConfig.PortForwarding, _vpnConfig.OpenVpnAdapter, _endpoint.Server.Label);
                break;
            case VpnStatus.Disconnecting:
            case VpnStatus.Disconnected:
                state = new VpnState(status, _disconnectError, _vpnConfig?.VpnProtocol ?? VpnProtocol.Smart);
                break;
            default:
                state = new VpnState(status, VpnError.None, _vpnConfig?.VpnProtocol ?? VpnProtocol.Smart);
                break;
        }

        _logger.Info<ConnectionStateChangeLog>($"State changed to {state.Status}, Error: {state.Error}");
        OnStateChanged(state);
    }

    private void OnStateChanged(VpnState state)
    {
        _managementStateChannel.Writer.TryWrite(state);
    }

    private void ApplyNetworkSettings()
    {
        uint interfaceIndex = GetInterfaceIndex();
        if (interfaceIndex == 0)
        {
            return;
        }

        try
        {
            _logger.Info<NetworkLog>("Setting interface metric...");
            _networkUtilities.SetLowestTapMetric(interfaceIndex);
            _logger.Info<NetworkLog>("Interface metric set.");
        }
        catch (NetworkUtilException e)
        {
            _logger.Error<NetworkLog>("Failed to apply network settings. Error code: " + e.Code);
        }
    }

    private void RestoreNetworkSettings()
    {
        uint interfaceIndex = GetInterfaceIndex();
        if (interfaceIndex == 0)
        {
            return;
        }

        try
        {
            _logger.Info<NetworkLog>("Restoring interface metric...");
            _networkUtilities.RestoreDefaultTapMetric(interfaceIndex);
            _logger.Info<NetworkLog>("Interface metric restored.");
        }
        catch (NetworkUtilException e)
        {
            _logger.Error<NetworkLog>("Failed restore network settings. Error code: " + e.Code);
        }
    }

    private uint GetInterfaceIndex()
    {
        if (_vpnConfig is null)
        {
            return 0;
        }

        return _networkInterfaceLoader.GetByVpnProtocol(_vpnConfig.VpnProtocol, _vpnConfig.OpenVpnAdapter).Index;
    }
}