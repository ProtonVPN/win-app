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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ProtonVPN.Api.Contracts;
using ProtonVPN.Api.Contracts.Common;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.StatisticalEvents;
using ProtonVPN.Common.Threading;
using ProtonVPN.Core.Auth;
using ProtonVPN.Core.Settings;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;

namespace ProtonVPN.StatisticalEvents.Sending
{
    public class StatisticalEventSender : IStatisticalEventSender, ILoggedInAware, ILogoutAware, ISettingsAware
    {
        private const int MAX_NUM_OF_EVENTS = 100;

        private readonly IApiClient _api;
        private readonly ILogger _logger;
        private readonly IAppSettings _appSettings;
        private readonly IConfiguration _config;
        private readonly ISchedulerTimer _timer;

        private readonly TimeSpan _minSendWaitTime;
        private readonly SingleAction _triggerSendAction;

        private ConcurrentQueue<StatisticalEvent>? _eventsToSend;
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        private DateTime _lastSendTime = DateTime.MinValue;
        private bool _isLoggedIn;

        public StatisticalEventSender(IApiClient api,
            ILogger logger,
            IScheduler scheduler,
            IAppSettings appSettings,
            IConfiguration config)
        {
            _api = api;
            _logger = logger;
            _appSettings = appSettings;
            _config = config;

            _minSendWaitTime = _config.StatisticalEventMinimumWaitInterval;
            _triggerSendAction = new SingleAction(TriggerSendAsync);

            _timer = scheduler.Timer();
            _timer.Interval = _config.StatisticalEventSendTriggerInterval.RandomizedWithDeviation(0.2);
            _timer.Tick += Timer_OnTick;
        }

        private List<StatisticalEvent> GetStoredStatisticalEvents()
        {
            List<StatisticalEvent>? statisticalEvents = null;
            string statisticalEventsJson = _appSettings.StatisticalEvents;
            if (!string.IsNullOrWhiteSpace(statisticalEventsJson))
            {
                try
                {
                    statisticalEvents = JsonConvert.DeserializeObject<List<StatisticalEvent>>(statisticalEventsJson);
                }
                catch (Exception ex)
                {
                    _logger.Error<AppLog>("Error deserializing the stored statistical events.", ex);
                }
            }
            return statisticalEvents?.TakeLast(MAX_NUM_OF_EVENTS).ToList() ?? new List<StatisticalEvent>();
        }

        public async Task EnqueueAsync(StatisticalEvent statisticalEvent)
        {
            if (_isLoggedIn && _appSettings.IsTelemetryGloballyEnabled)
            {
                await _semaphore.WaitAsync();
                try
                {
                    _eventsToSend ??= new ConcurrentQueue<StatisticalEvent>();
                    _eventsToSend.Enqueue(statisticalEvent);
                    _logger.Debug<AppLog>($"Statistical event queued. {JsonConvert.SerializeObject(statisticalEvent)}");
                    if (_eventsToSend.Count > MAX_NUM_OF_EVENTS)
                    {
                        int numOfEventsToDelete = _eventsToSend.Count - MAX_NUM_OF_EVENTS;
                        _logger.Warn<AppLog>($"Too many statistical events. Deleting the {numOfEventsToDelete} oldest" +
                            $"events because there are {_eventsToSend.Count} events when the max is {MAX_NUM_OF_EVENTS}.");
                        for (int i = 0; i < numOfEventsToDelete; i++)
                        {
                            _eventsToSend.TryDequeue(out StatisticalEvent? deletedStatisticalEvent);
                            _logger.Debug<AppLog>($"Statistical event deleted. {JsonConvert.SerializeObject(deletedStatisticalEvent)}");
                        }
                    }
                    _logger.Info<AppLog>($"{_eventsToSend.Count} statistical events are now queued.");
                    SaveToFile();
                }
                finally
                {
                    _semaphore.Release();
                }
                _triggerSendAction.Run();
            }
        }

        private void Timer_OnTick(object? sender, EventArgs e)
        {
            _triggerSendAction.Run();
        }

        private async Task TriggerSendAsync()
        {
            if (_isLoggedIn && _appSettings.IsTelemetryGloballyEnabled)
            {
                if ((_lastSendTime + _minSendWaitTime) <= DateTime.UtcNow)
                {
                    await SendAsync();
                }
            }
        }

        private async Task SendAsync()
        {
            StatisticalEventsBatch statisticalEventsBatch = new();
            await _semaphore.WaitAsync();
            try
            {
                statisticalEventsBatch.EventInfo = _eventsToSend?.ToList() ?? new();
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
            _logger.Info<AppLog>($"Sending {numOfEvents} statistical events.");

            try
            {
                ApiResponseResult<BaseResponse> baseResponse = await _api.PostStatisticalEventsAsync(statisticalEventsBatch);
                if (baseResponse.Success)
                {
                    _logger.Info<AppLog>($"Successfully sent {numOfEvents} statistical events. Removing them from the queue.");
                    await RemoveSuccessfullySentEventsAsync(statisticalEventsBatch.EventInfo);
                }
                else
                {
                    _logger.Error<AppLog>($"Failed to send {numOfEvents} statistical events. Keeping them in the queue.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error<AppLog>($"Exception thrown when sending {numOfEvents} statistical events. Keeping them in the queue.", ex);
            }
        }

        private async Task RemoveSuccessfullySentEventsAsync(List<StatisticalEvent> statisticalEventsSent)
        {
            await _semaphore.WaitAsync();
            try
            {
                int numOfEventsSent = statisticalEventsSent.Count;
                for (int i = 0; i < numOfEventsSent; i++)
                {
                    if (_eventsToSend is null)
                    {
                        _logger.Warn<AppLog>($"Can't remove statistical events because the queue is null.");
                        break;
                    }
                    if (!_eventsToSend.TryDequeue(out StatisticalEvent? statisticalEvent))
                    {
                        _logger.Warn<AppLog>($"The statistical events queue is unexpectedly shorter. Occurred when removing {i} of {numOfEventsSent}.");
                        break;
                    }
                }
                if (_eventsToSend is not null)
                {
                    _logger.Info<AppLog>($"Removed successfully sent statistical events. {_eventsToSend.Count} events are now queued.");
                }
                SaveToFile();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private void SaveToFile()
        {
            _appSettings.StatisticalEvents = _eventsToSend is null ? null : JsonConvert.SerializeObject(_eventsToSend.ToList());
        }

        public void OnUserLoggedIn()
        {
            _isLoggedIn = true;
            if (!_timer.IsEnabled)
            {
                _timer.Start();
            }
            _eventsToSend = new ConcurrentQueue<StatisticalEvent>(GetStoredStatisticalEvents());
        }

        public void OnUserLoggedOut()
        {
            _isLoggedIn = false;
            if (_timer.IsEnabled)
            {
                _timer.Stop();
            }
            _eventsToSend = null;
        }

        public async void OnAppSettingsChanged(PropertyChangedEventArgs e)
        {
            if (e.PropertyName is not null &&
                e.PropertyName.Equals(nameof(IAppSettings.IsTelemetryGloballyEnabled)) &&
                !_appSettings.IsTelemetryGloballyEnabled)
            {
                await _semaphore.WaitAsync();
                try
                {
                    if (_eventsToSend is null)
                    {
                        _logger.Warn<AppLog>($"Can't clear statistical events because the queue is null.");
                    }
                    else
                    {
                        int numOfEventsBefore = _eventsToSend.Count;
                        _eventsToSend.Clear();
                        SaveToFile();
                        _logger.Info<AppLog>($"Statistical events cleared from telemetry becoming disabled. " +
                            $"{numOfEventsBefore} queued events before deletion. " +
                            $"{_eventsToSend.Count} queued events after deletion.");
                    }
                }
                finally
                {
                    _semaphore.Release();
                }
            }
        }
    }
}