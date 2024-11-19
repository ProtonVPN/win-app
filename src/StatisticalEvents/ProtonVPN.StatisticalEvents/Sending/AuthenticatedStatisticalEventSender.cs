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

using System.Collections.Concurrent;
using System.ComponentModel;
using System.Threading.Tasks;
using ProtonVPN.Api.Contracts;
using ProtonVPN.Api.Contracts.Common;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.StatisticalEvents;
using ProtonVPN.Common.Threading;
using ProtonVPN.Core.Auth;
using ProtonVPN.Core.Settings;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.StatisticalEvents.Sending.Contracts;

namespace ProtonVPN.StatisticalEvents.Sending
{
    public class AuthenticatedStatisticalEventSender : StatisticEventSenderBase,
        IAuthenticatedStatisticalEventSender, ILoggedInAware, ILogoutAware, ISettingsAware
    {
        private bool _isLoggedIn;

        protected bool CanSendAuthenticatedTelemetryEvents => _isLoggedIn && AppSettings.IsTelemetryGloballyEnabled;

        public AuthenticatedStatisticalEventSender(IApiClient api,
            ILogger logger,
            IScheduler scheduler,
            IAppSettings appSettings,
            IConfiguration config)
            : base(api, logger, scheduler, appSettings, config)
        {
        }

        protected async override Task<ApiResponseResult<BaseResponse>> SendApiRequestAsync(
            StatisticalEventsBatch statisticalEventsBatch)
        {
            return await Api.PostStatisticalEventsAsync(statisticalEventsBatch);
        }

        protected override string GetStatisticalEventsFromSettings()
        {
            return AppSettings.StatisticalEvents;
        }

        protected override void SetStatisticalEventsInSettings(string? value)
        {
            AppSettings.StatisticalEvents = value;
        }

        public async Task EnqueueAsync(StatisticalEvent statisticalEvent)
        {
            if (CanSendAuthenticatedTelemetryEvents)
            {
                await EnqueueAsync(statisticalEvent, () => new ConcurrentQueue<StatisticalEvent>());
            }
        }

        protected override async Task TriggerSendAsync()
        {
            if (CanSendAuthenticatedTelemetryEvents)
            {
                await base.TriggerSendAsync();
            }
        }

        public void OnUserLoggedIn()
        {
            _isLoggedIn = true;
            if (!Timer.IsEnabled)
            {
                Timer.Start();
            }
            EventsToSend = new ConcurrentQueue<StatisticalEvent>(GetStoredStatisticalEvents());
        }

        public void OnUserLoggedOut()
        {
            _isLoggedIn = false;
            if (Timer.IsEnabled)
            {
                Timer.Stop();
            }
            EventsToSend = null;
        }

        public async void OnAppSettingsChanged(PropertyChangedEventArgs e)
        {
            if (e.PropertyName is not null &&
                e.PropertyName.Equals(nameof(IAppSettings.IsTelemetryGloballyEnabled)) &&
                !AppSettings.IsTelemetryGloballyEnabled)
            {
                await ClearEventsDueToDisabledTelemetryAsync();
            }
        }
    }
}