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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ProtonVPN.Api.Contracts;
using ProtonVPN.Api.Contracts.Common;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Common.Core.Extensions;
using ProtonVPN.Common.Core.StatisticalEvents;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;
using ProtonVPN.StatisticalEvents.Files;
using SingleAction = ProtonVPN.Common.Core.Threading.SingleAction;
using Timer = System.Threading.Timer;

namespace ProtonVPN.StatisticalEvents.Sending;

public abstract class StatisticEventSenderBase
{
    private const int MAX_NUM_OF_EVENTS = 100;

    private readonly IConfiguration _config;
    private readonly TimeSpan _minSendWaitTime;
    private readonly SingleAction _triggerSendAction;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    private Timer _timer;
    private TimeSpan _timerInterval;
    private DateTime _lastSendTime = DateTime.MinValue;

    protected IApiClient Api { get; }
    protected ILogger Logger { get; }
    protected ISettings Settings { get; }
    protected IStatisticalEventsFileReaderWriter StatisticalEventsFileReaderWriter { get; }
    protected ConcurrentQueue<StatisticalEvent>? EventsToSend { get; set; }

    protected StatisticEventSenderBase(
        IApiClient api,
        ILogger logger,
        ISettings settings,
        IConfiguration config,
        IStatisticalEventsFileReaderWriter statisticalEventsFileReaderWriter)
    {
        Api = api;
        Logger = logger;
        Settings = settings;
        StatisticalEventsFileReaderWriter = statisticalEventsFileReaderWriter;
        _config = config;

        _timerInterval = _config.StatisticalEventSendTriggerInterval.AddJitter(0.2);
        _minSendWaitTime = _config.StatisticalEventMinimumWaitInterval;
        _triggerSendAction = new SingleAction(TriggerSendAsync);

        _timer = new Timer(_ =>
        {
            _triggerSendAction.Run();
        }, null, TimeSpan.Zero, _timerInterval);
    }

    protected virtual async Task EnqueueAsync(StatisticalEvent statisticalEvent, Func<ConcurrentQueue<StatisticalEvent>> newQueueFunc)
    {
        await _semaphore.WaitAsync();
        try
        {
            EventsToSend ??= newQueueFunc();
            EventsToSend.Enqueue(statisticalEvent);
            Logger.Debug<AppLog>($"Statistical event queued. {JsonConvert.SerializeObject(statisticalEvent)}");
            if (EventsToSend.Count > MAX_NUM_OF_EVENTS)
            {
                int numOfEventsToDelete = EventsToSend.Count - MAX_NUM_OF_EVENTS;
                Logger.Warn<AppLog>($"Too many statistical events. Deleting the {numOfEventsToDelete} oldest" +
                    $"events because there are {EventsToSend.Count} events when the max is {MAX_NUM_OF_EVENTS}.");
                for (int i = 0; i < numOfEventsToDelete; i++)
                {
                    EventsToSend.TryDequeue(out StatisticalEvent? deletedStatisticalEvent);
                    Logger.Debug<AppLog>($"Statistical event deleted. {JsonConvert.SerializeObject(deletedStatisticalEvent)}");
                }
            }
            Logger.Info<AppLog>($"{EventsToSend.Count} statistical events are now queued.");
            SaveToFile(EventsToSend.ToList());
        }
        finally
        {
            _semaphore.Release();
        }
        _triggerSendAction.Run();
    }

    protected List<StatisticalEvent> GetStoredStatisticalEvents()
    {    
        return GetStatisticalEventsFromFile().TakeLast(MAX_NUM_OF_EVENTS).ToList();
    }

    protected abstract List<StatisticalEvent> GetStatisticalEventsFromFile();

    protected virtual async Task TriggerSendAsync()
    {
        if ((_lastSendTime + _minSendWaitTime) <= DateTime.UtcNow)
        {
            await SendAsync();
        }
    }

    private async Task SendAsync()
    {
        StatisticalEventsBatch statisticalEventsBatch = new();
        await _semaphore.WaitAsync();
        try
        {
            statisticalEventsBatch.EventInfo = EventsToSend?.ToList() ?? [];
        }
        finally
        {
            _semaphore.Release();
        }

        int numOfEvents = statisticalEventsBatch.EventInfo.Count;
        if (numOfEvents <= 0)
        {
            return;
        }

        _lastSendTime = DateTime.UtcNow;
        Logger.Info<AppLog>($"Sending {numOfEvents} statistical events.");

        try
        {
            ApiResponseResult<BaseResponse> baseResponse = await SendApiRequestAsync(statisticalEventsBatch);
            if (baseResponse.Success)
            {
                Logger.Info<AppLog>($"Successfully sent {numOfEvents} statistical events. Removing them from the queue.");
                await RemoveSuccessfullySentEventsAsync(statisticalEventsBatch.EventInfo);
            }
            else
            {
                Logger.Error<AppLog>($"Failed to send {numOfEvents} statistical events. Keeping them in the queue.");
            }
        }
        catch (Exception ex)
        {
            Logger.Error<AppLog>($"Exception thrown when sending {numOfEvents} statistical events. Keeping them in the queue.", ex);
        }
    }

    protected abstract Task<ApiResponseResult<BaseResponse>> SendApiRequestAsync(StatisticalEventsBatch statisticalEventsBatch);

    private async Task RemoveSuccessfullySentEventsAsync(List<StatisticalEvent> statisticalEventsSent)
    {
        await _semaphore.WaitAsync();
        try
        {
            int numOfEventsSent = statisticalEventsSent.Count;
            for (int i = 0; i < numOfEventsSent; i++)
            {
                if (EventsToSend is null)
                {
                    Logger.Warn<AppLog>($"Can't remove statistical events because the queue is null.");
                    break;
                }
                if (!EventsToSend.TryDequeue(out StatisticalEvent? statisticalEvent))
                {
                    Logger.Warn<AppLog>($"The statistical events queue is unexpectedly shorter. Occurred when removing {i} of {numOfEventsSent}.");
                    break;
                }
            }
            if (EventsToSend is not null)
            {
                Logger.Info<AppLog>($"Removed successfully sent statistical events. {EventsToSend.Count} events are now queued.");
            }
            SaveToFile(EventsToSend?.ToList() ?? []);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    protected abstract void SaveToFile(List<StatisticalEvent> events);

    protected async Task ClearEventsDueToDisabledTelemetryAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            if (EventsToSend is null)
            {
                Logger.Warn<AppLog>($"Can't clear statistical events because the queue is null.");
            }
            else
            {
                int numOfEventsBefore = EventsToSend.Count;
                EventsToSend.Clear();
                SaveToFile(EventsToSend.ToList());
                Logger.Info<AppLog>($"Statistical events cleared from telemetry becoming disabled. " +
                    $"{numOfEventsBefore} queued events before deletion. " +
                    $"{EventsToSend.Count} queued events after deletion.");
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    protected void StartTimer()
    {
        _timer.Change(TimeSpan.Zero, _timerInterval);
    }

    protected void StopTimer()
    {
        _timer.Change(Timeout.Infinite, Timeout.Infinite);
    }
}