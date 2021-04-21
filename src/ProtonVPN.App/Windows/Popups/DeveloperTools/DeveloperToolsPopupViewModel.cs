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
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Toolkit.Uwp.Notifications;
using ProtonVPN.Core.Auth;
using ProtonVPN.Core.Modals;
using ProtonVPN.Core.Window.Popups;
using ProtonVPN.Modals.SessionLimits;
using ProtonVPN.Notifications;
using ProtonVPN.Translations;
using ProtonVPN.Windows.Popups.Delinquency;
using ProtonVPN.Windows.Popups.SubscriptionExpiration;

namespace ProtonVPN.Windows.Popups.DeveloperTools
{
    public class DeveloperToolsPopupViewModel : BasePopupViewModel
    {
        private readonly UserAuth _userAuth;
        private readonly IPopupWindows _popups;
        private readonly IModals _modals;
        private readonly INotificationSender _notificationSender;

        public DeveloperToolsPopupViewModel(AppWindow appWindow,
            UserAuth userAuth, 
            IPopupWindows popups, 
            IModals modals, 
            INotificationSender notificationSender)
            : base(appWindow)
        {
            _userAuth = userAuth;
            _popups = popups;
            _modals = modals;
            _notificationSender = notificationSender;

            ShowModalCommand = new RelayCommand(ShowModalAction);
            ShowPopupWindowCommand = new RelayCommand(ShowPopupWindowAction);
            RefreshVpnInfoCommand = new RelayCommand(RefreshVpnInfoAction);
            FullToastCommand = new RelayCommand(FullToastAction);
            BasicToastCommand = new RelayCommand(BasicToastAction);
            ClearToastNotificationLogsCommand = new RelayCommand(ClearToastNotificationLogsAction);
        }

        public ICommand ShowModalCommand { get; set; }
        public ICommand ShowPopupWindowCommand { get; set; }
        public ICommand RefreshVpnInfoCommand { get; set; }
        public ICommand FullToastCommand { get; set; }
        public ICommand BasicToastCommand { get; set; }
        public ICommand ClearToastNotificationLogsCommand { get; set; }

        private string _toastNotificationLog;
        public string ToastNotificationLog
        {
            get => _toastNotificationLog;
            private set
            {
                if (value == _toastNotificationLog)
                {
                    return;
                }
                _toastNotificationLog = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsHardwareAccelerationEnabled
        {
            get => RenderOptions.ProcessRenderMode == RenderMode.Default;
            set => SetHardwareAcceleration(value);
        }

        private void SetHardwareAcceleration(bool isToEnableHardwareAcceleration)
        {
            RenderOptions.ProcessRenderMode = isToEnableHardwareAcceleration ? RenderMode.Default : RenderMode.SoftwareOnly;
        }

        private void ShowModalAction()
        {
            _modals.Show<MaximumDeviceLimitModalViewModel>();
        }

        private void ShowPopupWindowAction()
        {
            _popups.Show<SubscriptionExpiredPopupViewModel>();
            _popups.Show<DelinquencyPopupViewModel>();
        }

        private async void RefreshVpnInfoAction()
        {
            await _userAuth.RefreshVpnInfo();
        }

        private void FullToastAction()
        {
            new ToastContentBuilder()
                .AddArgument("toastMotive", "Clicked on the DeveloperTools toast button.")
                .AddArgument("userId", 1234)
                .AddText("This is a toast")
                .AddText("A toast has been created because you wanted one. So here you go, enjoy watching this toast while you consider clicking on the button again to show another.")
                .AddInlineImage(new Uri(Environment.CurrentDirectory + "\\Windows\\Popups\\DeveloperTools\\Images\\computers.png"))
                //.AddAppLogoOverride(new Uri(Environment.CurrentDirectory + "\\other-logo.png"), ToastGenericAppLogoCrop.Circle)
                .AddInputTextBox("inputReply", placeHolderContent: "Type a reply")
                .AddButton(new ToastButton()
                    .SetContent("Reply")
                    .AddArgument("action", "reply"))
                .AddButton(new ToastButton()
                    .SetContent("Like")
                    .AddArgument("action", "like")
                    .AddArgument("postId", 56789)
                    .AddArgument("posterName", "FirstName FamilyName"))
                .Show();
        }

        private void BasicToastAction()
        {
            _notificationSender.Send(
                Translation.Get("Dialogs_Delinquency_Title"), 
                Translation.Get("Dialogs_Delinquency_Subtitle"));
        }

        public void OnToastNotificationUserAction(NotificationUserAction data)
        {
            string toastNotificationLog = $"Date: {DateTime.Now}" + Environment.NewLine;
            toastNotificationLog += $"Arguments: {data.Arguments.Count}" + Environment.NewLine;
            foreach (KeyValuePair<string, string> argument in data.Arguments)
            {
                toastNotificationLog += $"   {argument.Key}: {argument.Value}" + Environment.NewLine;
            }
            toastNotificationLog += $"User inputs: {data.UserInputs.Count}" + Environment.NewLine;
            foreach (KeyValuePair<string, object> userInput in data.UserInputs)
            {
                toastNotificationLog += $"   {userInput.Key}: {userInput.Value}" + Environment.NewLine;
            }
            toastNotificationLog += "--------------------" + Environment.NewLine;

            ToastNotificationLog = toastNotificationLog + ToastNotificationLog;
        }

        private void ClearToastNotificationLogsAction()
        {
            ToastNotificationLog = string.Empty;
        }
    }
}
