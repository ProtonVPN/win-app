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
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;

namespace ProtonVPN.Client.Notifications;

public class ConnectionErrorNotificationSender : IConnectionErrorNotificationSender,
    IEventMessageReceiver<MainWindowVisibilityChangedMessage>
{
    private readonly ILocalizationProvider _localizer;

    private bool _isMainWindowVisible;

    public ConnectionErrorNotificationSender(ILocalizationProvider localizer)
    {
        _localizer = localizer;
    }

    public void Send(VpnError vpnError)
    {
        if (_isMainWindowVisible)
        {
            return;
        }

        ToastContentBuilder tcb = new();
        tcb.AddText(_localizer.Get("SystemNotification_Disconnected"));
        tcb.AddText(_localizer.Get("Notifications_SessionLimit_Description"));
        tcb.Show();
    }

    public void Receive(MainWindowVisibilityChangedMessage message)
    {
        _isMainWindowVisible = message.IsMainWindowVisible;
    }
}