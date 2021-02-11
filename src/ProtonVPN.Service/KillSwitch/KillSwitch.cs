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

using Autofac;
using ProtonVPN.Common;
using ProtonVPN.Common.KillSwitch;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Service.Contract.Settings;
using ProtonVPN.Service.Firewall;
using ProtonVPN.Service.Network;
using ProtonVPN.Service.Settings;
using ProtonVPN.Service.Vpn;
using ProtonVPN.Vpn.Common;

namespace ProtonVPN.Service.KillSwitch
{
    internal class KillSwitch : IVpnStateAware, IServiceSettingsAware, IStartable
    {
        private readonly IFirewall _firewall;
        private readonly IServiceSettings _serviceSettings;
        private readonly ICurrentNetworkInterface _currentNetworkInterface;
        private VpnState _lastVpnState = new(VpnStatus.Disconnected);
        private KillSwitchMode _killSwitchMode;

        public KillSwitch(
            IFirewall firewall,
            IServiceSettings serviceSettings,
            ICurrentNetworkInterface currentNetworkInterface)
        {
            _firewall = firewall;
            _serviceSettings = serviceSettings;
            _currentNetworkInterface = currentNetworkInterface;
        }

        public void Start()
        {
            _killSwitchMode = _serviceSettings.KillSwitchMode;
        }

        public void OnVpnConnecting(VpnState state)
        {
            _lastVpnState = state;
            UpdateLeakProtectionStatus(state);
        }

        public void OnVpnConnected(VpnState state)
        {
            _lastVpnState = state;
        }

        public void OnVpnDisconnected(VpnState state)
        {
            _lastVpnState = state;
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
                    EnableLeakProtection();
                    break;
                case false:
                    _firewall.DisableLeakProtection();
                    break;
            }
        }

        public void OnServiceSettingsChanged(SettingsContract settings)
        {
            if (_killSwitchMode != settings.KillSwitchMode)
            {
                switch (settings.KillSwitchMode)
                {
                    case KillSwitchMode.Off when _lastVpnState.Status != VpnStatus.Connected:
                        _firewall.DisableLeakProtection();
                        break;
                    case KillSwitchMode.Off when _lastVpnState.Status == VpnStatus.Connected:
                    case KillSwitchMode.Soft when _lastVpnState.Status == VpnStatus.Connected:
                    case KillSwitchMode.Hard:
                        EnableLeakProtection();
                        break;
                    case KillSwitchMode.Soft:
                        if (_lastVpnState.Error != VpnError.NoneKeepEnabledKillSwitch)
                        {
                            _firewall.DisableLeakProtection();
                        }
                        break;
                }
            }
            else
            {
                if (settings.KillSwitchMode == KillSwitchMode.Hard && !_firewall.LeakProtectionEnabled)
                {
                    EnableLeakProtection();
                }
            }

            _killSwitchMode = settings.KillSwitchMode;
        }

        private void EnableLeakProtection()
        {
            bool dnsLeakOnly = _serviceSettings.SplitTunnelSettings.Mode == SplitTunnelMode.Permit;
            bool persistent = _serviceSettings.KillSwitchMode == KillSwitchMode.Hard;
            var firewallParams = new FirewallParams(_lastVpnState.RemoteIp, dnsLeakOnly, _currentNetworkInterface.Index,
                persistent);
            _firewall.EnableLeakProtection(firewallParams);
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
                    if (state.Error == VpnError.None)
                    {
                        return _serviceSettings.KillSwitchMode == KillSwitchMode.Hard;
                    }
                    else
                    {
                        return _serviceSettings.KillSwitchMode != KillSwitchMode.Off;
                    }
            }

            return null;
        }
    }
}