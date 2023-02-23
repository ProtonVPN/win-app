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
using System.Net.Http;
using System.Threading.Tasks;
using ProtonVPN.Api.Contracts;
using ProtonVPN.Api.Contracts.Servers;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.AppLogs;
using ProtonVPN.Common.Logging.Categorization.Events.ConnectLogs;
using ProtonVPN.Common.Threading;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Servers.Models;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Vpn;

namespace ProtonVPN.Core.Service.Vpn
{
    public class ReconnectManager : IVpnStateAware
    {
        private VpnState _state;
        private readonly IApiClient _apiClient;
        private readonly IVpnManager _vpnManager;
        private readonly ISchedulerTimer _timer;
        private readonly ServerManager _serverManager;
        private readonly IServerUpdater _serverUpdater;
        private readonly ILogger _logger;
        private readonly IAppSettings _appSettings;

        public ReconnectManager(
            IAppSettings appSettings,
            IApiClient apiClient,
            ServerManager serverManager,
            IVpnManager vpnManager,
            IScheduler scheduler,
            IServerUpdater serverUpdater,
            ILogger logger)
        {
            _appSettings = appSettings;
            _serverUpdater = serverUpdater;
            _logger = logger;
            _serverManager = serverManager;
            _vpnManager = vpnManager;
            _apiClient = apiClient;

            _timer = scheduler.Timer();
            _timer.Tick += OnTimerTick;
        }

        public Task OnVpnStateChanged(VpnStateChangedEventArgs e)
        {
            if (!_appSettings.FeatureMaintenanceTrackerEnabled)
            {
                return Task.CompletedTask;
            }

            _state = e.State;

            if (e.State.Status == VpnStatus.Connected)
            {
                if (!_timer.IsEnabled)
                {
                    _timer.Interval = _appSettings.MaintenanceCheckInterval;
                    _timer.Start();
                }
            }
            else
            {
                if (_timer.IsEnabled)
                {
                    _timer.Stop();
                }
            }

            return Task.CompletedTask;
        }

        private async void OnTimerTick(object sender, EventArgs e)
        {
            await CheckIfCurrentServerIsOnlineAsync();
        }

        public async Task CheckIfCurrentServerIsOnlineAsync()
        {
            if (!_timer.IsEnabled || !await ServerOffline())
            {
                return;
            }

            _serverManager.MarkServerUnderMaintenance(_state.Server.ExitIp);
            await _serverUpdater.Update();
            _logger.Info<ConnectTriggerLog>($"Reconnecting due to server {_state.Server.Name} ({_state.Server.ExitIp}) being no longer available.");
            await _vpnManager.ReconnectAsync(new VpnReconnectionSettings
            {
                IsToReconnectIfDisconnected = true,
                IsToExcludeLastServer = true,
                IsToShowReconnectionPopup = true
            });
        }

        private async Task<bool> ServerOffline()
        {
            PhysicalServer server = _serverManager.GetPhysicalServerByServer(_state.Server);
            if (server == null)
            {
                _logger.Info<AppLog>($"The server {_state.Server.Name} ({_state.Server.ExitIp}) was removed from the API.");
                return true;
            }

            try
            {
                ApiResponseResult<PhysicalServerWrapperResponse> result = await _apiClient.GetServerAsync(server.Id);
                if (!result.Success)
                {
                    return false;
                }

                bool isServerUnderMaintenance = result.Value.Server.Status == 0;
                if (isServerUnderMaintenance)
                {
                    _logger.Info<AppLog>($"The server {_state.Server.Name} ({_state.Server.ExitIp}) is under maintenance.");
                }

                return isServerUnderMaintenance;
            }
            catch
            {
                return false;
            }
        }
    }
}