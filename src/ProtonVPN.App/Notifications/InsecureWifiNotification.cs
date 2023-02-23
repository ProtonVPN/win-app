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

using System.Threading.Tasks;
using Caliburn.Micro;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Auth;
using ProtonVPN.Core.Network;
using ProtonVPN.Core.Vpn;
using ProtonVPN.FlashNotifications;

namespace ProtonVPN.Notifications
{
    internal class InsecureNetworkNotification : IVpnStateAware, ILoggedInAware
    {
        private readonly InsecureWifiNotificationViewModel _insecureWifiNotificationViewModel;
        private readonly IEventAggregator _eventAggregator;
        private VpnStatus _vpnStatus = VpnStatus.Disconnected;
        private string _currentInsecureWifiName = string.Empty;

        public InsecureNetworkNotification(
            INetworkClient networkClient,
            InsecureWifiNotificationViewModel insecureWifiNotificationViewModel,
            IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _insecureWifiNotificationViewModel = insecureWifiNotificationViewModel;
            networkClient.WifiChangeDetected += OnWifiChangeDetected;
        }

        public async Task OnVpnStateChanged(VpnStateChangedEventArgs e)
        {
            if (_vpnStatus == e.State.Status)
            {
                return;
            }

            _vpnStatus = e.State.Status;
            HandleNotification();
        }

        public void OnUserLoggedIn()
        {
            HandleNotification();
        }

        private void HandleNotification()
        {
            if (IsCurrentWifiSecure)
            {
                return;
            }

            switch (_vpnStatus)
            {
                case VpnStatus.Connected:
                    HideNotification();
                    break;
                case VpnStatus.Disconnected:
                    ShowNotification();
                    break;
            }
        }

        private bool IsCurrentWifiSecure => string.IsNullOrEmpty(_currentInsecureWifiName);

        private void OnWifiChangeDetected(object sender, WifiChangeEventArgs e)
        {
            if (e.Secure)
            {
                HideNotification();
                _currentInsecureWifiName = string.Empty;
            }
            else
            {
                _currentInsecureWifiName = e.Name;
                if (_vpnStatus != VpnStatus.Disconnected && _vpnStatus != VpnStatus.Disconnecting)
                {
                    return;
                }

                ShowNotification();
            }
        }

        private void ShowNotification()
        {
            _insecureWifiNotificationViewModel.Name = _currentInsecureWifiName;
            _eventAggregator.PublishOnUIThread(new ShowFlashMessage(_insecureWifiNotificationViewModel));
        }

        private void HideNotification()
        {
            _eventAggregator.PublishOnUIThread(new HideFlashMessage(_insecureWifiNotificationViewModel));
        }
    }
}