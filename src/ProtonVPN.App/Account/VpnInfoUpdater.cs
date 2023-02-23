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
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using ProtonVPN.Api.Contracts;
using ProtonVPN.Api.Contracts.Auth;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.AppLogs;
using ProtonVPN.Common.Threading;
using ProtonVPN.Core.Auth;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Windows;

namespace ProtonVPN.Account
{
    public class VpnInfoUpdater :
        IHandle<WindowStateMessage>,
        IHandle<UpdateVpnInfoMessage>,
        IVpnInfoUpdater,
        ILoggedInAware,
        ILogoutAware
    {
        private readonly IApiClient _api;
        private readonly ILogger _logger;
        private readonly IUserStorage _userStorage;

        private readonly TimeSpan _checkInterval;
        private readonly ISchedulerTimer _timer;
        private DateTime _lastCheck = DateTime.Now;
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public VpnInfoUpdater(IConfiguration appConfig,
            ILogger logger,
            IApiClient api,
            IUserStorage userStorage,
            IEventAggregator eventAggregator,
            IScheduler scheduler)
        {
            eventAggregator.Subscribe(this);

            _api = api;
            _logger = logger;
            _userStorage = userStorage;

            _checkInterval = appConfig.VpnInfoCheckInterval.RandomizedWithDeviation(0.2);
            _timer = scheduler.Timer();
            _timer.Interval = appConfig.ServerUpdateInterval.RandomizedWithDeviation(0.2);
            _timer.Tick += OnTimerTick;
        }

        private async void OnTimerTick(object sender, EventArgs e)
        {
            await Handle();
        }

        public async void Handle(UpdateVpnInfoMessage message)
        {
            await Update();
        }

        public async void Handle(WindowStateMessage message)
        {
            if (!message.Active)
            {
                return;
            }

            await Handle();
        }

        private async Task Handle()
        {
            if (DateTime.Now.Subtract(_lastCheck) < _checkInterval)
            {
                return;
            }

            _lastCheck = DateTime.Now;
            await Update();
        }

        public async Task Update()
        {
            await _semaphore.WaitAsync();

            try
            {
                ApiResponseResult<VpnInfoWrapperResponse> response = await _api.GetVpnInfoResponse();
                if (response.Success)
                {
                    _userStorage.StoreVpnInfo(response.Value);
                }
            }
            catch (Exception e)
            {
                if (e is not HttpRequestException)
                {
                    _logger.Error<AppLog>("An unexpected exception was thrown when updating the VPN info.", e);
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public void OnUserLoggedIn()
        {
            _timer.Start();
        }

        public void OnUserLoggedOut()
        {
            _timer.Stop();
        }
    }
}