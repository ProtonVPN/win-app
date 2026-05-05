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
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Common.Legacy.Vpn;
using ProtonVPN.Logging.Contracts.Events.VpnStateMachineLogs;
using ProtonVPN.Service.StateMachine.Messages;
using ProtonVPN.Vpn.LocalAgent;

namespace ProtonVPN.Service.StateMachine;

internal sealed partial class VpnConnectionStateMachine
{
    private readonly CancellationTokenSource _messagesCts = new();
    private readonly Channel<IStateMachineMessage> _messageQueue = Channel.CreateUnbounded<IStateMachineMessage>();

    private Task? _messageSupervisorTask;

    private void StartMessageSupervisor()
    {
        if (_messageSupervisorTask is not null)
        {
            return;
        }

        _messageSupervisorTask = Task.Run(SuperviseMessagesAsync);

        _messageSupervisorTask.ContinueWith(task =>
            {
                Exception ex = task.Exception?.GetBaseException()
                    ?? new InvalidOperationException("Message supervisor task faulted without exception details.");

                _logger.Error<VpnStateMachineLog>(
                    "VPN state machine message supervisor task terminated unexpectedly.",
                    ex);
            },
            CancellationToken.None,
            TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default);
    }

    private void Fire(Trigger trigger, CancellationToken cancellationToken)
    {
        PostMessage(new TriggerMessage(trigger, cancellationToken));
    }

    private void PostMessage(IStateMachineMessage message)
    {
        if (_messagesCts.IsCancellationRequested)
        {
            _logger.Warn<VpnStateMachineLog>($"Failed to enqueue message {message.GetType().Name}, " +
                $"because the state machine no longer accepts messages.");
            return;
        }

        if (!_messageQueue.Writer.TryWrite(message))
        {
            _logger.Warn<VpnStateMachineLog>($"Failed to enqueue message {message.GetType().Name}.");
        }
    }

    private async Task SuperviseMessagesAsync()
    {
        while (!_messagesCts.IsCancellationRequested)
        {
            try
            {
                while (await _messageQueue.Reader.WaitToReadAsync(_messagesCts.Token).ConfigureAwait(false))
                {
                    while (_messageQueue.Reader.TryRead(out IStateMachineMessage? message))
                    {
                        await ProcessMessageAsync(message).ConfigureAwait(false);
                    }
                }

                if (!_messagesCts.IsCancellationRequested)
                {
                    _logger.Warn<VpnStateMachineLog>("VPN state machine message queue has completed unexpectedly.");
                }

                return;
            }
            catch (Exception ex)
            {
                if (ex is not OperationCanceledException)
                {
                    _logger.Error<VpnStateMachineLog>("VPN state machine message supervisor failed, restarting.", ex);

                    await Task.Delay(TimeSpan.FromMilliseconds(250)).ConfigureAwait(false);
                }
            }
        }
    }

    private async Task ProcessMessageAsync(IStateMachineMessage message)
    {
        if (_messagesCts.IsCancellationRequested)
        {
            return;
        }

        switch (message)
        {
            case ConnectRequestMessage connectRequestMessage:
                await ProcessConnectRequestAsync(connectRequestMessage).ConfigureAwait(false);
                break;
            case DisconnectRequestMessage disconnectRequestMessage:
                await ProcessDisconnectRequestAsync(disconnectRequestMessage).ConfigureAwait(false);
                break;
            case ReconnectRequestMessage:
                await ProcessReconnectRequestAsync().ConfigureAwait(false);
                break;
            case ReportDisconnectedMessage reportDisconnectedMessage:
                await ProcessReportDisconnectedMessageAsync(reportDisconnectedMessage).ConfigureAwait(false);
                break;
            case UpdateVpnFeaturesMessage updateVpnFeaturesMessage:
                ProcessUpdateVpnFeaturesMessage(updateVpnFeaturesMessage);
                break;
            case LocalAgentStateChangedMessage localAgentStateChangedMessage:
                await ProcessLocalAgentStateChangedAsync(localAgentStateChangedMessage).ConfigureAwait(false);
                break;
            case LocalAgentErrorMessage localAgentErrorMessage:
                await ProcessLocalAgentErrorMessageAsync(localAgentErrorMessage).ConfigureAwait(false);
                break;
            case CredentialsUpdatedMessage credentialsUpdatedMessage:
                await ProcessCredentialsUpdatedMessageAsync(credentialsUpdatedMessage).ConfigureAwait(false);
                break;
            case TriggerMessage triggerMessage:
                await ProcessTriggerAsync(triggerMessage).ConfigureAwait(false);
                break;
            default:
                _logger.Warn<VpnStateMachineLog>($"VPN state machine ignored unknown message type {message.GetType().Name}.");
                break;
        }
    }

    private async Task ProcessConnectRequestAsync(ConnectRequestMessage message)
    {
        ResetSessionContext();

        CancellationToken sessionToken = GetSessionToken();

        _servers = message.Servers;
        _vpnConfig = message.Config;
        _credentials = message.Credentials;

        _selectedEndpoint = null;
        _lastError = VpnError.None;
        _localAgentState = null;

        _candidates.Set(_servers);
        _candidates.Reset();

        await ProcessTriggerAsync(new TriggerMessage(Trigger.ConnectRequested, sessionToken)).ConfigureAwait(false);
    }

    private async Task ProcessDisconnectRequestAsync(DisconnectRequestMessage message)
    {
        _lastError = message.Error;
        _ = StartDisconnectingTunnelAsync();

        await ProcessTriggerAsync(new TriggerMessage(Trigger.DisconnectRequested, CancellationToken.None)).ConfigureAwait(false);
    }

    private async Task ProcessReconnectRequestAsync()
    {
        if (_vpnConfig is null || _credentials is null || _servers.Count == 0)
        {
            _logger.Warn<VpnStateMachineLog>("Ignoring reconnect request due to missing connection data.");
            return;
        }

        await ProcessConnectRequestAsync(new ConnectRequestMessage(_servers, _vpnConfig, _credentials.Value)).ConfigureAwait(false);
    }

    private async Task ProcessReportDisconnectedMessageAsync(ReportDisconnectedMessage message)
    {
        _lastError = message.Error;
        await ProcessTriggerAsync(new TriggerMessage(Trigger.DisconnectedReported, CancellationToken.None)).ConfigureAwait(false);
    }

    private void ProcessUpdateVpnFeaturesMessage(UpdateVpnFeaturesMessage message)
    {
        if (_vpnConfig is null)
        {
            return;
        }

        _vpnConfig = new VpnConfig(new()
        {
            Ports = _vpnConfig.Ports,
            CustomDns = _vpnConfig.CustomDns,
            SplitTunnelMode = _vpnConfig.SplitTunnelMode,
            SplitTunnelIPs = _vpnConfig.SplitTunnelIPs,
            OpenVpnAdapter = _vpnConfig.OpenVpnAdapter,
            VpnProtocol = _vpnConfig.VpnProtocol,
            PreferredProtocols = _vpnConfig.PreferredProtocols,
            NetShieldMode = message.VpnFeatures.NetShieldMode,
            SplitTcp = message.VpnFeatures.SplitTcp,
            PortForwarding = message.VpnFeatures.PortForwarding,
            IsIpv6Enabled = _vpnConfig.IsIpv6Enabled,
            WireGuardConnectionTimeout = _vpnConfig.WireGuardConnectionTimeout,
            DnsBlockMode = _vpnConfig.DnsBlockMode,
        });
    }

    private async Task ProcessLocalAgentStateChangedAsync(LocalAgentStateChangedMessage message)
    {
        if (!IsCurrentSession(message.SessionToken))
        {
            return;
        }

        // Handle feature changes after connection is already established, e.g. port forwarding.
        if (_localAgentState == LocalAgentState.Connected && message.State == LocalAgentState.Connected)
        {
            await RunVpnStateSideEffectsAsync(State.Connected).ConfigureAwait(false);
            return;
        }

        if (_localAgentState == message.State)
        {
            return;
        }

        _localAgentState = message.State;

        switch (message.State)
        {
            case LocalAgentState.Connected:
                _lastError = VpnError.None;
                Fire(Trigger.LocalAgentReceivedConnectedState, message.SessionToken);
                break;
            case LocalAgentState.ServerCertificateError:
                _lastError = VpnError.TlsCertificateError;
                Fire(Trigger.DisconnectRequested, message.SessionToken);
                break;
            case LocalAgentState.ClientCertificateExpiredError:
            case LocalAgentState.ClientCertificateUnknownCA:
                await HandleConnectionCertificateExpirationAsync(message.SessionToken).ConfigureAwait(false);
                break;
            case LocalAgentState.ServerUnreachable when MachineState is State.Connected:
                _lastError = VpnError.ServerUnreachable;
                Fire(Trigger.DisconnectRequested, message.SessionToken);
                break;
        }
    }

    private async Task ProcessLocalAgentErrorMessageAsync(LocalAgentErrorMessage message)
    {
        if (!IsCurrentSession(message.SessionToken))
        {
            return;
        }

        await ProcessLocalAgentErrorAsync(message.Error, message.SessionToken).ConfigureAwait(false);
    }

    private async Task ProcessCredentialsUpdatedMessageAsync(CredentialsUpdatedMessage message)
    {
        if (!IsCurrentSession(message.SessionToken))
        {
            return;
        }

        bool shouldHandle;
        lock (_sessionStateLock)
        {
            shouldHandle = message.Update.Version > _connectionCredentialsSubscribedVersion;
        }

        if (!shouldHandle)
        {
            return;
        }

        await HandleConnectionCredentialsChangeAsync(message.SessionToken).ConfigureAwait(false);
    }

    private async Task ProcessTriggerAsync(TriggerMessage item)
    {
        if (item.SessionToken is not null && item.SessionToken.Value.IsCancellationRequested)
        {
            return;
        }

        if (_machine.CanFire(item.Trigger))
        {
            try
            {
                await FireTriggerAsync(item).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (ex is not TaskCanceledException && ex is not OperationCanceledException)
                {
                    _logger.Error<VpnStateMachineLog>($"Trigger {item} failed.", ex);
                }
            }
            finally
            {
                Volatile.Write(ref _machineStateSnapshot, (int)_machine.State);
            }
        }
        else
        {
            _logger.Warn<VpnStateMachineLog>($"VPN state machine ignored trigger {item} while in state {MachineState}.");
        }
    }

    private Task FireTriggerAsync(TriggerMessage item)
    {
        return _typedTriggerDispatch.TryGetValue(item.Trigger, out Func<CancellationToken?, Task>? fireWithToken)
            ? fireWithToken(item.SessionToken)
            : _machine.FireAsync(item.Trigger);
    }
}