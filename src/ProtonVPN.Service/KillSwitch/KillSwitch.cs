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

using Autofac;
using ProtonVPN.Common;
using ProtonVPN.Common.KillSwitch;
using ProtonVPN.Common.Networking;
using ProtonVPN.Common.OS.Net;
using ProtonVPN.Common.OS.Net.NetworkInterface;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Service.Contract.Settings;
using ProtonVPN.Service.Firewall;
using ProtonVPN.Service.Settings;
using ProtonVPN.Service.Vpn;
using ProtonVPN.Vpn.Common;

namespace ProtonVPN.Service.KillSwitch
{
    public class KillSwitch : IVpnStateAware, IServiceSettingsAware, IStartable
    {
        private readonly IFirewall _firewall;
        private readonly IServiceSettings _serviceSettings;
        private readonly INetworkInterfaceLoader _networkInterfaceLoader;
        private VpnState _lastVpnState = new(VpnStatus.Disconnected, default);
        private KillSwitchMode _killSwitchMode;

        public KillSwitch(
            IFirewall firewall,
            IServiceSettings serviceSettings,
            INetworkInterfaceLoader networkInterfaceLoader)
        {
            _firewall = firewall;
            _serviceSettings = serviceSettings;
            _networkInterfaceLoader = networkInterfaceLoader;
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

            if (_lastVpnState.VpnProtocol == VpnProtocol.WireGuard)
            {
                UpdateLeakProtectionStatus(state);
            }
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
                HandleKillSwitchModeChange(settings.KillSwitchMode);
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

        private void HandleKillSwitchModeChange(KillSwitchMode killSwitchMode)
        {
            switch (killSwitchMode)
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

        private void EnableLeakProtection()
        {
            bool dnsLeakOnly = _serviceSettings.SplitTunnelSettings.Mode == SplitTunnelMode.Permit;
            bool persistent = _serviceSettings.KillSwitchMode == KillSwitchMode.Hard;
            INetworkInterface networkInterface = _networkInterfaceLoader.GetByVpnProtocol(_lastVpnState.VpnProtocol, _lastVpnState.OpenVpnAdapter);
            uint interfaceIndex = networkInterface?.Index ?? 0;
            FirewallParams firewallParams = new()
            {
                ServerIp = _lastVpnState.RemoteIp,
                DnsLeakOnly = dnsLeakOnly,
                InterfaceIndex = interfaceIndex,
                AddInterfaceFilters = interfaceIndex > 0,
                Persistent = persistent
            };
            _firewall.EnableLeakProtection(firewallParams);
        }

        private bool? UpdatedLeakProtectionStatus(VpnState state)
        {
            switch (state.Status)
            {
                case VpnStatus.Pinging:
                case VpnStatus.Connecting:
                case VpnStatus.Reconnecting:
                case VpnStatus.Connected when _lastVpnState.VpnProtocol == VpnProtocol.WireGuard:
                    return true;
                case VpnStatus.Disconnecting:
                case VpnStatus.Disconnected:
                    return state.Error switch
                    {
                        // Since PlanNeedsToBeUpgraded is received only when connected, we don't want to
                        // disable firewall while reconnecting, so keep the current firewall state.
                        VpnError.PlanNeedsToBeUpgraded => null,
                        VpnError.None => _serviceSettings.KillSwitchMode == KillSwitchMode.Hard,
                        _ => _serviceSettings.KillSwitchMode != KillSwitchMode.Off
                    };
            }

            return null;
        }
    }
}