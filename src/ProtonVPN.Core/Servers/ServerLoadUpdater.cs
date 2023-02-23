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
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using ProtonVPN.Api.Contracts.Servers;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Threading;
using ProtonVPN.Core.Auth;
using ProtonVPN.Core.Users;
using ProtonVPN.Core.Windows;

namespace ProtonVPN.Core.Servers
{
    public class ServerLoadUpdater :
        ILoggedInAware,
        ILogoutAware,
        IUserLocationAware,
        IHandle<WindowStateMessage>
    {
        private readonly TimeSpan _updateInterval;
        private readonly ServerManager _serverManager;
        private readonly ISchedulerTimer _timer;
        private readonly IMainWindowState _mainWindowState;
        private readonly ISingleAction _updateAction;
        private readonly IApiServers _apiServers;
        private readonly ILastServerLoadTimeProvider _lastServerLoadTimeProvider;

        public ServerLoadUpdater(
            TimeSpan updateInterval,
            ServerManager serverManager,
            IScheduler scheduler,
            IEventAggregator eventAggregator,
            IMainWindowState mainWindowState,
            IApiServers apiServers,
            ISingleActionFactory singleActionFactory,
            ILastServerLoadTimeProvider lastServerLoadTimeProvider)
        {
            _lastServerLoadTimeProvider = lastServerLoadTimeProvider;
            eventAggregator.Subscribe(this);

            _updateInterval = updateInterval;
            _mainWindowState = mainWindowState;
            _serverManager = serverManager;
            _apiServers = apiServers;
            _timer = scheduler.Timer();
            _timer.Interval = updateInterval.RandomizedWithDeviation(0.2);
            _timer.Tick += TimerOnTick;
            _updateAction = singleActionFactory.GetSingleAction(UpdateLoads);
        }

        public event EventHandler ServerLoadsUpdated;

        public void OnUserLoggedIn()
        {
            _timer.Start();
        }

        public void OnUserLoggedOut()
        {
            _timer.Stop();
        }

        public void Handle(WindowStateMessage message)
        {
            if (!message.Active || !TimeToUpdateLoads())
            {
                return;
            }

            StartUpdateAction();
        }

        public Task OnUserLocationChanged(UserLocationEventArgs e)
        {
            if (e.State == UserLocationState.Success)
            {
                _updateAction.Run();
            }

            return Task.CompletedTask;
        }

        private void TimerOnTick(object sender, EventArgs eventArgs)
        {
            if (!_mainWindowState.Active)
            {
                return;
            }

            StartUpdateAction();
        }

        private bool TimeToUpdateLoads()
        {
            return DateTime.Now.Subtract(_lastServerLoadTimeProvider.LastChecked()) > _updateInterval;
        }

        private void StartUpdateAction()
        {
            _updateAction.Run();
            _lastServerLoadTimeProvider.Update();
        }

        private async Task UpdateLoads()
        {
            IReadOnlyCollection<LogicalServerResponse> servers = await _apiServers.GetLoadsAsync();

            if (servers.Any())
            {
                _serverManager.UpdateLoads(servers);
                ServerLoadsUpdated?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}