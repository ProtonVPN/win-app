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

using System.Threading.Tasks;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Modals;
using ProtonVPN.Core.Network;
using ProtonVPN.Core.Vpn;
using ProtonVPN.Modals;

namespace ProtonVPN.Notifications
{
    internal class InsecureNetworkNotification : IVpnStateAware
    {
        private readonly IModals _modals;
        private VpnStatus _vpnStatus = VpnStatus.Disconnected;
        private string _name = string.Empty;

        public InsecureNetworkNotification(INetworkClient networkClient, IModals modals)
        {
            _modals = modals;
            networkClient.WifiChangeDetected += OnWifiChangeDetected;
        }

        public Task OnVpnStateChanged(VpnStateChangedEventArgs e)
        {
            _vpnStatus = e.State.Status;

            if (_vpnStatus == VpnStatus.Disconnected && !string.IsNullOrEmpty(_name))
            {
                ShowModal();
            }

            return Task.CompletedTask;
        }

        private void OnWifiChangeDetected(object sender, WifiChangeEventArgs e)
        {
            if (!e.Secure)
            {
                _name = e.Name;
                if (_vpnStatus != VpnStatus.Disconnected && _vpnStatus != VpnStatus.Disconnecting)
                {
                    return;
                }

                ShowModal();
            }
            else
            {
                _name = string.Empty;
                _modals.Close<InsecureWifiModalViewModel>();
            }
        }

        private void ShowModal()
        {
            _modals.Show<InsecureWifiModalViewModel>(new { Name = _name });
        }
    }
}
