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

using ProtonVPN.Common;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Service.Firewall;
using ProtonVPN.Service.Settings;
using ProtonVPN.Service.Vpn;
using ProtonVPN.Vpn.Common;

namespace ProtonVPN.Service.KillSwitch
{
    public class KillSwitch : IVpnStateAware
    {
        private readonly IFirewall _firewall;
        private readonly IServiceSettings _serviceSettings;

        public KillSwitch(
            IFirewall firewall,
            IServiceSettings serviceSettings)
        {
            _firewall = firewall;
            _serviceSettings = serviceSettings;
        }

        public void OnVpnConnecting(VpnState state)
        {
            UpdateLeakProtectionStatus(state);
        }

        public void OnVpnConnected(VpnState state)
        {
        }

        public void OnVpnDisconnected(VpnState state)
        {
            UpdateLeakProtectionStatus(state);
        }

        public bool ExpectedLeakProtectionStatus(VpnState state)
        {
            return UpdatedLeakProtectionStatus(state) ?? _firewall.LeakProtectionEnabled;
        }

        private void UpdateLeakProtectionStatus(VpnState state)
        {
            switch (UpdatedLeakProtectionStatus(state))
            {
                case true:
                    if (_firewall.LeakProtectionEnabled)
                    {
                        _firewall.DisableLeakProtection();
                    }
                    var dnsLeakOnly = _serviceSettings.SplitTunnelSettings.Mode == SplitTunnelMode.Permit;
                    var firewallParams = new FirewallParams(state.RemoteIp, dnsLeakOnly);
                    _firewall.EnableLeakProtection(firewallParams);
                    break;
                case false:
                    _firewall.DisableLeakProtection();
                    break;
            }
        }

        private bool? UpdatedLeakProtectionStatus(VpnState state)
        {
            switch (state.Status)
            {
                case VpnStatus.Connecting:
                case VpnStatus.Reconnecting:
                    return true;
                case VpnStatus.Disconnecting:
                case VpnStatus.Disconnected:
                    if (state.Error == VpnError.None || !_serviceSettings.KillSwitchSettings.Enabled)
                    {
                        return false;
                    }

                    break;
            }

            return null;
        }
    }
}
