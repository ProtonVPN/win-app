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
using System.Collections.Specialized;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using ProtonVPN.Account;
using ProtonVPN.Announcements.Contracts;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Core.Service;
using ProtonVPN.Core.Service.Vpn;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;
using ProtonVPN.Logging.Contracts.Events.ProcessCommunicationLogs;
using ProtonVPN.ProcessCommunication.Contracts;
using ProtonVPN.ProcessCommunication.Contracts.Entities.NetShield;
using ProtonVPN.ProcessCommunication.Contracts.Entities.PortForwarding;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Update;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;

namespace ProtonVPN.Client.Logic.Connection;

public class ClientControllerListener : IClientControllerListener
{
    private readonly ILogger _logger;
    private readonly IGrpcClient _grpcClient;
    private readonly IAnnouncementService _announcementService;
    private readonly IUpgradeModalManager _upgradeModalManager;
    private readonly IClientControllerEventHandler _clientControllerEventHandler;
    private readonly IServiceCommunicationErrorHandler _serviceCommunicationErrorHandler;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public ClientControllerListener(ILogger logger,
        IGrpcClient grpcClient,
        IAnnouncementService announcementService,
        IUpgradeModalManager upgradeModalManager,
        IClientControllerEventHandler clientControllerEventHandler,
        IServiceCommunicationErrorHandler serviceCommunicationErrorHandler)
    {
        _logger = logger;
        _grpcClient = grpcClient;
        _announcementService = announcementService;
        _upgradeModalManager = upgradeModalManager;
        _clientControllerEventHandler = clientControllerEventHandler;
        _serviceCommunicationErrorHandler = serviceCommunicationErrorHandler;
    }

    public void Start()
    {
        KeepAliveAsync(StartVpnStateListenerAsync);
        KeepAliveAsync(StartPortForwardingStateListenerAsync);
        KeepAliveAsync(StartConnectionDetailsListenerAsync);
        KeepAliveAsync(StartUpdateStateListenerAsync);
        KeepAliveAsync(StartNetShieldStatisticListenerAsync);
        KeepAliveAsync(StartOpenWindowListenerAsync);
    }

    private async Task KeepAliveAsync(Func<Task> listener)
    {
        while (!_cancellationTokenSource.IsCancellationRequested)
        {
            try
            {
                _logger.Info<AppLog>($"Listener starting ({listener.Method.Name})");
                await listener();
            }
            catch
            {
                _logger.Warn<AppLog>($"Listener stopped ({listener.Method.Name})");
            }

            if (!_cancellationTokenSource.IsCancellationRequested)
            {
                await _serviceCommunicationErrorHandler.HandleAsync();
            }
        }
    }

    private async Task StartVpnStateListenerAsync()
    {
        await foreach (VpnStateIpcEntity state in _grpcClient.ClientController.StreamVpnStateChangeAsync())
        {

            _logger.Info<ProcessCommunicationLog>($"Received VPN Status '{state.Status}', " +
                $"NetworkBlocked: {state.NetworkBlocked} Error: '{state.Error}', EndpointIp: '{state.EndpointIp}', " +
                $"Label: '{state.Label}', VpnProtocol: '{state.VpnProtocol}', OpenVpnAdapter: '{state.OpenVpnAdapterType}'");
            InvokeOnUiThread(() => { _clientControllerEventHandler.InvokeVpnStateChanged(state); });
        }
    }

    private void InvokeOnUiThread(Action action)
    {
        Application.Current?.Dispatcher?.Invoke(action);
    }

    private async Task StartPortForwardingStateListenerAsync()
    {
        await foreach (PortForwardingStateIpcEntity state in
            _grpcClient.ClientController.StreamPortForwardingStateChangeAsync())
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
            InvokeOnUiThread(() => _clientControllerEventHandler.InvokePortForwardingStateChanged(state));
        }
    }

    private async Task StartConnectionDetailsListenerAsync()
    {
        await foreach (ConnectionDetailsIpcEntity connectionDetails in
            _grpcClient.ClientController.StreamConnectionDetailsChangeAsync())
        {
            _logger.Info<ProcessCommunicationLog>($"Received connection details change while " +
                $"connected to server with IP '{connectionDetails.ServerIpAddress}'");
            InvokeOnUiThread(() => _clientControllerEventHandler.InvokeConnectionDetailsChanged(connectionDetails));
        }
    }

    private async Task StartUpdateStateListenerAsync()
    {
        await foreach (UpdateStateIpcEntity state in
            _grpcClient.ClientController.StreamUpdateStateChangeAsync())
        {
            _logger.Info<ProcessCommunicationLog>(
                $"Received update state change with status {state.Status}.");
            InvokeOnUiThread(() => _clientControllerEventHandler.InvokeUpdateStateChanged(state));
        }
    }

    private async Task StartNetShieldStatisticListenerAsync()
    {
        await foreach (NetShieldStatisticIpcEntity netShieldStatistic in
            _grpcClient.ClientController.StreamNetShieldStatisticChangeAsync())
        {
            _logger.Info<ProcessCommunicationLog>(
                $"Received NetShield statistic change with timestamp '{netShieldStatistic.TimestampUtc}' " +
                $"[Ads: '{netShieldStatistic.NumOfAdvertisementUrlsBlocked}']" +
                $"[Malware: '{netShieldStatistic.NumOfMaliciousUrlsBlocked}']" +
                $"[Trackers: '{netShieldStatistic.NumOfTrackingUrlsBlocked}']");
            InvokeOnUiThread(() => _clientControllerEventHandler.InvokeNetShieldStatisticChanged(netShieldStatistic));
        }
    }

    private async Task StartOpenWindowListenerAsync()
    {
        await foreach (string args in _grpcClient.ClientController.StreamOpenWindowAsync())
        {
            _logger.Info<ProcessCommunicationLog>("Received open window request.");
            await ProcessCommandArgumentsAsync(args);
            InvokeOnUiThread(() => _clientControllerEventHandler.InvokeOpenWindowInvoked());
        }
    }

    private async Task ProcessCommandArgumentsAsync(string args)
    {
        if (Uri.TryCreate(args, UriKind.Absolute, out Uri uri))
        {
            await ProcessCommandUriArgumentAsync(uri);
        }
    }

    private async Task ProcessCommandUriArgumentAsync(Uri uri)
    {
        string modalSource = null;
        string notificationReference = null;

        NameValueCollection uriQuery = HttpUtility.ParseQueryString(uri.Query);
        foreach (string queryKey in uriQuery.AllKeys)
        {
            if (queryKey.EqualsIgnoringCase("delete-notification-id"))
            {
                _announcementService.Delete(uriQuery[queryKey]);
            }
            else if (queryKey.EqualsIgnoringCase("notification-reference"))
            {
                _announcementService.DeleteByReference(uriQuery[queryKey]);
                notificationReference = uriQuery[queryKey];
            }
            else if (queryKey.EqualsIgnoringCase("modal-source"))
            {
                modalSource = uriQuery[queryKey];
            }
        }

        if (uri.Host.EqualsIgnoringCase(SubscriptionManager.REFRESH_ACCOUNT_COMMAND))
        {
            await _upgradeModalManager.CheckForVpnPlanUpgradeAsync(modalSource, notificationReference);
            await _announcementService.UpdateAsync();
        }
    }

    public void Stop()
    {
        _cancellationTokenSource.Cancel();
    }
}
