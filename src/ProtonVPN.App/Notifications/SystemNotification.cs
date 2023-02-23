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

using Caliburn.Micro;
using ProtonVPN.Core;
using ProtonVPN.Core.Events;
using ProtonVPN.Core.Settings;

namespace ProtonVPN.Notifications
{
    internal class SystemNotification : ISystemNotification
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IAppSettings _appSettings;
        private readonly AppExitHandler _appExitHandler;

        public SystemNotification(IEventAggregator eventAggregator, IAppSettings appSettings, AppExitHandler appExitHandler)
        {
            _appExitHandler = appExitHandler;
            _appSettings = appSettings;
            _eventAggregator = eventAggregator;
        }

        public void Show(string message)
        {
            if (_appSettings.ShowNotifications && !_appExitHandler.PendingExit)
            {
                _eventAggregator.PublishOnUIThread(new ShowNotificationMessage(message));
            }
        }
    }
}
