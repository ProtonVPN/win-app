/*
 * Copyright (c) 2022 Proton Technologies AG
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
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Threading;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Windows;

namespace ProtonVPN.Account
{
    public class VpnInfoUpdater : IHandle<WindowStateMessage>, IVpnInfoUpdater
    {
        private readonly TimeSpan _checkInterval;
        private readonly IApiClient _api;
        private readonly IUserStorage _userStorage;
        private static readonly SemaphoreSlim Semaphore = new(1, 1);
        private DateTime _lastCheck = DateTime.Now;
        private readonly ISchedulerTimer _timer;

        public VpnInfoUpdater(Common.Configuration.Config appConfig,
            IEventAggregator eventAggregator,
            IApiClient api,
            IUserStorage userStorage,
            IScheduler scheduler)
        {
            eventAggregator.Subscribe(this);

            _checkInterval = appConfig.VpnInfoCheckInterval.RandomizedWithDeviation(0.2);
            _api = api;
            _userStorage = userStorage;

            _timer = scheduler.Timer();
            _timer.Interval = appConfig.ServerUpdateInterval.RandomizedWithDeviation(0.2);
            _timer.Tick += OnTimerTick;
            _timer.Start();
        }

        private async void OnTimerTick(object sender, EventArgs e)
        {
            await Handle();
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
            await Semaphore.WaitAsync();

            try
            {
                ApiResponseResult<VpnInfoWrapperResponse> response = await _api.GetVpnInfoResponse();
                if (response.Success)
                {
                    _userStorage.StoreVpnInfo(response.Value);
                }
            }
            catch (HttpRequestException)
            {
            }
            finally
            {
                Semaphore.Release();
            }
        }
    }
}