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

using System.Text;
using ProtonVPN.Client.Common.Dispatching;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.ProcessCommunicationLogs;
using ProtonVPN.ProcessCommunication.Contracts.Controllers;
using ProtonVPN.ProcessCommunication.Contracts.Entities.NetShield;
using ProtonVPN.ProcessCommunication.Contracts.Entities.PortForwarding;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Update;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;

namespace ProtonVPN.Client.Logic.Connection;

public class AppController : IAppController
{
    private readonly ILogger _logger;
    private readonly IUIThreadDispatcher _uiThreadDispatcher;
    private readonly IEventMessageSender _eventMessageSender;

    public AppController(ILogger logger, 
        IUIThreadDispatcher uiThreadDispatcher, 
        IEventMessageSender eventMessageSender)
    {
        _logger = logger;
        _uiThreadDispatcher = uiThreadDispatcher;
        _eventMessageSender = eventMessageSender;
    }

    public async Task VpnStateChange(VpnStateIpcEntity state)
    {
        _logger.Debug<ProcessCommunicationLog>($"Received VPN Status '{state.Status}', NetworkBlocked: {state.NetworkBlocked} " +
            $"Error: '{state.Error}', EndpointIp: '{state.EndpointIp}', Label: '{state.Label}', " +
            $"VpnProtocol: '{state.VpnProtocol}', OpenVpnAdapter: '{state.OpenVpnAdapterType}'");

        _uiThreadDispatcher.TryEnqueue(() => _eventMessageSender.Send(state));
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
        _logger.Debug<ProcessCommunicationLog>(logMessage.ToString());

        _uiThreadDispatcher.TryEnqueue(() => _eventMessageSender.Send(state));
    }

    public async Task ConnectionDetailsChange(ConnectionDetailsIpcEntity connectionDetails)
    {
        _logger.Info<ProcessCommunicationLog>($"Received connection details change while " +
            $"connected to server with IP '{connectionDetails.ServerIpAddress}'");

        _uiThreadDispatcher.TryEnqueue(() => _eventMessageSender.Send(connectionDetails));
    }

    public async Task UpdateStateChange(UpdateStateIpcEntity updateStateDetails)
    {
        _logger.Info<ProcessCommunicationLog>(
            $"Received update state change with status {updateStateDetails.Status}.");

        _uiThreadDispatcher.TryEnqueue(() => _eventMessageSender.Send(updateStateDetails));
    }

    public async Task NetShieldStatisticChange(NetShieldStatisticIpcEntity netShieldStatistic)
    {
        _logger.Info<ProcessCommunicationLog>(
            $"Received NetShield statistic change with timestamp '{netShieldStatistic.TimestampUtc}' " +
            $"[Ads: '{netShieldStatistic.NumOfAdvertisementUrlsBlocked}']" +
            $"[Malware: '{netShieldStatistic.NumOfMaliciousUrlsBlocked}']" +
            $"[Trackers: '{netShieldStatistic.NumOfTrackingUrlsBlocked}']");

        // VPNWIN-1768 - Should invoke something
    }
}