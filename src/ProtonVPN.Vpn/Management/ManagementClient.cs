﻿/*
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
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ProtonVPN.Common;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.ConnectionLogs;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Vpn.Common;
using ProtonVPN.Vpn.Gateways;

namespace ProtonVPN.Vpn.Management
{
    /// <summary>
    /// Interacts with the OpenVPN over management interface.
    /// </summary>
    internal class ManagementClient
    {
        private readonly ILogger _logger;
        private readonly MessagingManagementChannel _managementChannel;
        private readonly IGatewayCache _gatewayCache;

        private VpnError _lastError;
        private VpnCredentials _credentials;
        private VpnEndpoint _endpoint;
        private bool _sendingFailed;
        private bool _disconnectRequested;
        private bool _disconnectAccepted;

        public ManagementClient(ILogger logger, IGatewayCache gatewayCache, IManagementChannel managementChannel)
            : this(logger, gatewayCache, new MessagingManagementChannel(logger, managementChannel))
        {
        }

        internal ManagementClient(ILogger logger, IGatewayCache gatewayCache, MessagingManagementChannel managementChannel)
        {
            _logger = logger;
            _gatewayCache = gatewayCache;
            _managementChannel = managementChannel;
        }

        public event EventHandler<EventArgs<InOutBytes>> TransportStatsChanged;

        public event EventHandler<EventArgs<VpnState>> VpnStateChanged;


        /// <summary>
        /// Connects to OpenVPN management interface.
        /// </summary>
        /// <param name="port">TCP port number of management interface</param>
        /// <param name="password">Password of management interface</param>
        /// <returns></returns>
        public async Task Connect(int port, string password)
        {
            await _managementChannel.Connect(port, password);
        }

        /// <summary>
        /// Primary VPN connect method, doesn't finish until disconnect.
        /// This method will raise <see cref="TransportStatsChanged"/>, <see cref="VpnStateChanged"/>
        /// </summary>
        /// <param name="credentials"><see cref="VpnCredentials"/> (username and password) for authenticating to VPN server</param>
        /// <param name="endpoint"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task StartVpnConnection(VpnCredentials credentials, VpnEndpoint endpoint, CancellationToken cancellationToken)
        {
            _lastError = VpnError.None;
            _credentials = credentials;
            _endpoint = endpoint;
            _sendingFailed = false;
            _disconnectRequested = false;
            _disconnectAccepted = false;

            while (!cancellationToken.IsCancellationRequested && !_sendingFailed)
            {
                ReceivedManagementMessage message = await Receive();
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
                    HandleMessage(message);
                }
            }

            if (!_sendingFailed)
            {
                await SendExit();
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
        public async Task CloseVpnConnection()
        {
            _disconnectRequested = true;
            await TrySend(_managementChannel.Messages.Disconnect());
        }

        /// <summary>
        /// Disconnects from OpenVPN management interface.
        /// </summary>
        /// <returns></returns>
        public void Disconnect()
        {
            _managementChannel.Disconnect();
        }

        private async void HandleMessage(ReceivedManagementMessage message)
        {
            bool handled = false;

            if (message.IsState)
            {
                await HandleStateMessage(message);
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
                await TrySend(_managementChannel.Messages.Username(_credentials.Username));
                handled = true;
            }
            else if (message.IsPasswordNeeded)
            {
                await TrySend(_managementChannel.Messages.Password(_credentials.Password));
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
                await TrySend(_managementChannel.Messages.Disconnect());
            }
            else if (message.IsWaitingHoldRelease)
            {
                await TrySend(_managementChannel.Messages.EchoOn());
            }
            else if (message.IsEchoSet)
            {
                await TrySend(_managementChannel.Messages.StateOn());
            }
            else if (message.IsStateSet)
            {
                await TrySend(_managementChannel.Messages.Bytecount());
            }
            else if (message.IsByteCountSet)
            {
                await TrySend(_managementChannel.Messages.LogOn());
            }
            else if (message.IsLogSet)
            {
                await TrySend(_managementChannel.Messages.HoldRelease());
            }
        }

        private void HandleControlMessage(ReceivedManagementMessage message)
        {
            MatchCollection regex = Regex.Matches(message.ToString(), @"route-gateway ([0-9]+\.[0-9]+\.[0-9]+\.[0-9]+)");
            if (regex.Count > 0 && regex[0].Groups.Count >= 2)
            {
                IPAddress gatewayIPAddress = IPAddress.Parse(regex[0].Groups[1].Value);
                _gatewayCache.Save(gatewayIPAddress);
            }
        }

        private void HandleByteMessage(ReceivedManagementMessage message)
        {
            InOutBytes bandwidth = message.Bandwidth();
            OnTransportStatsChanged(bandwidth);
        }

        private void HandleErrorMessage(ReceivedManagementMessage message)
        {
            _lastError = message.Error().VpnError();
        }

        private async Task HandleStateMessage(ReceivedManagementMessage message)
        {
            ManagementState managementState = message.State();

            if (managementState.HasError)
            {
                await TrySend(_managementChannel.Messages.Disconnect());

                if (_lastError == VpnError.None)
                {
                    _lastError = managementState.Error;
                }
            }
            else
            {
                if (managementState.HasStatus)
                {
                    OnVpnStateChanged(new VpnState(managementState.Status, _lastError,
                        managementState.LocalIpAddress, managementState.RemoteIpAddress, default, label: _endpoint.Server.Label));
                }
            }
        }

        private async Task<ReceivedManagementMessage> Receive()
        {
            try
            {
                return await _managementChannel.ReadMessage();
            }
            catch (IOException ex)
            {
                _logger.Warn<ConnectionErrorLog>($"Failed to read message from OpenVPN management interface: {ex.Message}");
                return _managementChannel.Messages.ReceivedMessage("");
            }
        }

        private Task SendExit()
        {
            return TrySend(_managementChannel.Messages.Exit());
        }

        private async Task TrySend(ManagementMessage message)
        {
            try
            {
                await _managementChannel.WriteMessage(message);
                _sendingFailed = false;
            }
            catch (IOException ex)
            {
                _sendingFailed = true;
                _logger.Warn<ConnectionErrorLog>($"Sending message \"{message.LogText}\" to OpenVPN management interface failed: {ex.Message}");
            }
        }

        private void OnVpnStateChanged(VpnStatus status)
        {
            OnVpnStateChanged(new VpnState(status, _lastError, string.Empty, string.Empty, default));
        }

        private void OnVpnStateChanged(VpnState state)
        {
            VpnStateChanged?.Invoke(this, new EventArgs<VpnState>(state));
        }

        private void OnTransportStatsChanged(InOutBytes bandwidth)
        {
            TransportStatsChanged?.Invoke(this, new EventArgs<InOutBytes>(bandwidth));
        }
    }
}