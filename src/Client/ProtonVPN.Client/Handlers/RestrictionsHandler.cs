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

using ProtonVPN.Client.Common.Dispatching;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Handlers.Bases;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Notifications;
using ProtonVPN.Client.Settings.Contracts;

namespace ProtonVPN.Client.Handlers;

public class RestrictionsHandler : IHandler,
    IEventMessageReceiver<P2PTrafficDetectedMessage>,
    IEventMessageReceiver<StreamingTrafficDetectedMessage>
{
    public const int NOTIFICATION_COOLDOWN_IN_HOURS = 24;

    private readonly ISettings _settings;
    private readonly IP2PDetectionWindowActivator _p2pWarningWindowActivator;
    private readonly IStreamingDetectionWindowActivator _streamingDetectionWindowActivator;
    private readonly IP2PWarningNotificationSender _p2pWarningNotificationSender;
    private readonly IStreamingWarningNotificationSender _streamingWarningNotificationSender;
    private readonly IUIThreadDispatcher _uiThreadDispatcher;

    public RestrictionsHandler(
        ISettings settings,
        IP2PDetectionWindowActivator p2pWarningWindowActivator,
        IStreamingDetectionWindowActivator streamingDetectionWindowActivator,
        IP2PWarningNotificationSender p2pWarningNotificationSender,
        IStreamingWarningNotificationSender streamingWarningNotificationSender,
        IUIThreadDispatcher uiThreadDispatcher)
    {
        _settings = settings;
        _p2pWarningWindowActivator = p2pWarningWindowActivator;
        _streamingDetectionWindowActivator = streamingDetectionWindowActivator;
        _p2pWarningNotificationSender = p2pWarningNotificationSender;
        _streamingWarningNotificationSender = streamingWarningNotificationSender;
        _uiThreadDispatcher = uiThreadDispatcher;
    }

    public void Receive(P2PTrafficDetectedMessage message)
    {
        HandleTrafficDetection(
            _settings.LastP2PWarningNotificationUtcDate,
            date => _settings.LastP2PWarningNotificationUtcDate = date,
            () =>
            {
                _p2pWarningWindowActivator.Activate();
                _p2pWarningNotificationSender.Send();
            });
    }

    public void Receive(StreamingTrafficDetectedMessage message)
    {
        HandleTrafficDetection(
            _settings.LastStreamingWarningNotificationUtcDate,
            date => _settings.LastStreamingWarningNotificationUtcDate = date,
            () =>
            {
                _streamingDetectionWindowActivator.Activate();
                _streamingWarningNotificationSender.Send();
            });
    }

    private void HandleTrafficDetection(
        DateTimeOffset lastNotificationDate,
        Action<DateTimeOffset> updateLastNotificationDate,
        Action handleNotification)
    {
        DateTimeOffset utcNow = DateTimeOffset.UtcNow;
        if ((utcNow - lastNotificationDate).TotalHours < NOTIFICATION_COOLDOWN_IN_HOURS)
        {
            return;
        }

        updateLastNotificationDate(utcNow);

        _uiThreadDispatcher.TryEnqueue(handleNotification);
    }
}