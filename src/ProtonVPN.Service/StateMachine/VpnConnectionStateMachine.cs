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
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using ProtonVPN.Common.Core.Extensions;
using ProtonVPN.Common.Core.Helpers;
using ProtonVPN.Common.Core.LocalAgent;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Common.Legacy.Vpn;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.LocalAgentLogs;
using ProtonVPN.Logging.Contracts.Events.VpnStateMachineLogs;
using ProtonVPN.Service.SplitTunneling;
using ProtonVPN.Service.StateMachine.Messages;
using ProtonVPN.Service.StateMachine.SideEffects;
using ProtonVPN.Service.Vpn;
using ProtonVPN.Vpn.Common;
using ProtonVPN.Vpn.Connection;
using ProtonVPN.Vpn.LocalAgent;
using Stateless;

namespace ProtonVPN.Service.StateMachine;

internal sealed partial class VpnConnectionStateMachine : IVpnConnectionStateMachine
{
    private readonly ILogger _logger;
    private readonly ILocalAgent _localAgent;
    private readonly ILocalAgentTlsCredentialsCache _localAgentTlsCredentialsCache;
    private readonly ILocalAgentEventReceiver _localagentEventReceiver;
    private readonly IVpnEndpointCandidates _candidates;
    private readonly IConnectionProbe _connectionProbe;
    private readonly ITunnelOrchestrator _tunnelOrchestrator;
    private readonly IVpnStateSideEffects _vpnStateSideEffects;
    private readonly StateMachine<State, Trigger> _machine;

    private readonly object _disconnectTaskLock = new();
    private readonly object _sessionStateLock = new();

    private static readonly TimeSpan _connectedStateTimeout = TimeSpan.FromSeconds(10);

    private CancellationTokenSource _sessionCts = new();
    private CancellationTokenSource _localAgentChannelsCts = new();

    private Task? _disconnectTask;
    private bool _connectionCredentialsMonitorStarted;
    private long _connectionCredentialsSubscribedVersion;
    private bool _hasPortForwardingError;
    private bool _keepConnectedDuringLocalAgentReconnect;
    private IReadOnlyList<VpnHost> _servers = [];
    private VpnConfig? _vpnConfig;
    private VpnCredentials? _credentials;
    private VpnEndpoint? _selectedEndpoint;
    private VpnError _lastError = VpnError.None;
    private LocalAgentState? _localAgentState;
    private ConnectionCertificate? _lastConnectionCertificate;
    private int _machineStateSnapshot = (int)State.Disconnected;

    private State MachineState => (State)Volatile.Read(ref _machineStateSnapshot);

    public bool IsConnected => MachineState == State.Connected;

    public VpnError LastError => _lastError;

    private readonly List<VpnError> _disconnectOnVpnErrors =
    [
        VpnError.SessionKilledDueToMultipleKeys,
        VpnError.CertificateRevoked,
        VpnError.CertCARevokedOrExpired,
        VpnError.PlanNeedsToBeUpgraded,
        VpnError.SessionLimitReachedFree,
        VpnError.SessionLimitReachedBasic,
        VpnError.SessionLimitReachedPlus,
        VpnError.SessionLimitReachedVisionary,
        VpnError.SessionLimitReachedPro,
        VpnError.SessionLimitReachedUnknown,
        VpnError.SystemErrorOnTheServer,
        VpnError.ServerSessionDoesNotMatch,
        VpnError.ServerSessionError,
    ];

    private readonly List<VpnError> _waitForUserActionOnVpnErrors =
    [
        VpnError.TwoFactorRequiredReasonUnknown,
        VpnError.TwoFactorExpired,
        VpnError.TwoFactorNewConnection,
    ];

    private readonly Dictionary<Trigger, Func<CancellationToken?, Task>> _typedTriggerDispatch;

    public VpnConnectionStateMachine(
        ILogger logger,
        ILocalAgent localAgent,
        ILocalAgentTlsCredentialsCache localAgentTlsCredentialsCache,
        ILocalAgentEventReceiver localAgentEventReceiver,
        IVpnEndpointCandidates candidates,
        IConnectionProbe connectionProbe,
        ITunnelOrchestrator tunnelOrchestrator,
        IVpnStateSideEffects vpnStateSideEffects)
    {
        _logger = logger;
        _localAgent = localAgent;
        _localAgentTlsCredentialsCache = localAgentTlsCredentialsCache;
        _localagentEventReceiver = localAgentEventReceiver;
        _candidates = candidates;
        _connectionProbe = connectionProbe;
        _tunnelOrchestrator = tunnelOrchestrator;
        _vpnStateSideEffects = vpnStateSideEffects;

        _machine = new StateMachine<State, Trigger>(State.Disconnected);
        _connectTrigger = _machine.SetTriggerParameters<CancellationToken>(Trigger.ConnectRequested);
        _availabilitySucceededTrigger = _machine.SetTriggerParameters<CancellationToken>(Trigger.AvailabilitySucceeded);
        _endpointSelectedTrigger = _machine.SetTriggerParameters<CancellationToken>(Trigger.EndpointSelected);
        _endpointSelectionFailedTrigger = _machine.SetTriggerParameters<CancellationToken>(Trigger.EndpointSelectionFailed);
        _clientSecretKeyChangedTrigger = _machine.SetTriggerParameters<CancellationToken>(Trigger.ClientSecretKeyChanged);
        _localAgentConnectionRequestedTrigger = _machine.SetTriggerParameters<CancellationToken>(Trigger.LocalAgentConnectionRequested);
        _connectionCertificateChangedTrigger = _machine.SetTriggerParameters<CancellationToken>(Trigger.ConnectionCertificateChanged);

        _typedTriggerDispatch = new()
        {
            [Trigger.ConnectRequested] = ct => _machine.FireAsync(_connectTrigger, ct),
            [Trigger.AvailabilitySucceeded] = ct => _machine.FireAsync(_availabilitySucceededTrigger, ct),
            [Trigger.EndpointSelectionFailed] = ct => _machine.FireAsync(_endpointSelectionFailedTrigger, ct),
            [Trigger.EndpointSelected] = ct => _machine.FireAsync(_endpointSelectedTrigger, ct),
            [Trigger.ClientSecretKeyChanged] = ct => _machine.FireAsync(_clientSecretKeyChangedTrigger, ct),
            [Trigger.LocalAgentConnectionRequested] = ct => _machine.FireAsync(_localAgentConnectionRequestedTrigger, ct),
            [Trigger.ConnectionCertificateChanged] = ct => _machine.FireAsync(_connectionCertificateChangedTrigger, ct),
        };

        Configure();

        _machine.OnTransitionedAsync(HandleTransitionedAsync);

        _machine.OnUnhandledTrigger((s, trigger) =>
        {
            _logger.Warn<VpnStateMachineLog>($"VPN state machine ignored trigger {trigger} while in state {s}.");
        });

        StartMessageSupervisor();
    }

    public void Connect(IReadOnlyList<VpnHost> servers, VpnConfig config, VpnCredentials credentials)
    {
        PostMessage(new ConnectRequestMessage(servers, config, credentials));
    }

    public void Reconnect()
    {
        PostMessage(new ReconnectRequestMessage());
    }

    public void Disconnect(VpnError error = VpnError.None)
    {
        CancelSession();
        PostMessage(new DisconnectRequestMessage(error));
    }

    // This method should only be used by the service shutdown logic
    // to synchronously wait for the tunnel to disconnect before allowing the process to exit.
    // It will not modify state in the state machine, therefore SubscribeToStateChanged callbacks will not be triggered.
    public async Task DisconnectAsync()
    {
        _messagesCts.Cancel();

        CancelSession();

        await _tunnelOrchestrator.DisconnectAsync();

        State machineState = State.Disconnected;
        VpnState vpnState = await MapStateAsync(machineState);

        await _vpnStateSideEffects.ApplyAsync(vpnState, machineState);
    }

    public void ReportDisconnected(VpnError error)
    {
        PostMessage(new ReportDisconnectedMessage(error));
    }

    public void UpdateVpnConfig(VpnFeatures vpnFeatures)
    {
        PostMessage(new UpdateVpnFeaturesMessage(vpnFeatures));
    }

    private void OnConnected()
    {
        SetKeepConnectedDuringLocalAgentReconnect(false);
    }

    private CancellationToken GetSessionToken()
    {
        lock (_sessionStateLock)
        {
            return _sessionCts.Token;
        }
    }

    private bool IsCurrentSession(CancellationToken token)
    {
        lock (_sessionStateLock)
        {
            return token == _sessionCts.Token && !token.IsCancellationRequested;
        }
    }

    private void CancelSession()
    {
        CancellationTokenSource cts;
        lock (_sessionStateLock)
        {
            cts = _sessionCts;
        }

        try
        {
            cts.Cancel();
        }
        catch (ObjectDisposedException)
        {
        }
    }

    private bool GetKeepConnectedDuringLocalAgentReconnect()
    {
        lock (_sessionStateLock)
        {
            return _keepConnectedDuringLocalAgentReconnect;
        }
    }

    private void SetKeepConnectedDuringLocalAgentReconnect(bool value)
    {
        lock (_sessionStateLock)
        {
            _keepConnectedDuringLocalAgentReconnect = value;
        }
    }

    private void ResetSessionContext()
    {
        CancellationTokenSource previousSessionCts;
        lock (_sessionStateLock)
        {
            previousSessionCts = _sessionCts;
            _sessionCts = new();

            _connectionCredentialsMonitorStarted = false;
            _connectionCredentialsSubscribedVersion = 0;
            _keepConnectedDuringLocalAgentReconnect = false;
        }

        previousSessionCts.Cancel();
        previousSessionCts.Dispose();
    }

    private void StartMonitoringCredentialsUpdates()
    {
        CancellationToken sessionToken;
        lock (_sessionStateLock)
        {
            if (_connectionCredentialsMonitorStarted)
            {
                return;
            }

            _connectionCredentialsMonitorStarted = true;
            _connectionCredentialsSubscribedVersion = _localAgentTlsCredentialsCache.CurrentVersion;
            sessionToken = _sessionCts.Token;
        }

        Task credentialUpdateMonitorTask = MonitorChannelAsync(
            _localAgentTlsCredentialsCache.LocalAgentTlsCredentialsChannel,
            HandleCredentialsUpdateAsync,
            sessionToken,
            sessionToken);

        _ = Task.Run(() => credentialUpdateMonitorTask, sessionToken);
    }

    private Task HandleCredentialsUpdateAsync(LocalAgentTlsCredentialsUpdate credentialsUpdate, CancellationToken ct)
    {
        PostMessage(new CredentialsUpdatedMessage(credentialsUpdate, ct));
        return Task.CompletedTask;
    }

    private void OnExitConnectedState(StateMachine<State, Trigger>.Transition transition)
    {
        switch (transition.Trigger)
        {
            case Trigger.ConnectionCertificateChanged:
                SetKeepConnectedDuringLocalAgentReconnect(true);
                break;
        }
    }

    private Task StartDisconnectingTunnelAsync()
    {
        lock (_disconnectTaskLock)
        {
            if (_disconnectTask is { IsCompleted: false })
            {
                return _disconnectTask;
            }

            _disconnectTask = Task.Run(async() =>
            {
                _hasPortForwardingError = false;

                await _tunnelOrchestrator.DisconnectAsync();
                await RunVpnStateSideEffectsAsync(State.Disconnected);
            });

            return _disconnectTask;
        }
    }

    private async Task RunVpnStateSideEffectsAsync(State state)
    {
        VpnState vpnState = await MapStateAsync(state);
        // MachineState is used intentionally here instead of the state parameter. When the
        // machine has already advanced to a new connection (e.g. AvailabilityCheck) while the
        // previous tunnel is still tearing down, MachineState will not equal State.Disconnected,
        // so _killSwitch.OnVpnDisconnected is suppressed — preventing the firewall from
        // dropping during a reconnect and leaking DNS/traffic.
        await _vpnStateSideEffects.ApplyAsync(vpnState, MachineState);
    }

    private async Task EstablishLocalAgentChannelAsync(StateMachine<State, Trigger>.Transition transition, CancellationToken ct)
    {
        if (_vpnConfig is null)
        {
            _logger.Error<VpnStateMachineLog>("Can't connect to local agent channel due to a missing VPN config.");
            _lastError = VpnError.Unknown;

            Fire(Trigger.DisconnectRequested, ct);
            return;
        }

        if (transition.Trigger == Trigger.ConnectionCertificateChanged)
        {
            _localAgent.CloseTlsChannel();
        }

        StartMonitoringCredentialsUpdates();

        ConnectionCertificate? certificate = (await _localAgentTlsCredentialsCache.GetAsync(ct))?.ConnectionCertificate;
        string? clientSecretPem = _credentials?.ClientKeyPair.SecretKey.Pem;

        if (certificate is null || clientSecretPem is null || _selectedEndpoint is null)
        {
            string? missingFields = NullFieldFormatter.FormatNullFields(
                (nameof(certificate), certificate),
                (nameof(clientSecretPem), clientSecretPem),
                (nameof(_selectedEndpoint), _selectedEndpoint));
            _logger.Error<VpnStateMachineLog>($"Can't connect to local agent channel due to missing data: {missingFields}.");

            _lastError = VpnError.Unknown;
            Fire(Trigger.DisconnectRequested, ct);

            return;
        }

        _ = DisconnectIfNotConnectedAfterTimeoutAsync(ct);

        _lastConnectionCertificate = certificate;

        bool result = _localAgent.ConnectToTlsChannel(new LocalAgentConnectParams()
        {
            Server = _selectedEndpoint.Server,
            ClientCertPem = certificate.Pem,
            ClientSecretPem = clientSecretPem,
            VpnConfig = _vpnConfig,
        });

        if (result)
        {
            StartMonitoringLocalAgentChannels(ct);
        }
        else
        {
            _lastError = _localAgent.LastError;
            Fire(Trigger.DisconnectRequested, ct);
        }
    }

    private async Task DisconnectIfNotConnectedAfterTimeoutAsync(CancellationToken ct)
    {
        try
        {
            await Task.Delay(_connectedStateTimeout, ct);

            if (ct.IsCancellationRequested || _lastError == VpnError.TwoFactorNewConnection)
            {
                return;
            }

            if (!IsConnected)
            {
                _logger.Info<VpnStateMachineLog>("Connected state not reached within 10 seconds after local agent channel setup.");

                _lastError = VpnError.ServerUnreachable;
                Fire(Trigger.DisconnectRequested, ct);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private void StartMonitoringLocalAgentChannels(CancellationToken sessionToken)
    {
        CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(sessionToken);

        _localAgentChannelsCts.Cancel();
        _localAgentChannelsCts.Dispose();
        _localAgentChannelsCts = cts;

        // Don't pass cts.Token to the task as we want WatchEventsAsync to drain all the events after cancelling the token
        _ = Task.Run(() => _localagentEventReceiver.WatchEventsAsync(cts.Token));

        _ = Task.Run(() => MonitorChannelAsync(_localagentEventReceiver.StateChannel, HandleLocalAgentStateAsync, cts.Token, sessionToken), cts.Token);
        _ = Task.Run(() => MonitorChannelAsync(_localagentEventReceiver.ErrorChannel, HandleLocalAgentErrorAsync, cts.Token, sessionToken), cts.Token);
    }

    private async Task MonitorChannelAsync<T>(
        Channel<T> channel,
        Func<T, CancellationToken, Task> entityHandler,
        CancellationToken channelReadToken,
        CancellationToken sessionToken)
    {
        try
        {
            while (!channelReadToken.IsCancellationRequested)
            {
                T entity = await channel.Reader.ReadAsync(channelReadToken);
                await entityHandler(entity, sessionToken);
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            _logger.Error<VpnStateMachineLog>("Local agent channel observer threw an exception.", ex);
        }
    }

    private Task HandleLocalAgentStateAsync(LocalAgentState state, CancellationToken ct)
    {
        PostMessage(new LocalAgentStateChangedMessage(state, ct));
        return Task.CompletedTask;
    }

    private Task HandleLocalAgentErrorAsync(VpnError error, CancellationToken ct)
    {
        PostMessage(new LocalAgentErrorMessage(error, ct));
        return Task.CompletedTask;
    }

    private async Task ProcessLocalAgentErrorAsync(VpnError error, CancellationToken ct)
    {
        if (_waitForUserActionOnVpnErrors.Contains(error))
        {
            // When local agent connects, we receive one event with error for two factor to be completed.
            // Then the local agent library sends request to set NetShield level and a second message
            // with the same error is received. So we want to skip that.
            if (_lastError == VpnError.TwoFactorNewConnection)
            {
                return;
            }

            _logger.Info<VpnStateMachineLog>("Two factor required. Waiting for user action.");

            _lastError = error;
            Fire(Trigger.TwoFactorRequested, ct);
        }
        else if (_disconnectOnVpnErrors.Contains(error))
        {
            Disconnect(error);
        }
        else if (error == VpnError.CertificateNotYetProvided)
        {
            _logger.Info<LocalAgentErrorLog>("Reconnecting to TLS channel.");
            Fire(Trigger.ConnectionCertificateChanged, ct);
        }
        else if (error == VpnError.CertificateExpired)
        {
            await HandleConnectionCertificateExpirationAsync(ct);
        }
        else if (error == VpnError.PortForwardingNotSupported)
        {
            _hasPortForwardingError = true;
        }
        else
        {
            _logger.Info<LocalAgentErrorLog>($"Ignoring error {error}.");
        }
    }

    private async Task HandleConnectionCredentialsChangeAsync(CancellationToken ct)
    {
        _lastError = VpnError.None;

        LocalAgentTlsCredentials? tlsCredentials = await _localAgentTlsCredentialsCache.GetAsync(ct);

        if (tlsCredentials is not null &&
            _credentials is not null &&
            tlsCredentials.ClientKeyPair.SecretKey.Pem != _credentials.Value.ClientKeyPair.SecretKey.Pem)
        {
            _credentials = new(
                tlsCredentials.ConnectionCertificate.Pem,
                tlsCredentials.ConnectionCertificate.ExpirationDateUtc,
                tlsCredentials.ClientKeyPair,
                _credentials.Value.Username,
                _credentials.Value.Password);

            _logger.Info<VpnStateMachineLog>("The client secret key has changed, reconnecting the tunnel.");
            Fire(Trigger.ClientSecretKeyChanged, ct);
            return;
        }

        if (_lastConnectionCertificate?.Pem != tlsCredentials?.ConnectionCertificate.Pem)
        {
            _logger.Info<VpnStateMachineLog>("The connection certificate has changed.");
            Fire(Trigger.ConnectionCertificateChanged, ct);
        }
    }

    private async Task HandleConnectionCertificateExpirationAsync(CancellationToken ct)
    {
        if (!IsCurrentSession(ct))
        {
            return;
        }

        ConnectionCertificate? lastCertificate = _lastConnectionCertificate;
        LocalAgentTlsCredentials? currentCredentials = await _localAgentTlsCredentialsCache.GetAsync(ct);

        if (string.IsNullOrWhiteSpace(currentCredentials?.ConnectionCertificate.Pem) ||
            currentCredentials?.ConnectionCertificate.Pem == lastCertificate?.Pem)
        {
            _lastError = VpnError.CertificateExpired;
            SetKeepConnectedDuringLocalAgentReconnect(true);
            Fire(Trigger.RequireCertificateUpdate, ct);
        }
        else
        {
            _logger.Info<VpnStateMachineLog>("The current connection certificate is not null and is different from the " +
                "last certificate used. Closing existing TLS channel and reconnecting.");

            Fire(Trigger.ConnectionCertificateChanged, ct);
        }
    }

    private async Task StartAvailabilityCheckAsync(CancellationToken ct)
    {
        if (_vpnConfig == null)
        {
            _logger.Error<VpnStateMachineLog>("VPN state machine: availability check failed because config is null.");
            _lastError = VpnError.Unknown;
            Fire(Trigger.AvailabilityFailed, ct);
            return;
        }

        _localAgent.CloseTlsChannel();
        await StartDisconnectingTunnelAsync();
        if (!IsCurrentSession(ct))
        {
            return;
        }

        _lastError = VpnError.None;

        try
        {
            _candidates.Reset();
            ProbeAvailabilityResult result = await _connectionProbe.ProbeAvailabilityAsync(_candidates, _vpnConfig, ct);
            if (!IsCurrentSession(ct))
            {
                return;
            }

            _lastError = result.Error;

            if (result.Success)
            {
                Fire(Trigger.AvailabilitySucceeded, ct);
            }
            else
            {
                Fire(Trigger.AvailabilityFailed, ct);
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            if (!IsCurrentSession(ct))
            {
                return;
            }

            _logger.Error<VpnStateMachineLog>("VPN state machine availability check failed unexpectedly.", ex);
            _lastError = VpnError.Unknown;
            Fire(Trigger.AvailabilityFailed, ct);
        }
    }

    private async Task StartSelectingEndpointAsync(CancellationToken ct)
    {
        if (_vpnConfig == null)
        {
            _logger.Error<VpnStateMachineLog>("VPN state machine: endpoint selection failed because config is null.");
            _lastError = VpnError.Unknown;
            Fire(Trigger.DisconnectRequested, ct);
            return;
        }

        try
        {
            _candidates.Reset();
            VpnEndpoint endpoint = await _connectionProbe.SelectEndpointAsync(_candidates, _vpnConfig, ct);
            if (!IsCurrentSession(ct))
            {
                return;
            }

            if (endpoint.Server.IsEmpty())
            {
                _lastError = VpnError.PingTimeoutError;
                Fire(Trigger.EndpointSelectionFailed, ct);
                return;
            }

            _selectedEndpoint = endpoint;
            if (_vpnConfig is not null)
            {
                _vpnStateSideEffects.UpdateSplitTunnelContext(new SplitTunnelContext(_vpnConfig, _selectedEndpoint));
            }

            Fire(Trigger.EndpointSelected, ct);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            if (!IsCurrentSession(ct))
            {
                return;
            }

            _logger.Error<VpnStateMachineLog>("VPN state machine endpoint selection failed unexpectedly.", ex);
            _lastError = VpnError.Unknown;
            Fire(Trigger.EndpointSelectionFailed, ct);
        }
    }

    private async Task EstablishTunnelAsync(CancellationToken ct)
    {
        if (!IsCurrentSession(ct))
        {
            return;
        }

        if (_credentials == null || _vpnConfig == null || _selectedEndpoint == null)
        {
            string? missingFields = NullFieldFormatter.FormatNullFields(
                (nameof(_credentials), _credentials),
                (nameof(_vpnConfig), _vpnConfig),
                (nameof(_selectedEndpoint), _selectedEndpoint));

            _logger.Error<VpnStateMachineLog>($"Can't establish tunnel due to missing data: {missingFields}.");

            Disconnect(VpnError.Unknown);
            return;
        }

        VpnEndpoint endpoint = _selectedEndpoint;
        VpnCredentials credentials = _credentials.Value;
        VpnConfig vpnConfig = _vpnConfig;

        vpnConfig.UpdateVpnProtocol(endpoint.VpnProtocol);
        _ = Task.Run(() => MonitorTunnelStatesAsync(ct), ct);

        VpnError error;

        try
        {
            error = await _tunnelOrchestrator.ConnectAsync(endpoint, credentials, vpnConfig, ct);
            if (!IsCurrentSession(ct))
            {
                return;
            }
        }
        catch (OperationCanceledException)
        {
            _logger.Info<VpnStateMachineLog>("Tunnel establishment was canceled.");
            return;
        }
        catch (Exception ex)
        {
            _logger.Error<VpnStateMachineLog>("Tunnel establishment failed.", ex);
            error = VpnError.Unknown;
        }

        if (error == VpnError.None)
        {
            LocalAgentTlsCredentials? localAgentTlsCredentials = await _localAgentTlsCredentialsCache.GetAsync(ct);
            if (localAgentTlsCredentials is null || string.IsNullOrEmpty(localAgentTlsCredentials.ConnectionCertificate.Pem))
            {
                Fire(Trigger.ConnectedToGuestHole, ct);
            }
            else
            {
                Fire(Trigger.LocalAgentConnectionRequested, ct);
            }
        }
        else
        {
            Disconnect(error);
        }
    }

    private async Task MonitorTunnelStatesAsync(CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                VpnState state = await _tunnelOrchestrator.StateChannel.Reader.ReadAsync(ct);
                if (state.Error == VpnError.None)
                {
                    if (state.Status == VpnStatus.AssigningIp)
                    {
                        await _vpnStateSideEffects.ApplyAsync(
                            CreateDecoratedState(VpnStatus.AssigningIp, state.Error, state.ConnectionCertificate),
                            MachineState);
                    }
                }
                else
                {
                    Disconnect(state.Error);
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            _logger.Error<VpnStateMachineLog>("Tunnel state monitor failed.", ex);
        }
    }

    private VpnState CreateDecoratedState(VpnStatus status, VpnError error, ConnectionCertificate? connectionCertificate = null)
    {
        VpnHost? server = _selectedEndpoint?.Server ?? (_servers.Count > 0 ? _servers[0] : null);
        VpnProtocol vpnProtocol = _vpnConfig?.VpnProtocol ?? VpnProtocol.Smart;

        string? remoteIp = server?.Ip;
        if (string.IsNullOrEmpty(remoteIp) && server?.RelayIpByProtocol.ContainsKey(vpnProtocol) == true)
        {
            remoteIp = server?.RelayIpByProtocol[vpnProtocol];
        }

        return new VpnState(
            status: status,
            error: error,
            localIp: _tunnelOrchestrator.VpnConnection?.LocalIpv4Address ?? string.Empty,
            remoteIp: remoteIp,
            endpointPort: _selectedEndpoint?.Port ?? 0,
            vpnProtocol: vpnProtocol,
            portForwarding: !_hasPortForwardingError && (_vpnConfig?.PortForwarding ?? false),
            openVpnAdapter: _vpnConfig?.VpnProtocol.IsOpenVpn() == true
                ? _vpnConfig.OpenVpnAdapter
                : null,
            label: server?.Label ?? string.Empty,
            connectionCertificate: connectionCertificate);
    }

    public void SubscribeToStateChanged(Func<VpnState, Task> action)
    {
        _machine.OnTransitioned(async transition =>
        {
            try
            {
                VpnState state = await MapStateAsync(transition.Destination);
                await action(state);
            }
            catch (OperationCanceledException)
            {
            }
        });
    }

    private async Task HandleTransitionedAsync(StateMachine<State, Trigger>.Transition transition)
    {
        _logger.Debug<VpnStateMachineLog>($"VPN state machine transitioned from {transition.Source} " +
            $"to {transition.Destination} state, " +
            $"Trigger: ({transition.Trigger}).");

        VpnState sideEffectState = await MapStateAsync(transition.Destination);
        await _vpnStateSideEffects.ApplyAsync(sideEffectState, transition.Destination);
    }

    private async Task<VpnState> MapStateAsync(State state)
    {
        ConnectionCertificate? connectionCertificate = null;
        try
        {
            connectionCertificate = (await _localAgentTlsCredentialsCache.GetAsync(GetSessionToken()))?.ConnectionCertificate;
        }
        catch (OperationCanceledException)
        {
        }

        return CreateDecoratedState(MapStatus(state), _lastError, connectionCertificate);
    }

    private VpnStatus MapStatus(State state)
    {
        bool keepConnectedDuringLocalAgentReconnect = GetKeepConnectedDuringLocalAgentReconnect();

        if (keepConnectedDuringLocalAgentReconnect && state is State.EstablishingLocalAgentChannel)
        {
            state = State.Connected;
        }

        return state switch
        {
            State.Disconnected => VpnStatus.Disconnected,
            State.AvailabilityCheck or
            State.SelectingEndpoint => VpnStatus.Pinging,
            State.EstablishingTunnel => VpnStatus.Connecting,
            State.EstablishingLocalAgentChannel => VpnStatus.AssigningIp,
            State.Connected => VpnStatus.Connected,
            State.ActionRequired => VpnStatus.ActionRequired,
            _ => throw new NotImplementedException(),
        };
    }
}