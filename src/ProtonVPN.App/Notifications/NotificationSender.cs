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
using System.Runtime.CompilerServices;
using Microsoft.Toolkit.Uwp.Notifications;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Helpers;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.AppLogs;
using ProtonVPN.Core.Settings;

namespace ProtonVPN.Notifications
{
    public class NotificationSender : INotificationSender
    {
        public const string ACTION_KEY = "action";
        public const string OPEN_MAIN_WINDOW_ACTION_KEY = "OpenMainWindow";

        private readonly ISystemNotification _systemNotification;
        private readonly IAppSettings _appSettings;
        private readonly ILogger _logger;
        private readonly bool _isNativeWindows10Notification;

        public NotificationSender(ISystemNotification systemNotification, IAppSettings appSettings, ILogger logger)
        {
            _systemNotification = systemNotification;
            _appSettings = appSettings;
            _logger = logger;
            _isNativeWindows10Notification = IsNativeWindow10Notification();
        }

        /// Build version specified in
        /// https://docs.microsoft.com/en-us/windows/uwp/design/shell/tiles-and-notifications/send-local-toast-cpp-uwp#handling-background-activation
        private bool IsNativeWindow10Notification()
        {
            OperatingSystem osVersion = Environment.OSVersion;
            return osVersion.Version.Major == 10 && osVersion.Version.Build >= 14393;
        }

        public void Send(string title, string description,
            [CallerFilePath] string sourceFilePath = "",
            [CallerMemberName] string sourceMemberName = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            if (_appSettings.ShowNotifications)
            {
                if (_isNativeWindows10Notification)
                {
                    CallerProfile callerProfile = new(sourceFilePath, sourceMemberName, sourceLineNumber);
                    SendNativeWindows10Notification(title, description, callerProfile);
                }
                else
                {
                    _systemNotification.Show(title);
                }
            }
        }

        private void SendNativeWindows10Notification(string title, string description, CallerProfile callerProfile)
        {
            try
            {
                ToastContentBuilder toastContentBuilder = new ToastContentBuilder()
                    .AddText(title);
                if (!description.IsNullOrEmpty())
                {
                    toastContentBuilder = toastContentBuilder.AddText(description);
                }
                toastContentBuilder
                    .AddArgument(ACTION_KEY, OPEN_MAIN_WINDOW_ACTION_KEY)
                    .Show();
            }
            // For some users the exception of "The notification platform is unavailable." is thrown.
            catch (Exception e)
            {
                _logger.Fatal<AppLog>(
                    $"The app failed to show native notification with title \"{title}\" and description \"{description}\".",
                    e, callerProfile.SourceClassName, callerProfile.SourceMemberName, callerProfile.SourceLineNumber);
            }
        }
    }
}