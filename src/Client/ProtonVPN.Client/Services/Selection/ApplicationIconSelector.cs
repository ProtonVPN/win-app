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

using System.Drawing;
using Microsoft.UI.Xaml.Media;
using ProtonVPN.Client.Core.Enums;
using ProtonVPN.Client.Core.Helpers;
using ProtonVPN.Client.Core.Messages;
using ProtonVPN.Client.Core.Services.Selection;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Extensions;
using ProtonVPN.Client.Settings.Contracts.Messages;
using ProtonVPN.Common.Core.Helpers;
using ProtonVPN.Configurations.Contracts;

namespace ProtonVPN.Client.Services.Selection;

public class ApplicationIconSelector : IApplicationIconSelector,
    IEventMessageReceiver<ConnectionStatusChangedMessage>,
    IEventMessageReceiver<AuthenticationStatusChanged>,
    IEventMessageReceiver<SettingChangedMessage>
{
    public const string PROTON_VPN_ICON_PATH = "Assets/ProtonVPN.ico";

    public static readonly ImageSource ConnectedTrayIcon = ResourceHelper.GetIcon("ProtonVpnProtectedTrayIcon");
    public static readonly ImageSource ErrorTrayIcon = ResourceHelper.GetIcon("ProtonVpnErrorTrayIcon");
    public static readonly ImageSource WarningTrayIcon = ResourceHelper.GetIcon("ProtonVpnWarningTrayIcon");
    public static readonly ImageSource DisconnectedTrayIcon = ResourceHelper.GetIcon("ProtonVpnUnprotectedTrayIcon");
    public static readonly ImageSource LoggedOutTrayIcon = ResourceHelper.GetIcon("ProtonVpnLoggedOutTrayIcon");

    public readonly Icon ConnectedBadgeIcon;
    public readonly Icon DisconnectedBadgeIcon;
    public readonly Icon ErrorBadgeIcon;
    public readonly Icon WarningBadgeIcon;

    private readonly IConfiguration _configuration;
    private readonly IUserAuthenticator _userAuthenticator;
    private readonly IConnectionManager _connectionManager;
    private readonly ISettings _settings;
    private readonly IEventMessageSender _eventMessageSender;

    private bool _hasAuthenticationError;
    private bool _hasConnectionError;

    public AppIconStatus AppIconStatus { get; private set; } = AppIconStatus.None;

    public ApplicationIconSelector(
        IConfiguration configuration,
        IUserAuthenticator userAuthenticator,
        IConnectionManager connectionManager,
        ISettings settings,
        IEventMessageSender eventMessageSender)
    {
        _configuration = configuration;
        _userAuthenticator = userAuthenticator;
        _connectionManager = connectionManager;
        _settings = settings;
        _eventMessageSender = eventMessageSender;

        if (OSVersion.IsWindows11OrHigher())
        {
            ConnectedBadgeIcon = GetBadgeIcon("badge-protected.ico");
            DisconnectedBadgeIcon = GetBadgeIcon("badge-unprotected.ico");
            ErrorBadgeIcon = GetBadgeIcon("badge-error.ico");
            WarningBadgeIcon = GetBadgeIcon("badge-warning.ico");
        }
        else
        {
            ConnectedBadgeIcon = GetBadgeIcon("badge-protected-w10.ico");
            DisconnectedBadgeIcon = GetBadgeIcon("badge-unprotected-w10.ico");
            ErrorBadgeIcon = GetBadgeIcon("badge-error-w10.ico");
            WarningBadgeIcon = GetBadgeIcon("badge-warning-w10.ico");
        }
    }

    public void OnAuthenticationErrorTriggered()
    {
        _hasAuthenticationError = true;
        InvalidateAppIconStatus();
    }

    public void OnAuthenticationErrorDismissed()
    {
        _hasAuthenticationError = false;
        InvalidateAppIconStatus();
    }

    public void OnConnectionErrorTriggered()
    {
        _hasConnectionError = true;
        InvalidateAppIconStatus();
    }

    public void OnConnectionErrorDismissed()
    {
        _hasConnectionError = false;
        InvalidateAppIconStatus();
    }

    public string GetAppIconPath()
    {
        return PROTON_VPN_ICON_PATH;
    }

    public ImageSource GetStatusIcon()
    {
        return AppIconStatus switch
        {
            AppIconStatus.Connected => ConnectedTrayIcon,
            AppIconStatus.Disconnected => DisconnectedTrayIcon,
            AppIconStatus.Warning => WarningTrayIcon,
            AppIconStatus.Error => ErrorTrayIcon,
            _ => LoggedOutTrayIcon
        };
    }

    public Icon? GetTaskbarBadgeIcon()
    {
        return AppIconStatus switch
        {
            AppIconStatus.Connected => ConnectedBadgeIcon,
            AppIconStatus.Disconnected => DisconnectedBadgeIcon,
            AppIconStatus.Warning => WarningBadgeIcon,
            AppIconStatus.Error => ErrorBadgeIcon,
            _ => null
        };
    }

    public void Receive(ConnectionStatusChangedMessage message)
    {
        InvalidateAppIconStatus();
    }

    public void Receive(AuthenticationStatusChanged message)
    {
        InvalidateAppIconStatus();
    }

    public void Receive(SettingChangedMessage message)
    {
        if (message.PropertyName is nameof(ISettings.KillSwitchMode) or nameof(ISettings.IsKillSwitchEnabled))
        {
            InvalidateAppIconStatus();
        }
    }

    private void InvalidateAppIconStatus()
    {
        bool isAdvancedKillSwitchActive = _connectionManager.IsDisconnected && _settings.IsAdvancedKillSwitchActive();

        AppIconStatus status = _userAuthenticator.IsLoggedIn
            ? _connectionManager.IsConnected
                ? AppIconStatus.Connected
                : _hasConnectionError
                    ? AppIconStatus.Error
                    : isAdvancedKillSwitchActive
                        ? AppIconStatus.Warning
                        : AppIconStatus.Disconnected
            : _hasAuthenticationError
                ? AppIconStatus.Error
                : isAdvancedKillSwitchActive
                    ? AppIconStatus.Warning
                    : AppIconStatus.None;

        if (AppIconStatus != status)
        {
            AppIconStatus = status;
            _eventMessageSender.Send(new AppIconStatusChangedMessage(status));
        }
    }

    private Icon GetBadgeIcon(string iconFileName)
    {
        return new Icon(Path.Combine(_configuration.AssetsFolder, "Icons", "App", iconFileName));
    }
}