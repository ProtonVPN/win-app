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

using Microsoft.Toolkit.Uwp.Notifications;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.Models;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;
using ProtonVPN.Client.Settings.Contracts;

namespace ProtonVPN.Client.Notifications;

public class ConnectionStatusNotificationSender : IConnectionStatusNotificationSender
{
    private readonly ISettings _settings;
    private readonly ILocalizationProvider _localizer;
    private readonly IConnectionManager _connectionManager;

    private ConnectionStatus _lastStatus;

    public ConnectionStatusNotificationSender(ISettings settings, ILocalizationProvider localizer,
        IConnectionManager connectionManager)
    {
        _settings = settings;
        _localizer = localizer;
        _connectionManager = connectionManager;
    }

    public void Send(ConnectionStatus currentStatus)
    {
        if (!IsToNotify(currentStatus))
        {
            return;
        }

        _lastStatus = currentStatus;

        string title = GetNotificationTitle(currentStatus);
        if (string.IsNullOrEmpty(title))
        {
            return;
        }

        ToastContentBuilder notification = new();
        notification.AddText(_localizer.Get(title), AdaptiveTextStyle.Header);

        if (currentStatus == ConnectionStatus.Connected)
        {
            AddLocationDetailsToNotification(notification);
        }

        notification.Show();
    }

    private bool IsToNotify(ConnectionStatus currentStatus)
    {
        return _settings.IsNotificationEnabled &&
               (currentStatus == ConnectionStatus.Connected || (_lastStatus == ConnectionStatus.Connected && currentStatus == ConnectionStatus.Disconnected));
    }

    private string GetNotificationTitle(ConnectionStatus connectionStatus)
    {
        return connectionStatus switch
        {
            ConnectionStatus.Connected => "SystemNotification_Connected",
            ConnectionStatus.Disconnected => "SystemNotification_Disconnected",
            _ => string.Empty,
        };
    }

    private void AddLocationDetailsToNotification(ToastContentBuilder notification)
    {
        string? locationDetails = GetLocationDetails();
        if (locationDetails != null)
        {
            notification.AddText(_localizer.GetFormat("SystemNotification_ConnectedTo", locationDetails));
        }
    }

    private string? GetLocationDetails()
    {
        ConnectionDetails? connectionDetails = _connectionManager.CurrentConnectionDetails;
        if (connectionDetails == null)
        {
            return null;
        }

        string locationDetails = _localizer.GetConnectionDetailsTitle(_connectionManager.CurrentConnectionDetails);
        string connectionIntentSubtitle = _localizer.GetConnectionDetailsSubtitle(_connectionManager.CurrentConnectionDetails);

        if (!string.IsNullOrEmpty(connectionIntentSubtitle))
        {
            locationDetails += $" {connectionIntentSubtitle}";
        }

        IFeatureIntent? featureIntent = _connectionManager.CurrentConnectionIntent?.Feature;
        if (featureIntent != null && featureIntent is P2PFeatureIntent or TorFeatureIntent)
        {
            locationDetails += $" {featureIntent}";
        }

        return locationDetails;
    }
}