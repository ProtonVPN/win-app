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

using System.Collections.Generic;
using System.Threading.Tasks;
using ProtonVPN.Common.Networking;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Vpn;

namespace ProtonVPN.Settings.ReconnectNotification
{
    public class ReconnectState : IVpnStateAware
    {
        private VpnStatus _vpnStatus;
        private VpnProtocol _vpnProtocol = VpnProtocol.Smart;

        private readonly SettingsBuilder _settingsBuilder;
        private List<Setting> _reconnectRequiredSettings = new();

        public ReconnectState(SettingsBuilder settingsBuilder)
        {
            _settingsBuilder = settingsBuilder;
        }

        public bool Required(string settingChanged)
        {
            if (_vpnStatus == VpnStatus.Disconnecting ||
                _vpnStatus == VpnStatus.Disconnected)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(settingChanged))
            {
                OnSettingChanged(settingChanged);
            }

            foreach (Setting setting in _reconnectRequiredSettings)
            {
                if (setting.Changed(_vpnProtocol))
                {
                    return true;
                }
            }

            return false;
        }

        public Task OnVpnStateChanged(VpnStateChangedEventArgs e)
        {
            _vpnStatus = e.State.Status;
            _vpnProtocol = e.State.VpnProtocol;

            if (_vpnStatus == VpnStatus.Connected)
            {
                _reconnectRequiredSettings = _settingsBuilder.Build();
            }

            return Task.CompletedTask;
        }

        private void RevertChanges(Setting setting, string name)
        {
            if (setting.Name == name)
            {
                setting.SetChangesReverted();
                return;
            }

            foreach (Setting s in setting.GetChildren())
            {
                if (s.Name == name)
                {
                    s.SetChangesReverted();
                    return;
                }

                RevertChanges(s, name);
            }
        }

        private void OnSettingChanged(string settingName)
        {
            foreach (Setting setting in _reconnectRequiredSettings)
            {
                RevertChanges(setting, settingName);
            }
        }
    }
}