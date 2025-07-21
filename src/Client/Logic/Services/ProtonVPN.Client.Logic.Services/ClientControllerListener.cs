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
using ProtonVPN.Client.Common.Messages;
using ProtonVPN.Client.Contracts.ProcessCommunication;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Services.Contracts;
using ProtonVPN.Common.Core.Extensions;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.ProcessCommunicationLogs;
using ProtonVPN.ProcessCommunication.Contracts;
using ProtonVPN.ProcessCommunication.Contracts.Entities.NetShield;
using ProtonVPN.ProcessCommunication.Contracts.Entities.PortForwarding;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Update;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;

namespace ProtonVPN.Client.Logic.Services;

public class ClientControllerListener : IClientControllerListener, IEventMessageReceiver<ApplicationStoppedMessage>
{
    private readonly ILogger _logger;
    private readonly IGrpcClient _grpcClient;
    private readonly IEventMessageSender _eventMessageSender;
    private readonly IServiceCommunicationErrorHandler _serviceCommunicationErrorHandler;

    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public ClientControllerListener(ILogger logger,
        IGrpcClient grpcClient,
        IEventMessageSender eventMessageSender,
        IServiceCommunicationErrorHandler serviceCommunicationErrorHandler)
    {
        _logger = logger;
        _grpcClient = grpcClient;
        _eventMessageSender = eventMessageSender;
        _serviceCommunicationErrorHandler = serviceCommunicationErrorHandler;
    }

    public void Receive(ApplicationStoppedMessage message)
    {
        Stop();
    }

    public void Stop()
    {
        _cancellationTokenSource.Cancel();
    }

    public void Start()
    {
        Task.Run(() => KeepAliveAsync(StartVpnStateListenerAsync)).FireAndForget();
        Task.Run(() => KeepAliveAsync(StartPortForwardingStateListenerAsync)).FireAndForget();
        Task.Run(() => KeepAliveAsync(StartConnectionDetailsListenerAsync)).FireAndForget();
        Task.Run(() => KeepAliveAsync(StartUpdateStateListenerAsync)).FireAndForget();
        Task.Run(() => KeepAliveAsync(StartNetShieldStatisticListenerAsync)).FireAndForget();
    }

    private async Task KeepAliveAsync(Func<Task> listener)
    {
        while (!_cancellationTokenSource.IsCancellationRequested)
        {
            try
            {
                _logger.Info<ProcessCommunicationLog>($"Listener starting ({listener.Method.Name})");
                await listener();
            }
            catch (Exception ex)
            {
                _logger.Warn<ProcessCommunicationLog>($"Listener stopped ({listener.Method.Name})", ex);
            }

            if (!_cancellationTokenSource.IsCancellationRequested)
            {
                await _serviceCommunicationErrorHandler.HandleAsync();
            }
        }
    }

    private async Task StartVpnStateListenerAsync()
    {
        await foreach (VpnStateIpcEntity state in
            _grpcClient.ClientController.StreamVpnStateChangeAsync(_cancellationTokenSource.Token))
        {
            _logger.Info<ProcessCommunicationLog>($"Received VPN Status '{state.Status}', NetworkBlocked: {state.NetworkBlocked} " +
            $"Error: '{state.Error}', EndpointIp: '{state.EndpointIp}', Label: '{state.Label}', " +
            $"VpnProtocol: '{state.VpnProtocol}', OpenVpnAdapter: '{state.OpenVpnAdapterType}'");

            _eventMessageSender.Send(state);
        }
    }

    private async Task StartPortForwardingStateListenerAsync()
    {
        await foreach (PortForwardingStateIpcEntity state in
            _grpcClient.ClientController.StreamPortForwardingStateChangeAsync(_cancellationTokenSource.Token))
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

            _eventMessageSender.Send(state);
        }
    }

    private async Task StartConnectionDetailsListenerAsync()
    {
        await foreach (ConnectionDetailsIpcEntity connectionDetails in
            _grpcClient.ClientController.StreamConnectionDetailsChangeAsync(_cancellationTokenSource.Token))
        {
            _logger.Info<ProcessCommunicationLog>($"Received connection details change while " +
                $"connected to server with IP '{connectionDetails.ServerIpAddress}'");

            _eventMessageSender.Send(connectionDetails);
        }
    }

    private async Task StartUpdateStateListenerAsync()
    {
        await foreach (UpdateStateIpcEntity state in
            _grpcClient.ClientController.StreamUpdateStateChangeAsync(_cancellationTokenSource.Token))
        {
            _logger.Info<ProcessCommunicationLog>(
                $"Received update state change with status {state.Status}.");

            _eventMessageSender.Send(state);
        }
    }

    private async Task StartNetShieldStatisticListenerAsync()
    {
        await foreach (NetShieldStatisticIpcEntity netShieldStatistic in
            _grpcClient.ClientController.StreamNetShieldStatisticChangeAsync(_cancellationTokenSource.Token))
        {
            _logger.Info<ProcessCommunicationLog>(
                $"Received NetShield statistic change with timestamp '{netShieldStatistic.TimestampUtc}' " +
                $"[Ads: '{netShieldStatistic.NumOfAdvertisementUrlsBlocked}']" +
                $"[Malware: '{netShieldStatistic.NumOfMaliciousUrlsBlocked}']" +
                $"[Trackers: '{netShieldStatistic.NumOfTrackingUrlsBlocked}']");

            _eventMessageSender.Send(netShieldStatistic);
        }
    }
}