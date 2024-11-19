/*
 * Copyright (c) 2024 Proton AG
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
using System.Threading.Tasks;
using ProtonVPN.Api.Contracts;
using ProtonVPN.Api.Contracts.Common;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.StatisticalEvents;
using ProtonVPN.Common.Threading;
using ProtonVPN.Core.Settings;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.StatisticalEvents.Sending.Contracts;

namespace ProtonVPN.StatisticalEvents.Sending
{
    public class UnauthStatisticalEventSender : StatisticEventSenderBase, IUnauthStatisticalEventSender
    {
        public UnauthStatisticalEventSender(IApiClient api,
            ILogger logger,
            IScheduler scheduler,
            IAppSettings appSettings,
            IConfiguration config)
            : base(api, logger, scheduler, appSettings, config)
        {
            Timer.Start();
        }

        protected async override Task<ApiResponseResult<BaseResponse>> SendApiRequestAsync(
            StatisticalEventsBatch statisticalEventsBatch)
        {
            return await Api.PostUnauthStatisticalEventsAsync(statisticalEventsBatch);
        }

        protected override string GetStatisticalEventsFromSettings()
        {
            return AppSettings.UnauthStatisticalEvents;
        }

        protected override void SetStatisticalEventsInSettings(string? value)
        {
            AppSettings.UnauthStatisticalEvents = value;
        }

        public async Task EnqueueAsync(StatisticalEvent statisticalEvent)
        {
            await EnqueueAsync(statisticalEvent, () => new ConcurrentQueue<StatisticalEvent>(GetStoredStatisticalEvents()));
        }
    }
}