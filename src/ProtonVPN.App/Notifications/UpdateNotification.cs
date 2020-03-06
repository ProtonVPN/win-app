/*
 * Copyright (c) 2020 Proton Technologies AG
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
using ProtonVPN.Core.Auth;
using ProtonVPN.Core.Modals;
using ProtonVPN.Core.Update;
using ProtonVPN.Modals;
using ProtonVPN.Resources;

namespace ProtonVPN.Notifications
{
    public class UpdateNotification : IUpdateStateAware
    {
        private readonly IModals _modals;
        private readonly ISystemNotification _systemNotification;
        private readonly TimeSpan _remindInterval;
        private readonly UserAuth _userAuth;

        private DateTime _lastNotified = DateTime.MinValue;
        

        public UpdateNotification(
            TimeSpan remindInterval,
            ISystemNotification systemNotification,
            IModals modals,
            UserAuth userAuth)
        {
            _modals = modals;
            _systemNotification = systemNotification;
            _remindInterval = remindInterval;
            _userAuth = userAuth;
        }

        public void OnUpdateStateChanged(UpdateStateChangedEventArgs e)
        {
            if (e.Ready)
            {
                if (RemindRequired(e) && _userAuth.LoggedIn)
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
            return e.Status == UpdateStatus.Ready && (e.ManualCheck ||
                DateTime.Now - _lastNotified >= _remindInterval);
        }

        private void Show()
        {
            _lastNotified = DateTime.Now;
            _systemNotification.Show(StringResources.Get("Dialogs_Update_msg_NewAppVersion"));
            _modals.Show<UpdateModalViewModel>();
        }

        private void Hide()
        {
            _modals.Close<UpdateModalViewModel>();
        }
    }
}
