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
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.ProcessCommunicationLogs;
using ProtonVPN.ProcessCommunication.Contracts.Controllers;
using ProtonVPN.ProcessCommunication.Contracts.Entities.NetShield;
using ProtonVPN.ProcessCommunication.Contracts.Entities.PortForwarding;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Update;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;

namespace ProtonVPN.Core.Service.Vpn
{
    public class AppController : IOldAppController
    {
        private readonly ILogger _logger;

        private static string SanitizeForLog(string input)
        {
            if (input == null) return string.Empty;
            return input.Replace("\r", "").Replace("\n", "");
        }
        public event EventHandler<VpnStateIpcEntity> OnVpnStateChanged;
        public event EventHandler<PortForwardingStateIpcEntity> OnPortForwardingStateChanged;
        public event EventHandler<ConnectionDetailsIpcEntity> OnConnectionDetailsChanged;
        public event EventHandler<NetShieldStatisticIpcEntity> OnNetShieldStatisticChanged;
        public event EventHandler<UpdateStateIpcEntity> OnUpdateStateChanged;
        public event EventHandler OnOpenWindowInvoked;

        public AppController(ILogger logger)
        {
            _logger = logger;
        }

        public async Task VpnStateChange(VpnStateIpcEntity state)
        {
            _logger.Info<ProcessCommunicationLog>($"Received VPN Status '{SanitizeForLog(state.Status)}', NetworkBlocked: {state.NetworkBlocked} " +
                $"Error: '{SanitizeForLog(state.Error)}', EndpointIp: '{SanitizeForLog(state.EndpointIp)}', Label: '{SanitizeForLog(state.Label)}', " +
                $"VpnProtocol: '{SanitizeForLog(state.VpnProtocol)}', OpenVpnAdapter: '{SanitizeForLog(state.OpenVpnAdapterType)}'");
            InvokeOnUiThread(() => OnVpnStateChanged?.Invoke(this, state));
        }

        private void InvokeOnUiThread(Action action)
        {
            Application.Current?.Dispatcher?.Invoke(action);
        }

        public async Task PortForwardingStateChange(PortForwardingStateIpcEntity state)
        {
            StringBuilder logMessage = new StringBuilder().Append("Received PortForwarding " +
                $"Status '{state.Status}' triggered at '{state.TimestampUtc}'");
            if (state.MappedPort is not null)
            {
                TemporaryMappedPortIpcEntity mappedPort = state.MappedPort;
                logMessage.Append($", Port pair {mappedPort.InternalPort}->{mappedPort.ExternalPort}, expiring in " +
                                  $"{mappedPort.Lifetime} at {mappedPort.ExpirationDateUtc}");
            }
            _logger.Info<ProcessCommunicationLog>(logMessage.ToString());
            InvokeOnUiThread(() => OnPortForwardingStateChanged?.Invoke(this, state));
        }

        public async Task ConnectionDetailsChange(ConnectionDetailsIpcEntity connectionDetails)
        {
            _logger.Info<ProcessCommunicationLog>($"Received connection details change while " +
                $"connected to server with IP '{connectionDetails.ServerIpAddress}'");
            InvokeOnUiThread(() => OnConnectionDetailsChanged?.Invoke(this, connectionDetails));
        }

        public async Task UpdateStateChange(UpdateStateIpcEntity updateStateDetails)
        {
            _logger.Info<ProcessCommunicationLog>(
                $"Received update state change with status {updateStateDetails.Status}.");
            InvokeOnUiThread(() => OnUpdateStateChanged?.Invoke(this, updateStateDetails));
        }

        public async Task NetShieldStatisticChange(NetShieldStatisticIpcEntity netShieldStatistic)
        {
            _logger.Info<ProcessCommunicationLog>(
                $"Received NetShield statistic change with timestamp '{netShieldStatistic.TimestampUtc}' " +
                $"[Ads: '{netShieldStatistic.NumOfAdvertisementUrlsBlocked}']" +
                $"[Malware: '{netShieldStatistic.NumOfMaliciousUrlsBlocked}']" +
                $"[Trackers: '{netShieldStatistic.NumOfTrackingUrlsBlocked}']");
            InvokeOnUiThread(() => OnNetShieldStatisticChanged?.Invoke(this, netShieldStatistic));
        }

        public async Task OpenWindow()
        {
            _logger.Debug<ProcessCommunicationLog>("Another process requested to open the main window.");
            InvokeOnUiThread(() => OnOpenWindowInvoked?.Invoke(this, null));
        }
    }
}