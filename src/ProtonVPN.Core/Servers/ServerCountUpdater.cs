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
using System.Threading.Tasks;
using ProtonVPN.Api.Contracts.Servers;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Threading;
using ProtonVPN.Core.Auth;
using ProtonVPN.Core.Settings;

namespace ProtonVPN.Core.Servers
{
    public class ServerCountUpdater : IServerCountUpdater, ILoggedInAware, ILogoutAware
    {
        private readonly ISchedulerTimer _timer;
        private readonly IApiServers _apiServers;
        private readonly IAppSettings _appSettings;
        private readonly SingleAction _updateAction;

        public event EventHandler ServerCountUpdated;

        public ServerCountUpdater(
            IScheduler scheduler,
            IConfiguration appConfig,
            IApiServers apiServers,
            ServerLoadUpdater serverLoadUpdater,
            IAppSettings appSettings)
        {
            _apiServers = apiServers;
            _appSettings = appSettings;

            _timer = scheduler.Timer();
            _timer.Interval = appConfig.ServerUpdateInterval.RandomizedWithDeviation(0.2);
            _timer.Tick += Timer_OnTick;

            _updateAction = new SingleAction(UpdateServerCountAsync);
        }

        public void OnUserLoggedIn()
        {
            _timer.Start();
        }

        public void OnUserLoggedOut()
        {
            _timer.Stop();
        }

        public async Task UpdateAsync()
        {
            await _updateAction.Run();
        }

        private async Task UpdateServerCountAsync()
        {
            ServerCountResponse serverCount = await _apiServers.GetServerCountAsync();

            if (serverCount is not null &&
                serverCount.Servers > 0 &&
                serverCount.Countries > 0)
            {
                _appSettings.ServerCount = serverCount.Servers;
                _appSettings.CountryCount = serverCount.Countries;
                InvokeServerCountUpdated();
            }
        }

        private void Timer_OnTick(object sender, EventArgs eventArgs)
        {
            _updateAction.Run();
        }

        private void InvokeServerCountUpdated()
        {
            ServerCountUpdated?.Invoke(this, EventArgs.Empty);
        }
    }
}