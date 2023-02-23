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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using ProtonVPN.Api.Contracts.Servers;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Networking;
using ProtonVPN.Common.Threading;
using ProtonVPN.Core.Auth;
using ProtonVPN.Core.Servers.FileStoraging;
using ProtonVPN.Core.Settings;

namespace ProtonVPN.Core.Servers
{
    public class ServerUpdater : IServerUpdater, ILoggedInAware, ILogoutAware, ISettingsAware
    {
        private readonly ISchedulerTimer _timer;
        private readonly ServerManager _serverManager;
        private readonly IApiServers _apiServers;
        private readonly IServersFileStorage _serversFileStorage;
        private readonly SingleAction _updateAction;
        private readonly IAppSettings _appSettings;

        private VpnProtocol _lastVpnProtocol;
        private bool _firstTime = true;

        public ServerUpdater(
            IScheduler scheduler,
            IConfiguration appConfig,
            ServerManager serverManager,
            IApiServers apiServers,
            IServersFileStorage serversFileStorage,
            ServerLoadUpdater serverLoadUpdater,
            IAppSettings appSettings)
        {
            _serverManager = serverManager;
            _apiServers = apiServers;
            _serversFileStorage = serversFileStorage;
            _appSettings = appSettings;

            _timer = scheduler.Timer();
            _timer.Interval = appConfig.ServerUpdateInterval.RandomizedWithDeviation(0.2);
            _timer.Tick += Timer_OnTick;

            _updateAction = new SingleAction(UpdateServers);
            serverLoadUpdater.ServerLoadsUpdated += OnServerLoadsUpdated;
        }

        public event EventHandler ServersUpdated;

        public void OnUserLoggedIn()
        {
            _timer.Start();
            _lastVpnProtocol = _appSettings.GetProtocol();
        }

        public void OnUserLoggedOut()
        {
            _timer.Stop();
            _firstTime = true;
        }

        public async Task Update()
        {
            await _updateAction.Run();
        }

        private void OnServerLoadsUpdated(object sender, EventArgs e)
        {
            InvokeServersUpdated();
        }

        private async Task UpdateServers()
        {
            IReadOnlyCollection<LogicalServerResponse> servers = await GetServers();

            if (servers.Any())
            {
                _serverManager.Load(servers);
                InvokeServersUpdated();
                _serversFileStorage.Set(servers);
            }
        }

        private async Task<IReadOnlyCollection<LogicalServerResponse>> GetServers()
        {
            if (!_firstTime)
            {
                return await _apiServers.GetServersAsync();
            }

            _firstTime = false;

            if (_serverManager.Empty())
            {
                return await _apiServers.GetServersAsync();
            }

            // First time after start or logoff server update is scheduled without waiting for the result
            ScheduleUpdate();

            return new List<LogicalServerResponse>(0);
        }

        private void ScheduleUpdate()
        {
            // Schedule servers update from API without waiting for the result
            _updateAction.Task.ContinueWith(t => Update());
        }

        private void Timer_OnTick(object sender, EventArgs eventArgs)
        {
            _updateAction.Run();
        }

        private void InvokeServersUpdated()
        {
            ServersUpdated?.Invoke(this, EventArgs.Empty);
        }

        public void OnAppSettingsChanged(PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IAppSettings.OvpnProtocol) &&
                _lastVpnProtocol == VpnProtocol.WireGuard && _appSettings.GetProtocol() != VpnProtocol.WireGuard ||
                _lastVpnProtocol != VpnProtocol.WireGuard && _appSettings.GetProtocol() == VpnProtocol.WireGuard)
            {
                _updateAction.Run();
                _lastVpnProtocol = _appSettings.GetProtocol();
            }
        }
    }
}