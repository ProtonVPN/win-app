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

using System.Threading.Channels;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.ConnectionLogs;
using ProtonVPN.Logging.Contracts.Events.ProtocolLogs;
using ProtonVPN.ProTun.Generated;
using static ProtonVPN.ProTun.Generated.ConnectionState;
using static ProtonVPN.ProTun.Generated.DisconnectReason;
using ProTunVpnState = ProtonVPN.ProTun.Generated.VpnState;
using VpnState = ProtonVPN.Common.Core.Networking.VpnState;

namespace ProtonVPN.ProTun.StateChanges;

public class ProTunStateChangeHandler : IProTunStateChangeHandler
{
    private readonly ILogger _logger;

    public Channel<VpnState> StateChannel { get; } = Channel.CreateUnbounded<VpnState>();

    private CancellationToken? _cancellationToken;

    public ProTunStateChangeHandler(ILogger logger)
    {
        _logger = logger;
    }

    public void SetCancellationToken(CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;
    }

    public async void OnStateChanged(ProTunVpnState state)
    {
        ConnectionState connectionState = state.ConnectionState;

        await (connectionState switch
        {
            Disconnected disconnectedState => HandleDisconnectedStateAsync(disconnectedState),
            Connecting connectingState => HandleConnectingStateAsync(connectingState),
            ConnectingToLocalAgent connectingToLocalAgentState => HandleConnectingToLocalAgentAsync(connectingToLocalAgentState),
            Connected connectedState => HandleConnectedStateAsync(connectedState),
        });
    }

    private async Task HandleConnectedStateAsync(Connected connectedState)
    {
        await InvokeStateWithPeerAsync(VpnStatus.Connected, connectedState.Peer);
    }

    private async Task HandleConnectingStateAsync(Connecting connectingState)
    {
        await InvokeStateAsync(new(VpnStatus.Waiting, VpnProtocol.Smart));

        PeerConnectionInfo? peer = connectingState.Peers.FirstOrDefault();
        if (peer is null) // ProTUN sends connecting without peers on a change of peers, or network availability change
        {
            await InvokeStateAsync(new(VpnStatus.Connecting, VpnProtocol.Smart));
        }
        else
        {
            await InvokeStateWithPeerAsync(VpnStatus.Connecting, peer);
        }
    }

    private Task HandleConnectingToLocalAgentAsync(ConnectingToLocalAgent connectingToLocalAgentState)
    {
        _logger.Error<ConnectionErrorLog>("ProTUN is connecting to Local Agent and it shouldn't.");
        return Task.CompletedTask;
    }

    private async Task HandleDisconnectedStateAsync(Disconnected disconnectedState)
    {
        if (disconnectedState.Error is null)
        {
            await InvokeStateAsync(new(VpnStatus.Disconnected, VpnProtocol.Smart));
        }
        else
        {
            string errorMessage = GetErrorMessage(disconnectedState.Error);
            _logger.Error<ConnectionErrorLog>($"ProTUN disconnected with error: {errorMessage}");
            await InvokeStateAsync(new(VpnStatus.Disconnected, VpnError.Unknown, VpnProtocol.Smart));
        }
    }

    private string GetErrorMessage(DisconnectReason error)
    {
        return error switch
        {
            TunEstablishError e => $"Tun error: {e.Message}",
        };
    }

    private async Task InvokeStateWithPeerAsync(VpnStatus vpnStatus, PeerConnectionInfo peer)
    {
        await InvokeStateAsync(new(vpnStatus,
            remoteIp: peer.EntryIp,
            endpointPort: peer.Port,
            vpnProtocol: MapProtocol(peer.Protocol),
            openVpnAdapter: null,
            label: GetLabelFromId(peer.PeerId)));
    }

    private VpnProtocol MapProtocol(Protocol protocol)
    {
        switch (protocol)
        {
            case Protocol.WireguardUdp:
                return VpnProtocol.ProTunUdp;
            case Protocol.WireguardTcp:
                return VpnProtocol.ProTunTcp;
            case Protocol.Stealth:
                return VpnProtocol.ProTunTls;
            default:
                _logger.Error<ProTunProtocolLog>($"The protocol '{protocol}' is not implemented in the mapper.");
                return VpnProtocol.Smart;
        }
    }

    private string GetLabelFromId(string peerId)
    {
        string[] parts = peerId.Split('@');
        if (parts.Length < 2)
        {
            _logger.Error<ProTunProtocolLog>($"Received a peer ID with only {parts.Length} parts (Peer ID: {peerId}) and therefore no Label.");
            return string.Empty;
        }
        if (parts.Length > 2)
        {
            _logger.Error<ProTunProtocolLog>($"Received a peer ID with more parts than expected (Received {parts.Length}, Expected 2) (Peer ID: {peerId}).");
        }
        return parts[1];
    }

    private async Task InvokeStateAsync(VpnState vpnState)
    {
        try
        {
            CancellationToken? cancellationToken = _cancellationToken;
            if (cancellationToken is not null)
            {
                await StateChannel.Writer.WriteAsync(vpnState, cancellationToken.Value);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }
}