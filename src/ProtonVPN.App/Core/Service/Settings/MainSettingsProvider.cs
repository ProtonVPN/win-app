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

using System.Linq;
using ProtonVPN.Common;
using ProtonVPN.Common.KillSwitch;
using ProtonVPN.Common.Networking;
using ProtonVPN.Core.Settings;
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Settings;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;

namespace ProtonVPN.Core.Service.Settings
{
    public class MainSettingsProvider
    {
        private readonly IAppSettings _appSettings;
        private readonly IUserStorage _userStorage;
        private readonly IEntityMapper _entityMapper;

        public MainSettingsProvider(IAppSettings appSettings,
            IUserStorage userStorage,
            IEntityMapper entityMapper)
        {
            _appSettings = appSettings;
            _userStorage = userStorage;
            _entityMapper = entityMapper;
        }

        public MainSettingsIpcEntity Create(OpenVpnAdapter? openVpnAdapter = null)
        {
            bool isPaidFeatureAllowed = _userStorage.GetUser().Paid() || !_appSettings.FeatureFreeRescopeEnabled;
            return new()
            {
                KillSwitchMode = _entityMapper.Map<KillSwitchMode, KillSwitchModeIpcEntity>(_appSettings.KillSwitchMode),
                SplitTunnel = new SplitTunnelSettingsIpcEntity
                {
                    Mode = _appSettings.SplitTunnelingEnabled
                        ? _entityMapper.Map<SplitTunnelMode, SplitTunnelModeIpcEntity>(_appSettings.SplitTunnelMode)
                        : SplitTunnelModeIpcEntity.Disabled,
                    AppPaths = _appSettings.GetSplitTunnelApps(),
                    Ips = GetSplitTunnelIps()
                },
                ModerateNat = !_userStorage.GetUser().Empty() && _appSettings.ModerateNat && isPaidFeatureAllowed,
                NetShieldMode = _appSettings.IsNetShieldEnabled() ? _appSettings.NetShieldMode : 0,
                SplitTcp = isPaidFeatureAllowed ? _appSettings.IsVpnAcceleratorEnabled() : null,
                AllowNonStandardPorts = _appSettings.ShowNonStandardPortsToFreeUsers ? _appSettings.AllowNonStandardPorts : null,
                Ipv6LeakProtection = _appSettings.Ipv6LeakProtection,
                VpnProtocol = _entityMapper.Map<VpnProtocol, VpnProtocolIpcEntity>(_appSettings.GetProtocol()),
                OpenVpnAdapter = _entityMapper.MapNullableStruct<OpenVpnAdapter, OpenVpnAdapterIpcEntity>(openVpnAdapter) ??
                                 _entityMapper.Map<OpenVpnAdapter, OpenVpnAdapterIpcEntity>(_appSettings.NetworkAdapterType),
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
                    return new string[] { };
            }
        }
    }
}