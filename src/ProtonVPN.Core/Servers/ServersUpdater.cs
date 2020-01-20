/*
 * Copyright (c) 2020 Proton Technologies AG
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
using System.Linq;
using System.Threading.Tasks;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Threading;
using ProtonVPN.Core.Auth;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.User;

namespace ProtonVPN.Core.Servers
{
    public class ServersUpdater : ILoggedInAware, ILogoutAware, IUserLocationAware
    {
        private readonly ILogger _logger;
        private readonly ISchedulerTimer _timer;
        private readonly ServerManager _serverManager;
        private readonly CachedServersProvider _serversProvider;

        private readonly SingleAction _updateAction;

        public ServersUpdater(
            ILogger logger,
            IScheduler scheduler,
            Config appConfig,
            ServerManager serverManager,
            CachedServersProvider serversProvider)
        {
            _logger = logger;
            _serverManager = serverManager;
            _serversProvider = serversProvider;

            _timer = scheduler.Timer();
            _timer.Interval = appConfig.ServerUpdateInterval.RandomizedWithDeviation(0.2);
            _timer.Tick += Timer_OnTick;

            _updateAction = new SingleAction(UpdateServers);
        }

        public event EventHandler ServersUpdated;

        public void OnUserLoggedIn()
        {
            _timer.Start();
        }

        public void OnUserLoggedOut()
        {
            _timer.Stop();
        }

        public async Task Update()
        {
            await _updateAction.Run();
        }

        public Task OnUserLocationChanged(UserLocation location)
        {
            _ = Update();

            return Task.CompletedTask;
        }

        private async Task UpdateServers()
        {
            var servers = await _serversProvider.GetServersAsync();
            if (!servers.Any())
            {
                _logger.Error("Failed to update server list");
                return;
            }

            _serverManager.Load(servers);
            InvokeServersUpdated();
        }

        private void Timer_OnTick(object sender, EventArgs eventArgs)
        {
            _updateAction.Run();
        }

        private void InvokeServersUpdated()
        {
            ServersUpdated?.Invoke(this, EventArgs.Empty);
        }
    }
}
