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
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using ProtonVPN.Account;
using ProtonVPN.Announcements.Contracts;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.ProcessCommunicationLogs;
using ProtonVPN.ProcessCommunication.Contracts.Controllers;
using ProtonVPN.ProcessCommunication.Contracts.Entities.NetShield;
using ProtonVPN.ProcessCommunication.Contracts.Entities.PortForwarding;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Update;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;

namespace ProtonVPN.Core.Service.Vpn
{
    public class AppController : IAppController
    {
        private readonly ILogger _logger;
        private readonly IUpgradeModalManager _upgradeModalManager;
        private readonly IAnnouncementService _announcementService;

        public event EventHandler<VpnStateIpcEntity> OnVpnStateChanged;
        public event EventHandler<PortForwardingStateIpcEntity> OnPortForwardingStateChanged;
        public event EventHandler<ConnectionDetailsIpcEntity> OnConnectionDetailsChanged;
        public event EventHandler<NetShieldStatisticIpcEntity> OnNetShieldStatisticChanged;
        public event EventHandler<UpdateStateIpcEntity> OnUpdateStateChanged;
        public event EventHandler OnOpenWindowInvoked;

        public AppController(ILogger logger, IUpgradeModalManager upgradeModalManager, IAnnouncementService announcementService)
        {
            _logger = logger;
            _upgradeModalManager = upgradeModalManager;
            _announcementService = announcementService;
        }

        public async Task VpnStateChange(VpnStateIpcEntity state)
        {
            _logger.Info<ProcessCommunicationLog>($"Received VPN Status '{state.Status}', NetworkBlocked: {state.NetworkBlocked} " +
                $"Error: '{state.Error}', EndpointIp: '{state.EndpointIp}', Label: '{state.Label}', " +
                $"VpnProtocol: '{state.VpnProtocol}', OpenVpnAdapter: '{state.OpenVpnAdapterType}'");
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

        public async Task OpenWindow(string args)
        {
            _logger.Debug<ProcessCommunicationLog>("Another process requested to open the main window.");
            await ProcessCommandArgumentsAsync(args);
            InvokeOnUiThread(() => OnOpenWindowInvoked?.Invoke(this, null));
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
            }
        }
    }
}