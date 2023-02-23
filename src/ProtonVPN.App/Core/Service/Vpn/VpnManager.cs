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
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using ProtonVPN.Common.Events;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.ConnectionLogs;
using ProtonVPN.Common.Logging.Categorization.Events.ConnectLogs;
using ProtonVPN.Common.Logging.Categorization.Events.DisconnectLogs;
using ProtonVPN.Common.Threading;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Profiles;
using ProtonVPN.Core.Vpn;

namespace ProtonVPN.Core.Service.Vpn
{
    public class VpnManager : IVpnManager
    {
        private readonly ITaskQueue _taskQueue = new SerialTaskQueue();
        private readonly IVpnServiceManager _vpnServiceManager;
        private readonly IVpnReconnector _vpnReconnector;
        private readonly IVpnConnector _vpnConnector;
        private readonly ILogger _logger;
        private readonly IEventPublisher _eventPublisher;

        public VpnManager(
            IVpnServiceManager vpnServiceManager,
            IVpnReconnector vpnReconnector,
            IVpnConnector vpnConnector,
            ILogger logger,
            IEventPublisher eventPublisher)
        {
            _vpnServiceManager = vpnServiceManager;
            _vpnReconnector = vpnReconnector;
            _vpnConnector = vpnConnector;
            _logger = logger;
            _eventPublisher = eventPublisher;
            _vpnConnector.VpnStateChanged += OnConnectorVpnStateChanged;
        }

        public event EventHandler<VpnStateChangedEventArgs> VpnStateChanged;

        private void OnConnectorVpnStateChanged(object sender, VpnStateChangedEventArgs e)
        {
            VpnStateChanged?.Invoke(this, e);
        }

        public void OnVpnStateChanged(VpnStateChangedEventArgs e)
        {
            _vpnConnector.OnVpnStateChanged(e);
        }

        public async Task ConnectAsync(Profile profile, Profile fallbackProfile = null,
            [CallerFilePath] string sourceFilePath = "",
            [CallerMemberName] string sourceMemberName = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            _logger.Info<ConnectTriggerLog>("[VpnManager] Profile connect requested.", sourceFilePath: sourceFilePath,
                sourceMemberName: sourceMemberName, sourceLineNumber: sourceLineNumber);
            await EnqueueAsync(() => _vpnConnector.ConnectToBestProfileAsync(profile, fallbackProfile),
                sourceFilePath: sourceFilePath, sourceMemberName: sourceMemberName, sourceLineNumber: sourceLineNumber);
        }

        public async Task QuickConnectAsync(
            [CallerFilePath] string sourceFilePath = "",
            [CallerMemberName] string sourceMemberName = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            _logger.Info<ConnectTriggerLog>("[VpnManager] Quick connect requested.", sourceFilePath: sourceFilePath,
                sourceMemberName: sourceMemberName, sourceLineNumber: sourceLineNumber);
            await EnqueueAsync(() => _vpnConnector.QuickConnectAsync(),
                sourceFilePath: sourceFilePath, sourceMemberName: sourceMemberName, sourceLineNumber: sourceLineNumber);
        }

        public async Task ReconnectAsync(VpnReconnectionSettings settings = null,
            [CallerFilePath] string sourceFilePath = "",
            [CallerMemberName] string sourceMemberName = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            _logger.Info<ConnectTriggerLog>("[VpnManager] Reconnect requested.", sourceFilePath: sourceFilePath,
                sourceMemberName: sourceMemberName, sourceLineNumber: sourceLineNumber);
            await EnqueueAsync(() => _vpnReconnector.ReconnectAsync(_vpnConnector.LastServer, _vpnConnector.LastProfile, settings),
                sourceFilePath: sourceFilePath, sourceMemberName: sourceMemberName, sourceLineNumber: sourceLineNumber);
        }

        public async Task DisconnectAsync(VpnError vpnError = VpnError.None,
            [CallerFilePath] string sourceFilePath = "",
            [CallerMemberName] string sourceMemberName = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            _logger.Info<DisconnectTriggerLog>("[VpnManager] Disconnect requested.", sourceFilePath: sourceFilePath,
                sourceMemberName: sourceMemberName, sourceLineNumber: sourceLineNumber);
            _vpnReconnector.OnDisconnectionRequest();
            await EnqueueAsync(() => _vpnServiceManager.Disconnect(vpnError),
                sourceFilePath: sourceFilePath, sourceMemberName: sourceMemberName, sourceLineNumber: sourceLineNumber);
        }

        public async Task GetStateAsync()
        {
            await _vpnServiceManager.RepeatState();
        }

        private async Task EnqueueAsync(Func<Task> function,
            string sourceFilePath,
            string sourceMemberName,
            int sourceLineNumber,
            [CallerMemberName] string requestSourceMemberName = "")
        {
            try
            {
                await _taskQueue.Enqueue(function);
            }
            catch (Exception exception)
            {
                _eventPublisher.CaptureError(exception,
                    sourceFilePath: sourceFilePath,
                    sourceMemberName: sourceMemberName,
                    sourceLineNumber: sourceLineNumber);
                _logger.Error<ConnectionErrorLog>($"[VpnManager] Request failed ({requestSourceMemberName}).", exception,
                    sourceFilePath: sourceFilePath,
                    sourceMemberName: sourceMemberName,
                    sourceLineNumber: sourceLineNumber);
            }
        }
    }
}