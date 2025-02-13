/*
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
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Messages;
using ProtonVPN.Common.Core.StatisticalEvents;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.StatisticalEvents.Contracts.Models;
using ProtonVPN.StatisticalEvents.Files;
using ProtonVPN.StatisticalEvents.Sending.Contracts;

namespace ProtonVPN.StatisticalEvents.Sending;

public class AuthenticatedStatisticalEventSender : StatisticEventSenderBase, IAuthenticatedStatisticalEventSender,
    IEventMessageReceiver<SettingChangedMessage>,
    IEventMessageReceiver<LoggedInMessage>,
    IEventMessageReceiver<LoggedOutMessage>
{
    private bool _isLoggedIn;

    protected bool CanSendAuthenticatedTelemetryEvents => _isLoggedIn && Settings.IsShareStatisticsEnabled;

    public AuthenticatedStatisticalEventSender(
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
        return await Api.PostStatisticalEventsAsync(statisticalEventsBatch);
    }

    protected override List<StatisticalEvent> GetStatisticalEventsFromFile()
    {
        return StatisticalEventsFileReaderWriter.ReadAuthenticatedEvents().StatisticalEvents ?? [];
    }

    protected override void SaveToFile(List<StatisticalEvent> events)
    {
        StatisticalEventsFileReaderWriter.SaveAuthenticatedEvents(new StatisticalEventsFile()
        {
            StatisticalEvents = events
        });
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

    public void Receive(LoggedInMessage message)
    {
        _isLoggedIn = true;
        StartTimer();
        EventsToSend = new ConcurrentQueue<StatisticalEvent>(GetStoredStatisticalEvents());
    }

    public void Receive(LoggedOutMessage message)
    {
        _isLoggedIn = false;
        StopTimer();
        EventsToSend = null;
    }

    public async void Receive(SettingChangedMessage message)
    {
        if (message.PropertyName is not null &&
            message.PropertyName.Equals(nameof(ISettings.IsShareStatisticsEnabled)) &&
            !Settings.IsShareStatisticsEnabled)
        {
            await ClearEventsDueToDisabledTelemetryAsync();
        }
    }
}