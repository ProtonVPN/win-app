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

using System.ComponentModel;
using System.Threading.Tasks;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Service.Vpn;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Vpn;
using ProtonVPN.Vpn.Connectors;

namespace ProtonVPN.Settings
{
    internal class SettingChangeListener : ISettingsAware, IVpnStateAware
    {
        private readonly IVpnManager _vpnManager;
        private readonly QuickConnector _quickConnector;
        private VpnStatus _vpnStatus;
        private bool _pendingReconnect;

        public SettingChangeListener(IVpnManager vpnManager, QuickConnector quickConnector)
        {
            _quickConnector = quickConnector;
            _vpnManager = vpnManager;
        }

        public async void OnAppSettingsChanged(PropertyChangedEventArgs e)
        {
            if (_vpnStatus == VpnStatus.Disconnected)
            {
                return;
            }

            switch (e.PropertyName)
            {
                case nameof(IAppSettings.SecureCore):
                    _pendingReconnect = true;
                    await _quickConnector.Connect();
                    break;
                case nameof(IAppSettings.KillSwitch):
                case nameof(IAppSettings.NetShieldEnabled):
                case nameof(IAppSettings.NetShieldMode):
                    _pendingReconnect = true;
                    await _vpnManager.Reconnect();
                    break;
            }
        }

        public Task OnVpnStateChanged(VpnStateChangedEventArgs e)
        {
            _vpnStatus = e.State.Status;

            if (_vpnStatus == VpnStatus.Connected || _vpnStatus == VpnStatus.Disconnected)
            {
                _pendingReconnect = false;
            }

            return Task.CompletedTask;
        }

        public bool PendingReconnect()
        {
            return _pendingReconnect;
        }
    }
}