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

using Microsoft.Toolkit.Uwp.Notifications;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.UnsecureWifiDetection.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.Notifications;

public class UnsecuredWifiNotificationSender : NotificationSenderBase,
    IEventMessageReceiver<ConnectionStatusChangedMessage>
{
    private readonly ILocalizationProvider _localizer;

    private ConnectionStatus _connectionStatus;

    private string _currentUnsecureWifiName = string.Empty;

    private bool IsCurrentWifiSecure => string.IsNullOrEmpty(_currentUnsecureWifiName);

    public UnsecuredWifiNotificationSender(
        ILogger logger,
        ILocalizationProvider localizationProvider,
        INetworkClient networkClient)
        : base(logger)
    {
        _localizer = localizationProvider;

        networkClient.WifiChangeDetected += OnWifiChangeDetected;
    }

    public void Receive(ConnectionStatusChangedMessage message)
    {
        if (_connectionStatus == message.ConnectionStatus)
        {
            return;
        }

        _connectionStatus = message.ConnectionStatus;

        HandleNotification();
    }

    private void HandleNotification()
    {
        if (IsCurrentWifiSecure || _connectionStatus != ConnectionStatus.Disconnected)
        {
            return;
        }

        Send();
    }

    public void Send()
    {
        Send(new ToastContentBuilder()
            .AddText(_localizer.Get("Notifications_UnsecureWifi_Title"))
            .AddText(_localizer.Get("Notifications_UnsecureWifi_Description")));
    }

    private void OnWifiChangeDetected(object? sender, WifiChangeEventArgs e)
    {
        if (e.IsSecure)
        {
            _currentUnsecureWifiName = string.Empty;
        }
        else
        {
            _currentUnsecureWifiName = e.Name;

            HandleNotification();
        }
    }
}