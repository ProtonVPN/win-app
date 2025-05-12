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

using Microsoft.Toolkit.Uwp.Notifications;
using ProtonVPN.Client.Contracts.Messages;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Notifications.Contracts;
using ProtonVPN.Client.Notifications.Contracts.Arguments;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.Notifications;

public class SubscriptionExpiredNotificationSender : NotificationSenderBase, ISubscriptionExpiredNotificationSender,
    IEventMessageReceiver<MainWindowVisibilityChangedMessage>
{
    private readonly ILocalizationProvider _localizer;
    private readonly IConnectionManager _connectionManager;

    private bool _isMainWindowVisible;

    public SubscriptionExpiredNotificationSender(
        ILogger logger,
        ILocalizationProvider localizer, 
        IConnectionManager connectionManager)
        : base(logger)
    {
        _localizer = localizer;
        _connectionManager = connectionManager;
    }

    public void Send()
    {
        if (_isMainWindowVisible)
        {
            return;
        }

        ToastContentBuilder tcb = new();
        tcb.AddText(_localizer.Get("Dialogs_Common_SubscriptionExpired"));

        if (!_connectionManager.IsDisconnected)
        {
            tcb.AddText(_localizer.Get("Notifications_SubscriptionExpired_Description_Connected"));
        }

        tcb.AddText(_localizer.Get("Dialogs_Common_UpgradeToGetPlusFeatures"));
        tcb.AddButton(_localizer.Get("Common_Actions_Upgrade"), ToastActivationType.Foreground, NotificationArguments.UPGRADE);

        Send(tcb);
    }

    public void Receive(MainWindowVisibilityChangedMessage message)
    {
        _isMainWindowVisible = message.IsMainWindowVisible;
    }
}