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

using System.Diagnostics;
using System.Windows;
using ProtonVPN.Core.Auth;
using ProtonVPN.Login.Views;
using ProtonVPN.Windows;
using ProtonVPN.Windows.Popups.DeveloperTools;

namespace ProtonVPN.Notifications
{
    public class NotificationUserActionHandler : INotificationUserActionHandler
    {
        private readonly IUserAuthenticator _userAuthenticator;
        private readonly AppWindow _appWindow;
        private readonly LoginWindow _loginWindow;
        private readonly DeveloperToolsPopupViewModel _developerToolsPopupViewModel;

        public NotificationUserActionHandler(
            IUserAuthenticator userAuthenticator,
            AppWindow appWindow,
            LoginWindow loginWindow,
            DeveloperToolsPopupViewModel developerToolsPopupViewModel)
        {
            _userAuthenticator = userAuthenticator;
            _appWindow = appWindow;
            _loginWindow = loginWindow;
            _developerToolsPopupViewModel = developerToolsPopupViewModel;
        }

        public void Handle(NotificationUserAction data)
        {
            if (data?.Arguments != null &&
                data.Arguments.ContainsKey(NotificationSender.ACTION_KEY) &&
                data.Arguments[NotificationSender.ACTION_KEY] == NotificationSender.OPEN_MAIN_WINDOW_ACTION_KEY)
            {
                OpenMainWindow();
            }

            SetDeveloperToolsToastNotificationUserAction(data);
        }

        private void OpenMainWindow()
        {
            if (_userAuthenticator.IsLoggedIn)
            {
                OpenWindow(_appWindow);
            }
            else
            {
                OpenWindow(_loginWindow);
            }
        }

        private void OpenWindow(Window window)
        {
            window.Show();
            window.Activate();
        }

        [Conditional("DEBUG")]
        private void SetDeveloperToolsToastNotificationUserAction(NotificationUserAction data)
        {
            _developerToolsPopupViewModel.OnToastNotificationUserAction(data);
        }
    }
}