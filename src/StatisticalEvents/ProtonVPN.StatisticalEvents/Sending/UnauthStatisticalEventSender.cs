﻿/*
 * Copyright (c) 2025 Proton AG
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
using System.Collections.Generic;
using System.Threading.Tasks;
using ProtonVPN.Api.Contracts;
using ProtonVPN.Api.Contracts.Common;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Common.Core.StatisticalEvents;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.StatisticalEvents.Contracts.Models;
using ProtonVPN.StatisticalEvents.Files;
using ProtonVPN.StatisticalEvents.Sending.Contracts;

namespace ProtonVPN.StatisticalEvents.Sending;

public class UnauthStatisticalEventSender : StatisticEventSenderBase, IUnauthStatisticalEventSender
{
    public UnauthStatisticalEventSender(
        IApiClient api,
        ILogger logger,
        ISettings settings,
        IConfiguration config,
        IStatisticalEventsFileReaderWriter statisticalEventsFileReaderWriter)
        : base(api, logger, settings, config, statisticalEventsFileReaderWriter)
    {
    }

    protected async override Task<ApiResponseResult<BaseResponse>> SendApiRequestAsync(
        StatisticalEventsBatch statisticalEventsBatch)
    {
        return await Api.PostUnauthStatisticalEventsAsync(statisticalEventsBatch);
    }

    protected override List<StatisticalEvent> GetStatisticalEventsFromFile()
    {
        return StatisticalEventsFileReaderWriter.ReadUnauthenticatedEvents().StatisticalEvents ?? [];
    }

    protected override void SaveToFile(List<StatisticalEvent> events)
    {
        StatisticalEventsFileReaderWriter.SaveUnauthenticatedEvents(new StatisticalEventsFile()
        {
            StatisticalEvents = events
        });
    }

    public async Task EnqueueAsync(StatisticalEvent statisticalEvent)
    {
        await EnqueueAsync(statisticalEvent, () => new ConcurrentQueue<StatisticalEvent>(GetStoredStatisticalEvents()));
    }
}