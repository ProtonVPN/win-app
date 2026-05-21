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

using System.Threading;
using Stateless;

namespace ProtonVPN.Service.StateMachine;

internal sealed partial class VpnConnectionStateMachine
{
    private readonly StateMachine<State, Trigger>.TriggerWithParameters<CancellationToken> _connectTrigger;
    private readonly StateMachine<State, Trigger>.TriggerWithParameters<CancellationToken> _availabilitySucceededTrigger;
    private readonly StateMachine<State, Trigger>.TriggerWithParameters<CancellationToken> _endpointSelectionFailedTrigger;
    private readonly StateMachine<State, Trigger>.TriggerWithParameters<CancellationToken> _endpointSelectedTrigger;
    private readonly StateMachine<State, Trigger>.TriggerWithParameters<CancellationToken> _clientSecretKeyChangedTrigger;
    private readonly StateMachine<State, Trigger>.TriggerWithParameters<CancellationToken> _localAgentConnectionRequestedTrigger;
    private readonly StateMachine<State, Trigger>.TriggerWithParameters<CancellationToken> _connectionCertificateChangedTrigger;

    private void Configure()
    {
        _machine.Configure(State.Disconnected)
            .Permit(Trigger.ConnectRequested, State.AvailabilityCheck)
            .PermitReentry(Trigger.DisconnectedReported)
            .Ignore(Trigger.DisconnectRequested);

        _machine.Configure(State.AvailabilityCheck)
            .OnEntryFromAsync(_connectTrigger, StartAvailabilityCheckAsync)
            .PermitReentry(Trigger.ConnectRequested)
            .Permit(Trigger.AvailabilitySucceeded, State.SelectingEndpoint)
            .Permit(Trigger.AvailabilityFailed, State.Disconnected)
            .Permit(Trigger.DisconnectRequested, State.Disconnected);

        _machine.Configure(State.SelectingEndpoint)
            .OnEntryFromAsync(_availabilitySucceededTrigger, StartSelectingEndpointAsync)
            .OnEntryFromAsync(_endpointSelectionFailedTrigger, StartSelectingEndpointAsync)
            .PermitReentry(Trigger.EndpointSelectionFailed)
            .Permit(Trigger.ConnectRequested, State.AvailabilityCheck)
            .Permit(Trigger.DisconnectRequested, State.Disconnected)
            .Permit(Trigger.EndpointSelected, State.EstablishingTunnel);

        _machine.Configure(State.EstablishingTunnel)
            .OnEntryFromAsync(_endpointSelectedTrigger, EstablishTunnelAsync)
            .OnEntryFromAsync(_clientSecretKeyChangedTrigger, EstablishTunnelAsync)
            .Permit(Trigger.ConnectRequested, State.AvailabilityCheck)
            .Permit(Trigger.DisconnectRequested, State.Disconnected)
            .Permit(Trigger.LocalAgentConnectionRequested, State.EstablishingLocalAgentChannel)
            .Permit(Trigger.ConnectedToGuestHole, State.Connected);

        _machine.Configure(State.EstablishingLocalAgentChannel)
            .OnEntryFromAsync(_localAgentConnectionRequestedTrigger, (ct, transition) => EstablishLocalAgentChannelAsync(transition, ct))
            .OnEntryFromAsync(_connectionCertificateChangedTrigger, (ct, transition) => EstablishLocalAgentChannelAsync(transition, ct))
            .Permit(Trigger.DisconnectRequested, State.Disconnected)
            .Permit(Trigger.ConnectRequested, State.AvailabilityCheck)
            .Permit(Trigger.TwoFactorRequested, State.ActionRequired)
            .Permit(Trigger.LocalAgentReceivedConnectedState, State.Connected);

        _machine.Configure(State.Connected)
            .OnEntry(OnConnected)
            .OnExit(OnExitConnectedState)
            .Permit(Trigger.DisconnectRequested, State.Disconnected)
            .Permit(Trigger.ConnectionCertificateChanged, State.EstablishingLocalAgentChannel)
            .Permit(Trigger.RequireCertificateUpdate, State.ActionRequired)
            .Permit(Trigger.ClientSecretKeyChanged, State.EstablishingTunnel)
            .Permit(Trigger.TwoFactorRequested, State.ActionRequired)
            .Permit(Trigger.ConnectRequested, State.AvailabilityCheck);

        _machine.Configure(State.ActionRequired)
            .Permit(Trigger.ConnectRequested, State.AvailabilityCheck)
            .Permit(Trigger.DisconnectRequested, State.Disconnected)
            .Permit(Trigger.LocalAgentReceivedConnectedState, State.Connected)
            .Permit(Trigger.ConnectionCertificateChanged, State.EstablishingLocalAgentChannel);
    }
}