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
using ProtonVPN.Api.Contracts.Settings;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Threading;
using ProtonVPN.Core.Auth;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Windows;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;

namespace ProtonVPN.Account
{
    public class UserSettingsUpdater :
        IHandle<WindowStateMessage>,
        IUserSettingsUpdater,
        ILoggedInAware,
        ILogoutAware
    {
        private readonly IApiClient _api;
        private readonly ILogger _logger;
        private readonly IAppSettings _appSettings;

        private readonly TimeSpan _checkInterval;
        private readonly ISchedulerTimer _timer;
        private DateTime _lastCheck = DateTime.MinValue;
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public UserSettingsUpdater(IConfiguration appConfig,
            ILogger logger,
            IApiClient api,
            IAppSettings appSettings,
            IEventAggregator eventAggregator,
            IScheduler scheduler)
        {
            eventAggregator.Subscribe(this);

            _api = api;
            _logger = logger;
            _appSettings = appSettings;
            _checkInterval = appConfig.VpnInfoCheckInterval.RandomizedWithDeviation(0.2);
            _timer = scheduler.Timer();
            _timer.Interval = appConfig.ServerUpdateInterval.RandomizedWithDeviation(0.2);
            _timer.Tick += OnTimerTick;
        }

        private async void OnTimerTick(object sender, EventArgs e)
        {
            await Handle();
        }

        public async Task HandleAsync(WindowStateMessage message, CancellationToken cancellationToken)
        {
            if (!message.IsActive)
            {
                return;
            }

            await Handle();
        }

        private async Task Handle()
        {
            if (DateTime.UtcNow.Subtract(_lastCheck) < _checkInterval)
            {
                return;
            }

            _lastCheck = DateTime.UtcNow;
            await Update();
        }

        public async Task Update()
        {
            await _semaphore.WaitAsync();

            try
            {
                ApiResponseResult<SettingsResponse> response = await _api.GetSettingsAsync();
                if (response.Success)
                {
                    _appSettings.IsTelemetryGloballyEnabled = response.Value.UserSettings.Telemetry;
                }
            }
            catch (Exception e)
            {
                if (e is not HttpRequestException)
                {
                    _logger.Error<AppLog>("An unexpected exception was thrown when updating the user settings.", e);
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