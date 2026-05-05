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

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using ProtonVPN.Common.Core.Extensions;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Common.Legacy.Vpn;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.ConnectionLogs;
using ProtonVPN.Vpn.Common;
using ProtonVPN.Vpn.Gateways;

namespace ProtonVPN.Vpn.Management;

/// <summary>
/// Interacts with the OpenVPN over management interface.
/// </summary>
public partial class ManagementClient : IManagementClient
{
    private readonly ILogger _logger;
    private readonly IMessagingManagementChannel _managementChannel;
    private readonly IGatewayCache _gatewayCache;
    private readonly IDnsServerCache _dnsServerCache;

    private VpnError _lastError;
    private VpnCredentials _credentials;
    private VpnEndpoint? _endpoint;
    private bool _sendingFailed;
    private bool _disconnectRequested;
    private bool _disconnectAccepted;

    [GeneratedRegex(@"dhcp-option DNS ([^,]+)(?=,|$)")]
    private static partial Regex DhcpRegex();

    public Channel<VpnState> StateChannel { get; private set; } = Channel.CreateUnbounded<VpnState>();
    public NetworkTraffic NetworkTraffic { get; private set; } = NetworkTraffic.Zero;

    public ManagementClient(
        ILogger logger,
        IGatewayCache gatewayCache,
        IDnsServerCache dnsServerCache,
        IMessagingManagementChannel managementChannel)
    {
        _logger = logger;
        _gatewayCache = gatewayCache;
        _dnsServerCache = dnsServerCache;
        _managementChannel = managementChannel;
    }

    public void ResetState()
    {
        StateChannel = Channel.CreateUnbounded<VpnState>();
        NetworkTraffic = NetworkTraffic.Zero;
    }

    /// <summary>
    /// Connects to OpenVPN management interface.
    /// </summary>
    /// <param name="port">TCP port number of management interface</param>
    /// <param name="password">Password of management interface</param>
    /// <returns></returns>
    public async Task ConnectAsync(int port, string password, CancellationToken cancellationToken)
    {
        await _managementChannel.ConnectAsync(port, password, cancellationToken);
    }

    /// <summary>
    /// Primary VPN connect method, doesn't finish until disconnect.
    /// This method will write to <see cref="TransportStatsChannel"/> and <see cref="StateChannel"/>.
    /// </summary>
    /// <param name="credentials"><see cref="VpnCredentials"/> (username and password) for authenticating to VPN server</param>
    /// <param name="endpoint"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task StartVpnConnectionAsync(VpnCredentials credentials, VpnEndpoint endpoint, CancellationToken cancellationToken)
    {
        _lastError = VpnError.None;
        _credentials = credentials;
        _endpoint = endpoint;
        _sendingFailed = false;
        _disconnectRequested = false;
        _disconnectAccepted = false;

        while (!cancellationToken.IsCancellationRequested && !_sendingFailed)
        {
            ReceivedManagementMessage message = await ReceiveAsync(cancellationToken);
            if (message.IsChannelDisconnected)
            {
                if (!_disconnectRequested && _lastError == VpnError.None)
                {
                    _lastError = VpnError.Unknown;
                }

                OnVpnStateChanged(VpnStatus.Disconnecting);
                return;
            }

            if (!cancellationToken.IsCancellationRequested)
            {
                await HandleMessageAsync(message, cancellationToken);
            }
        }

        if (!_sendingFailed)
        {
            await SendExitAsync(cancellationToken);
        }

        if (!cancellationToken.IsCancellationRequested && _sendingFailed)
        {
            OnVpnStateChanged(VpnStatus.Disconnecting);
        }
    }

    /// <summary>
    /// Closes the VPN. Only meaningful while StartVpnConnection() is running.
    /// May be called asynchronously from a different thread when StartVpnConnection() is running.
    /// </summary>
    /// <returns></returns>
    public async Task CloseVpnConnectionAsync()
    {
        _disconnectRequested = true;
        await TrySendAsync(_managementChannel.Messages.Disconnect(), CancellationToken.None);
    }

    /// <summary>
    /// Disconnects from OpenVPN management interface.
    /// </summary>
    /// <returns></returns>
    public void Disconnect()
    {
        _managementChannel.Disconnect();
    }

    private async Task HandleMessageAsync(ReceivedManagementMessage message, CancellationToken cancellationToken)
    {
        bool handled = false;

        if (message.IsState)
        {
            await HandleStateMessageAsync(message, cancellationToken);
            handled = true;
        }
        else if (message.IsByteCount)
        {
            HandleByteMessage(message);
            handled = true;
        }
        else if (message.IsError)
        {
            HandleErrorMessage(message);
            handled = true;
        }
        else if (message.IsDisconnectReceived)
        {
            OnVpnStateChanged(VpnStatus.Disconnecting);
            _disconnectAccepted = true;
            handled = true;
        }
        else if (message.IsUsernameNeeded)
        {
            await TrySendAsync(_managementChannel.Messages.Username(_credentials.Username), cancellationToken);
            handled = true;
        }
        else if (message.IsPasswordNeeded)
        {
            await TrySendAsync(_managementChannel.Messages.Password(_credentials.Password), cancellationToken);
            handled = true;
        }
        else if (message.IsControlMessage)
        {
            HandleControlMessage(message);
            handled = true;
        }

        if (handled)
        {
            return;
        }

        if (_disconnectRequested && !_disconnectAccepted)
        {
            await TrySendAsync(_managementChannel.Messages.Disconnect(), cancellationToken);
        }
        else if (message.IsWaitingHoldRelease)
        {
            await TrySendAsync(_managementChannel.Messages.EchoOn(), cancellationToken);
        }
        else if (message.IsEchoSet)
        {
            await TrySendAsync(_managementChannel.Messages.StateOn(), cancellationToken);
        }
        else if (message.IsStateSet)
        {
            await TrySendAsync(_managementChannel.Messages.Bytecount(), cancellationToken);
        }
        else if (message.IsByteCountSet)
        {
            await TrySendAsync(_managementChannel.Messages.LogOn(), cancellationToken);
        }
        else if (message.IsLogSet)
        {
            await TrySendAsync(_managementChannel.Messages.HoldRelease(), cancellationToken);
        }
    }

    private void HandleControlMessage(ReceivedManagementMessage message)
    {
        string messageString = message.ToString();
        HandleRouteGateway(messageString);
        HandleDnsServers(messageString);
    }

    private void HandleRouteGateway(string message)
    {
        MatchCollection regexResult = Regex.Matches(message, @"route-gateway ((25[0-5]|2[0-4]\d|1?\d{1,2})(\.(25[0-5]|2[0-4]\d|1?\d{1,2})){3})");
        if (regexResult.Count > 0 && regexResult[0].Groups.Count >= 2)
        {
            IPAddress gatewayIPAddress = IPAddress.Parse(regexResult[0].Groups[1].Value);
            _gatewayCache.Save(gatewayIPAddress);
        }
    }

    private void HandleDnsServers(string message)
    {
        MatchCollection regexResult = DhcpRegex().Matches(message);
        List<IPAddress> dnsServerIpAddresses = [];
        foreach (Match match in regexResult)
        {
            dnsServerIpAddresses.AddIfNotNull(ParseDnsServerIpAddress(match));
        }
        _dnsServerCache.Save(dnsServerIpAddresses);
    }

    private IPAddress? ParseDnsServerIpAddress(Match match)
    {
        return match.Groups.Count >= 2
            ? IPAddress.Parse(match.Groups[1].Value)
            : null;
    }

    private void HandleByteMessage(ReceivedManagementMessage message)
    {
        NetworkTraffic bandwidth = message.Bandwidth();
        OnTransportStatsChanged(bandwidth);
    }

    private void HandleErrorMessage(ReceivedManagementMessage message)
    {
        _lastError = message.Error().GetVpnError();
    }

    private async Task HandleStateMessageAsync(ReceivedManagementMessage message, CancellationToken cancellationToken)
    {
        ManagementState managementState = message.State();

        if (managementState.HasError)
        {
            await TrySendAsync(_managementChannel.Messages.Disconnect(), cancellationToken);

            if (_lastError == VpnError.None)
            {
                _lastError = managementState.Error;
            }
        }
        else
        {
            if (managementState.HasStatus)
            {
                OnVpnStateChanged(new VpnState(
                    managementState.Status,
                    _lastError,
                    managementState.LocalIpAddress ?? string.Empty,
                    managementState.RemoteIpAddress ?? string.Empty,
                    _endpoint?.Port ?? 0,
                    default,
                    label: _endpoint?.Server.Label ?? string.Empty));
            }
        }
    }

    private async Task<ReceivedManagementMessage> ReceiveAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await _managementChannel.ReadMessageAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return _managementChannel.Messages.ReceivedMessage("");
        }
        catch (IOException ex)
        {
            _logger.Warn<ConnectionErrorLog>($"Failed to read message from OpenVPN management interface: {ex.Message}");
            return _managementChannel.Messages.ReceivedMessage("");
        }
    }

    private Task SendExitAsync(CancellationToken cancellationToken)
    {
        return TrySendAsync(_managementChannel.Messages.Exit(), cancellationToken);
    }

    private async Task TrySendAsync(ManagementMessage message, CancellationToken cancellationToken)
    {
        try
        {
            await _managementChannel.WriteMessage(message, cancellationToken);
            _sendingFailed = false;
        }
        catch (OperationCanceledException)
        {
            // Ignore cancellation: the caller is shutting down the connection.
        }
        catch (IOException ex)
        {
            _sendingFailed = true;
            _logger.Warn<ConnectionErrorLog>($"Sending message \"{message.LogText}\" to OpenVPN management interface failed: {ex.Message}");
        }
    }

    private void OnVpnStateChanged(VpnStatus status)
    {
        OnVpnStateChanged(new VpnState(status, _lastError, string.Empty, _endpoint?.Server.Ip ?? string.Empty, _endpoint?.Port ?? 0, default));
    }

    private void OnVpnStateChanged(VpnState state)
    {
        StateChannel.Writer.TryWrite(state);
    }

    private void OnTransportStatsChanged(NetworkTraffic bandwidth)
    {
        NetworkTraffic = bandwidth;
    }
}