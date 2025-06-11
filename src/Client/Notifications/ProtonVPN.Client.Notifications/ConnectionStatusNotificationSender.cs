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
using ProtonVPN.Client.Contracts.Messages;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.GuestHole;
using ProtonVPN.Client.Logic.Connection.Contracts.Models;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Extensions;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.Notifications;

public class ConnectionStatusNotificationSender : NotificationSenderBase, IConnectionStatusNotificationSender,
    IEventMessageReceiver<MainWindowVisibilityChangedMessage>
{
    private readonly ISettings _settings;
    private readonly ILocalizationProvider _localizer;
    private readonly IGuestHoleManager _guestHoleManager;
    private readonly IConnectionManager _connectionManager;
    private readonly IConfiguration _configuration;

    private ConnectionStatus _lastStatus;
    private bool _isMainWindowVisible;

    public ConnectionStatusNotificationSender(
        ILogger logger,
        ISettings settings,
        ILocalizationProvider localizer,
        IGuestHoleManager guestHoleManager,
        IConnectionManager connectionManager,
        IConfiguration configuration)
        : base(logger)
    {
        _settings = settings;
        _localizer = localizer;
        _guestHoleManager = guestHoleManager;
        _connectionManager = connectionManager;
        _configuration = configuration;
    }

    public void Send(ConnectionStatus currentStatus)
    {
        if (_isMainWindowVisible || _guestHoleManager.IsActive || !IsToNotify(currentStatus))
        {
            _lastStatus = currentStatus;
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

        AddDescription(notification, currentStatus);

        Send(notification);
    }

    public void Receive(MainWindowVisibilityChangedMessage message)
    {
        _isMainWindowVisible = message.IsMainWindowVisible;
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

    private void AddDescription(ToastContentBuilder notification, ConnectionStatus connectionStatus)
    {
        string? description = null;

        if (connectionStatus == ConnectionStatus.Connected)
        {
            string? locationDetails = GetLocationDetails();
            if (locationDetails != null)
            {
                description = _localizer.GetFormat("SystemNotification_ConnectedTo", locationDetails);
            }
        }
        else if (connectionStatus == ConnectionStatus.Disconnected && _settings.IsAdvancedKillSwitchActive())
        {
            notification.AddAppLogoOverride(new Uri(Path.Combine(_configuration.AssetsFolder, "Illustrations", "kill-switch-protected.png")));
            description = _localizer.Get("Notifications_KillSwitch_Description");
        }

        if (description is not null)
        {
            notification.AddText(description);
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