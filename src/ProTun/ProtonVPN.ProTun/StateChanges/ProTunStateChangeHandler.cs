/*
 * Copyright (c) 2025 Proton AG
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

using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Common.Legacy;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.ConnectionLogs;
using ProtonVPN.Logging.Contracts.Events.ProtocolLogs;
using ProtonVPN.ProTun.Generated;
using static ProtonVPN.ProTun.Generated.State;

namespace ProtonVPN.ProTun.StateChanges;

public class ProTunStateChangeHandler : IProTunStateChangeHandler
{
    private readonly ILogger _logger;
    private readonly IIssueReporter _issueReporter;

    public event EventHandler<EventArgs<VpnState>>? StateChanged;

    public ProTunStateChangeHandler(ILogger logger, IIssueReporter issueReporter)
    {
        _logger = logger;
        _issueReporter = issueReporter;
    }

    public void OnStateChanged(State state)
    {
        if (state is Disconnected disconnectedState)
        {
            if (disconnectedState.error is null)
            {
                InvokeState(new(VpnStatus.Disconnected, VpnProtocol.Smart));
            }
            else
            {
                _logger.Error<ConnectionErrorLog>($"ProTUN disconnected with error: {disconnectedState.error}");
                InvokeState(new(VpnStatus.Disconnected, VpnError.Unknown, VpnProtocol.Smart));
            }
        }
        else if (state is Connected connectedState)
        {
            InvokeStateWithPeer(VpnStatus.Connected, connectedState.peer);
        }
        else if (state is WaitingForAction)
        {
            InvokeState(new(VpnStatus.Waiting, VpnProtocol.Smart));
        }
        else if (state is Connecting connectingState)
        {
            PeerConnectionInfo? peer = connectingState.peers.FirstOrDefault();
            if (peer is null) // ProTUN sends connecting without peers on a change of peers, or network availability change
            {
                InvokeState(new(VpnStatus.Connecting, VpnProtocol.Smart));
            }
            else
            {
                InvokeStateWithPeer(VpnStatus.Connecting, peer);
            }
        }
        else
        {
            string message = $"The ProTUN state '{state?.GetType().FullName}' is not implemented.";
            _logger.Error<ProTunProtocolLog>(message);
            _issueReporter.CaptureError(message);
        }
    }

    private void InvokeStateWithPeer(VpnStatus vpnStatus, PeerConnectionInfo peer)
    {
        InvokeState(new(vpnStatus,
            remoteIp: peer.entryIp,
            endpointPort: peer.port,
            vpnProtocol: MapProtocol(peer.protocol),
            openVpnAdapter: null,
            label: GetLabelFromId(peer.peerId)));
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

    private void InvokeState(VpnState vpnState)
    {
        StateChanged?.Invoke(this, new EventArgs<VpnState>(vpnState));
    }
}
