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
using GalaSoft.MvvmLight.Command;
using ProtonVPN.Core.Auth;
using ProtonVPN.Core.MVVM;
using ProtonVPN.Notifications;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace ProtonVPN.FlashNotifications
{
    public class FlashNotificationViewModel :
        ViewModel,
        IHandle<ShowFlashMessage>,
        IHandle<HideFlashMessage>,
        ILogoutAware
    {
        private readonly INotificationSender _notificationSender;

        public FlashNotificationViewModel(IEventAggregator eventAggregator, INotificationSender notificationSender)
        {
            _notificationSender = notificationSender;
            eventAggregator.Subscribe(this);
            CloseMessageCommand = new RelayCommand<INotification>(CloseMessage);
            Notifications = new ObservableCollection<INotification>();
        }

        public ICommand CloseMessageCommand { get; set; }
        public ObservableCollection<INotification> Notifications { get; set; }

        public void Handle(ShowFlashMessage message)
        {
            ShowMessage(message.Message);
        }

        public void Handle(HideFlashMessage message)
        {
            CloseMessage(message.Message);
        }

        public void OnUserLoggedOut()
        {
            Notifications.Clear();
        }

        private void CloseMessage(INotification message)
        {
            int index = Notifications.IndexOf(message);
            if (index != -1)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Notifications.RemoveAt(index);
                });
            }
        }

        private void ShowMessage(INotification notification)
        {
            int index = Notifications.IndexOf(notification);
            if (index == -1)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Notifications.Add(notification);
                });
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Notifications.RemoveAt(index);
                    Notifications.Add(notification);
                });
            }

            _notificationSender.Send(notification.Message);
        }
    }
}
