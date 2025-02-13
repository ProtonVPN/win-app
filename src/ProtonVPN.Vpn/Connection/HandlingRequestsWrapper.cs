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
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Common.Legacy;
using ProtonVPN.Common.Legacy.Threading;
using ProtonVPN.Common.Legacy.Vpn;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.ConnectLogs;
using ProtonVPN.Logging.Contracts.Events.DisconnectLogs;
using ProtonVPN.Vpn.Common;

namespace ProtonVPN.Vpn.Connection;

/// <summary>
/// Handles <see cref="Connect"/> and <see cref="Disconnect"/> requests by disconnecting
/// first if not disconnected yet and connecting only when disconnect completes.
/// A wrapper around <see cref="ISingleVpnConnection"/>.
/// </summary>
/// <remarks>
/// On connect request sends queued <see cref="StateChanged"/> event containing status value <see cref="VpnStatus.Connecting"/>.
/// </remarks>
public class HandlingRequestsWrapper : ISingleVpnConnection
{
    private readonly ISingleVpnConnection _origin;
    private readonly ILogger _logger;
    private readonly ITaskQueue _taskQueue;

    private bool _connectRequested;
    private bool _connecting;
    private bool _disconnectRequested;
    private bool _disconnecting;
    private bool _disconnected;
    private VpnError _disconnectError;
    private VpnEndpoint _endpoint;
    private VpnCredentials _credentials;
    private VpnConfig _config;

    public HandlingRequestsWrapper(
        ILogger logger,
        ITaskQueue taskQueue,
        ISingleVpnConnection origin)
    {
        _logger = logger;
        _taskQueue = taskQueue;
        _origin = origin;

        _origin.StateChanged += Origin_StateChanged;
    }

    public event EventHandler<EventArgs<VpnState>> StateChanged;
    public event EventHandler<ConnectionDetails> ConnectionDetailsChanged
    {
        add => _origin.ConnectionDetailsChanged += value;
        remove => _origin.ConnectionDetailsChanged -= value;
    }

    public NetworkTraffic NetworkTraffic => _origin.NetworkTraffic;

    public void Connect(VpnEndpoint endpoint, VpnCredentials credentials, VpnConfig config)
    {
        _endpoint = endpoint;
        _credentials = credentials;
        _config = config;

        _connectRequested = true;
        _disconnectRequested = false;
        _disconnectError = VpnError.Unknown;

        _logger.Info<ConnectLog>("HandlingRequestsWrapper: Connect requested, queuing Connect");
        Queued(Connect);
    }

    public void Disconnect(VpnError error)
    {
        _connectRequested = false;
        _disconnectRequested = true;
        _disconnectError = error;

        _logger.Info<DisconnectLog>("HandlingRequestsWrapper: Disconnect requested, queuing Disconnect");
        Queued(() => Disconnect(VpnProtocol.Smart));
    }

    public void SetFeatures(VpnFeatures vpnFeatures)
    {
        _origin.SetFeatures(vpnFeatures);
    }

    public void RequestNetShieldStats()
    {
        _origin.RequestNetShieldStats();
    }

    public void RequestConnectionDetails()
    {
        _origin.RequestConnectionDetails();
    }

    private void Origin_StateChanged(object sender, EventArgs<VpnState> e)
    {
        OnStateChanged(e.Data);
    }

    private void OnStateChanged(VpnState state)
    {
        _disconnected = state.Status == VpnStatus.Disconnected;

        if (_connectRequested)
        {
            if (_disconnected)
            {
                _logger.Info<ConnectLog>("HandlingRequestsWrapper: Already disconnected, queuing Connect");
                Queued(Connect);
            }

            return;
        }

        if (_disconnectRequested || _disconnecting)
        {
            if (state.Status == VpnStatus.Disconnecting || state.Status == VpnStatus.Disconnected)
            {
                InvokeStateChanged(state.WithError(_disconnectError));
                return;
            }

            InvokeDisconnecting(state.VpnProtocol);

            return;
        }

        if (_connecting && state.Status == VpnStatus.Disconnecting)
        {
            // Force disconnect if disconnected while connecting
            _disconnectRequested = true;
            _disconnectError = state.Error;
            _logger.Info<DisconnectLog>("HandlingRequestsWrapper: Disconnecting unexpectedly, queuing Disconnect");
            Queued(() => Disconnect(state.VpnProtocol));
        }

        if (state.Status == VpnStatus.Disconnecting || state.Status == VpnStatus.Disconnected)
        {
            VpnError error = state.Error;

            if (_connecting)
            {
                // Force disconnect if disconnected while connecting
                _disconnectRequested = true;
                _disconnectError = error;
                _logger.Info<DisconnectLog>("HandlingRequestsWrapper: Disconnecting unexpectedly, queuing Disconnect");
                Queued(() => Disconnect(state.VpnProtocol));
                return;
            }

            InvokeStateChanged(state.WithError(error));
            return;
        }

        InvokeStateChanged(WithFallbackRemoteServer(state, _endpoint));
    }

    private void Connect()
    {
        if (_connectRequested)
        {
            _disconnecting = false;

            if (_disconnected)
            {
                _connectRequested = false;
                _connecting = true;
                _logger.Info<ConnectLog>("HandlingRequestsWrapper: Connecting");
                _origin.Connect(_endpoint, _credentials, _config);
            }
            else
            {
                _connecting = false;
                _logger.Info<DisconnectLog>("HandlingRequestsWrapper: Not yet disconnected, Disconnecting");
                _origin.Disconnect(VpnError.None);
            }
        }
    }

    private void Disconnect(VpnProtocol vpnProtocol)
    {
        if (_disconnectRequested)
        {
            _disconnecting = true;

            if (_disconnected)
            {
                InvokeDisconnected(vpnProtocol);
            }
            else
            {
                InvokeDisconnecting(vpnProtocol);

                _connecting = false;
                _logger.Info<DisconnectLog>("HandlingRequestsWrapper: Disconnecting");
                _origin.Disconnect(_disconnectError);
            }
        }
    }

    private void InvokeDisconnecting(VpnProtocol vpnProtocol)
    {
        InvokeStateChanged(new VpnState(
            VpnStatus.Disconnecting,
            _disconnectError,
            vpnProtocol));
    }

    private void InvokeDisconnected(VpnProtocol vpnProtocol)
    {
        InvokeStateChanged(new VpnState(
            VpnStatus.Disconnected,
            _disconnectError,
            vpnProtocol));
    }

    private void InvokeStateChanged(VpnState state)
    {
        StateChanged?.Invoke(this, new EventArgs<VpnState>(state));
    }

    private VpnState WithFallbackRemoteServer(VpnState state, VpnEndpoint endpoint)
    {
        if (state.Status == VpnStatus.Disconnecting ||
            state.Status == VpnStatus.Disconnected ||
            !string.IsNullOrEmpty(state.RemoteIp))
        {
            return state;
        }

        return state.WithRemoteIp(endpoint.Server.Ip, endpoint.Port, endpoint.Server.Label);
    }

    private void Queued(Action action)
    {
        _taskQueue.Enqueue(action);
    }
}