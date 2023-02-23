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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Toolkit.Uwp.Notifications;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Networking;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core;
using ProtonVPN.Core.Auth;
using ProtonVPN.Core.Modals;
using ProtonVPN.Core.Servers.Models;
using ProtonVPN.Core.Service.Vpn;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Vpn;
using ProtonVPN.Core.Windows.Popups;
using ProtonVPN.Notifications;
using ProtonVPN.Sidebar;
using ProtonVPN.Translations;

namespace ProtonVPN.Windows.Popups.DeveloperTools
{
    public class DeveloperToolsPopupViewModel : BasePopupViewModel, IVpnStateAware
    {
        private readonly IConfiguration _config;
        private readonly IConfigWriter _configWriter;
        private readonly IUserAuthenticator _userAuthenticator;
        private readonly IPopupWindows _popupWindowOpener;
        private readonly IModals _modalOpener;
        private readonly INotificationSender _notificationSender;
        private readonly ConnectionStatusViewModel _connectionStatusViewModel;
        private readonly IAppSettings _appSettings;
        private readonly ReconnectManager _reconnectManager;
        private readonly IAppExitInvoker _appExitInvoker;

        public DeveloperToolsPopupViewModel(AppWindow appWindow,
            IConfiguration config,
            IConfigWriter configWriter,
            IUserAuthenticator userAuthenticator,
            IPopupWindows popupWindowOpener,
            IModals modalOpener,
            INotificationSender notificationSender,
            ConnectionStatusViewModel connectionStatusViewModel,
            IAppSettings appSettings,
            ReconnectManager reconnectManager,
            IAppExitInvoker appExitInvoker,
            IEnumerable<IModal> modals,
            IEnumerable<IPopupWindow> popupWindows)
            : base(appWindow)
        {
            _config = config;
            _configWriter = configWriter;
            _userAuthenticator = userAuthenticator;
            _popupWindowOpener = popupWindowOpener;
            _modalOpener = modalOpener;
            _notificationSender = notificationSender;
            _connectionStatusViewModel = connectionStatusViewModel;
            _appSettings = appSettings;
            _reconnectManager = reconnectManager;
            _appExitInvoker = appExitInvoker;
            LoadModals(modals);
            LoadPopupWindows(popupWindows);
            InitializeCommands();
        }

        private void LoadModals(IEnumerable<IModal> modals)
        {
            SortedDictionary<string, IModal> modalsByName = new();
            foreach (IModal modal in modals)
            {
                Type type = modal.GetType();
                string modalName = $"{type.Name} ({type.Namespace})";
                modalsByName.Add(modalName, modal);
            }
            SelectedModalName = modalsByName.Keys.FirstOrDefault();
            ModalsByName = modalsByName;
        }
        
        private void LoadPopupWindows(IEnumerable<IPopupWindow> popupWindows)
        {
            SortedDictionary<string, IPopupWindow> popupWindowsByName = new();
            foreach (IPopupWindow popupWindow in popupWindows)
            {
                Type type = popupWindow.GetType();
                string popupWindowName = $"{type.Name} ({type.Namespace})";
                popupWindowsByName.Add(popupWindowName, popupWindow);
            }
            SelectedPopupWindowName = popupWindowsByName.Keys.FirstOrDefault();
            PopupWindowsByName = popupWindowsByName;
        }

        [Conditional("DEBUG")]
        private void InitializeCommands()
        {
            OpenModalCommand = new RelayCommand(OpenModalAction);
            OpenAllModalsCommand = new RelayCommand(OpenAllModalsAction);
            OpenPopupWindowCommand = new RelayCommand(OpenPopupWindowAction);
            OpenAllPopupWindowsCommand = new RelayCommand(OpenAllPopupWindowsAction);
            RefreshVpnInfoCommand = new RelayCommand(RefreshVpnInfoAction);
            CheckIfCurrentServerIsOnlineCommand = new RelayCommand(CheckIfCurrentServerIsOnlineAction);
            ShowReconnectionTooltipCommand = new RelayCommand(ShowReconnectionTooltipAction);
            ResetDoNotShowAgainCommand = new RelayCommand(ResetDoNotShowAgainAction);
            FullToastCommand = new RelayCommand(FullToastAction);
            BasicToastCommand = new RelayCommand(BasicToastAction);
            ClearToastNotificationLogsCommand = new RelayCommand(ClearToastNotificationLogsAction);
            TriggerIntentionalCrashCommand = new RelayCommand(TriggerIntentionalCrashAction);
            DisableTlsPinningCommand = new RelayCommand(DisableTlsPinningAction);
        }

        public ICommand OpenModalCommand { get; set; }
        public ICommand OpenAllModalsCommand { get; set; }
        public ICommand OpenPopupWindowCommand { get; set; }
        public ICommand OpenAllPopupWindowsCommand { get; set; }
        public ICommand RefreshVpnInfoCommand { get; set; }
        public ICommand CheckIfCurrentServerIsOnlineCommand { get; set; }
        public ICommand ShowReconnectionTooltipCommand { get; set; }
        public ICommand ResetDoNotShowAgainCommand { get; set; }
        public ICommand FullToastCommand { get; set; }
        public ICommand BasicToastCommand { get; set; }
        public ICommand ClearToastNotificationLogsCommand { get; set; }
        public ICommand TriggerIntentionalCrashCommand { get; set; }
        public ICommand DisableTlsPinningCommand { get; set; }

        private IDictionary<string, IModal> _modalsByName;
        public IDictionary<string, IModal> ModalsByName
        {
            get => _modalsByName;
            set
            {
                _modalsByName = value;
                NotifyOfPropertyChange();
            }
        }

        private string _selectedModalName;
        public string SelectedModalName
        {
            get => _selectedModalName;
            set
            {
                _selectedModalName = value;
                NotifyOfPropertyChange();
            }
        }
        private IDictionary<string, IPopupWindow> _popupWindowsByName;
        public IDictionary<string, IPopupWindow> PopupWindowsByName
        {
            get => _popupWindowsByName;
            set
            {
                _popupWindowsByName = value;
                NotifyOfPropertyChange();
            }
        }

        private string _selectedPopupWindowName;
        public string SelectedPopupWindowName
        {
            get => _selectedPopupWindowName;
            set
            {
                _selectedPopupWindowName = value;
                NotifyOfPropertyChange();
            }
        }

        private bool _isConnected;
        public bool IsConnected
        {
            get => _isConnected;
            set
            {
                _isConnected = value;
                NotifyOfPropertyChange();
            }
        }

        private string _networkInformation;
        public string NetworkInformation
        {
            get => _networkInformation;
            set
            {
                _networkInformation = value;
                NotifyOfPropertyChange();
            }
        }

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

        private void OpenModalAction()
        {
            if (ModalsByName.TryGetValue(SelectedModalName, out IModal modal))
            {
                _modalOpener.Show(type: modal.GetType());
            }
        }

        private void OpenAllModalsAction()
        {
            foreach (KeyValuePair<string, IModal> modalByName in ModalsByName)
            {
                _modalOpener.Show(type: modalByName.Value.GetType());
            }
        }

        private void OpenPopupWindowAction()
        {
            if (PopupWindowsByName.TryGetValue(SelectedPopupWindowName, out IPopupWindow popupWindow))
            {
                SetSubscriptionExpiredPopupIfNeeded(popupWindow);
                _popupWindowOpener.Show(type: popupWindow.GetType());
            }
        }

        private void SetSubscriptionExpiredPopupIfNeeded(IPopupWindow popupWindow)
        {
            if (popupWindow is IReconnectionDataPopupViewModel reconnectionDataPopupViewModel)
            {
                Server previousServer = new(Guid.NewGuid().ToString(), "CH-PT#20", "Porto",
                    "CH", "PT", "protonvpn.com", 0, 2, 1, 50, 1, null, null, "192.168.123.124");
                Server currentServer = new(Guid.NewGuid().ToString(), "SE-PT#23", "Porto",
                    "SE", "PT", "protonvpn.com", 1, 2, 1, 30, 1, null, null, "192.168.123.125");
                reconnectionDataPopupViewModel.SetReconnectionData(previousServer, currentServer);
            }
        }

        private void OpenAllPopupWindowsAction()
        {
            foreach (KeyValuePair<string, IPopupWindow> popupWindowByName in PopupWindowsByName)
            {
                SetSubscriptionExpiredPopupIfNeeded(popupWindowByName.Value);
                _popupWindowOpener.Show(type: popupWindowByName.Value.GetType());
            }
        }

        private async void RefreshVpnInfoAction()
        {
            try
            {
                await _userAuthenticator.RefreshVpnInfoAsync();
            }
            catch (Exception ex)
            {
                _notificationSender.Send("Request failed",
                    $"[{ex.GetType().Name}] Exception message: {ex.CombinedMessage()}");
            }
        }

        private async void CheckIfCurrentServerIsOnlineAction()
        {
            await _reconnectManager.CheckIfCurrentServerIsOnlineAsync();
        }

        private async void ShowReconnectionTooltipAction()
        {
            await Task.Delay(TimeSpan.FromSeconds(5));
            Server previousServer = new(Guid.NewGuid().ToString(), "CH-PT#20", "Porto",
                "CH", "PT", "protonvpn.com", 0, 2, 1, 50, 1, null, null, "192.168.123.124");
            Server currentServer = new(Guid.NewGuid().ToString(), "SE-PT#23", "Porto",
                "SE", "PT", "protonvpn.com", 1, 2, 1, 30, 1, null, null, "192.168.123.125");
            _connectionStatusViewModel.ShowVpnAcceleratorReconnectionPopup(previousServer: previousServer, currentServer: currentServer);
        }

        private void ResetDoNotShowAgainAction()
        {
            _appSettings.DoNotShowEnableSmartProtocolDialog = false;
            _appSettings.DoNotShowKillSwitchConfirmationDialog = false;
            _appSettings.DoNotShowPortForwardingConfirmationDialog = false;
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

        private void TriggerIntentionalCrashAction()
        {
            throw new StackOverflowException("Intentional crash test");
        }

        public async Task OnVpnStateChanged(VpnStateChangedEventArgs e)
        {
            if (e.State.Status == VpnStatus.Connected)
            {
                IsConnected = true;
                NetworkInformation = GenerateNetworkAdapterString(e.State);
            }
            else
            {
                IsConnected = false;
                NetworkInformation = "Disconnected";
            }
        }

        private string GenerateNetworkAdapterString(VpnState vpnState)
        {
            string result = "Connected" + Environment.NewLine + GenerateVpnProtocolString(vpnState.VpnProtocol);
            if (vpnState.VpnProtocol == VpnProtocol.WireGuard)
            {
                return result;
            }

            return result + Environment.NewLine + GenerateOpenVpnAdapterString(vpnState.NetworkAdapterType);
        }

        private string GenerateVpnProtocolString(VpnProtocol vpnProtocol)
        {
            return $"Network protocol: {vpnProtocol}";
        }

        private string GenerateOpenVpnAdapterString(OpenVpnAdapter? networkAdapterType)
        {
            return $"Network adapter: {networkAdapterType}";
        }

        private void DisableTlsPinningAction()
        {
            _configWriter.Write(_config.DisableTlsPinning());
            _appExitInvoker.Exit();
        }
    }
}