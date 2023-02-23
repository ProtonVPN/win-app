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

using ProtonVPN.Common;
using ProtonVPN.Core.Settings;
using ProtonVPN.Service.Contract.Settings;
using System.Linq;
using ProtonVPN.Common.Networking;

namespace ProtonVPN.Core.Service.Settings
{
    public class SettingsContractProvider
    {
        private readonly IAppSettings _appSettings;
        private readonly IUserStorage _userStorage;

        public SettingsContractProvider(IAppSettings appSettings, IUserStorage userStorage)
        {
            _appSettings = appSettings;
            _userStorage = userStorage;
        }

        public SettingsContract GetSettingsContract(OpenVpnAdapter? openVpnAdapter = null)
        {
            return new()
            {
                KillSwitchMode = _appSettings.KillSwitchMode,
                SplitTunnel = new SplitTunnelSettingsContract
                {
                    Mode = _appSettings.SplitTunnelingEnabled ? _appSettings.SplitTunnelMode : SplitTunnelMode.Disabled,
                    AppPaths = _appSettings.GetSplitTunnelApps(),
                    Ips = GetSplitTunnelIps()
                },
                ModerateNat = !_userStorage.GetUser().Empty() && _appSettings.ModerateNat,
                NetShieldMode = _appSettings.IsNetShieldEnabled() ? _appSettings.NetShieldMode : 0,
                SplitTcp = _appSettings.IsVpnAcceleratorEnabled(),
                AllowNonStandardPorts = _appSettings.ShowNonStandardPortsToFreeUsers ? _appSettings.AllowNonStandardPorts : null,
                Ipv6LeakProtection = _appSettings.Ipv6LeakProtection,
                VpnProtocol = _appSettings.GetProtocol(),
                OpenVpnAdapter = openVpnAdapter ?? _appSettings.NetworkAdapterType,
                PortForwarding = _appSettings.IsPortForwardingEnabled()
            };
        }

        private string[] GetSplitTunnelIps()
        {
            switch (_appSettings.SplitTunnelMode)
            {
                case SplitTunnelMode.Permit:
                    return _appSettings.SplitTunnelIncludeIps.Where(i => i.Enabled).Select(i => i.Ip).ToArray();
                case SplitTunnelMode.Block:
                    return _appSettings.SplitTunnelExcludeIps.Where(i => i.Enabled).Select(i => i.Ip).ToArray();
                default:
                    return new string[] {};
            }
        }
    }
}