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
using Caliburn.Micro;
using ProtonVPN.Common.Configuration;
using ProtonVPN.FlashNotifications;
using ProtonVPN.Update;
using ProtonVPN.Update.Contracts;

namespace ProtonVPN.Notifications
{
    public class UpdateNotification : IUpdateStateAware
    {
        private readonly TimeSpan _remindInterval;
        private readonly IEventAggregator _eventAggregator;
        private readonly UpdateFlashNotificationViewModel _notificationViewModel;
        private DateTime _lastNotified = DateTime.MinValue;

        public UpdateNotification(
            IConfiguration config,
            IEventAggregator eventAggregator,
            UpdateFlashNotificationViewModel notificationViewModel)
        {
            _remindInterval = config.UpdateRemindInterval;
            _eventAggregator = eventAggregator;
            _notificationViewModel = notificationViewModel;
        }

        public void OnUpdateStateChanged(UpdateStateChangedEventArgs e)
        {
            if (e.Ready)
            {
                if (RemindRequired(e))
                {
                    Show();
                }
            }
            else
            {
                Hide();
            }
        }

        private bool RemindRequired(UpdateStateChangedEventArgs e)
        {
            return (e.Status is AppUpdateStatus.Ready
                or AppUpdateStatus.AutoUpdated
                or AppUpdateStatus.AutoUpdateFailed) && (e.ManualCheck || DateTime.UtcNow - _lastNotified >= _remindInterval);
        }

        private void Show()
        {
            _lastNotified = DateTime.UtcNow;
            _eventAggregator.PublishOnUIThreadAsync(new ShowFlashMessage(_notificationViewModel));
        }

        private void Hide()
        {
            _eventAggregator.PublishOnUIThreadAsync(new HideFlashMessage(_notificationViewModel));
        }
    }
}