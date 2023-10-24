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

using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Enums;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Settings;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;

namespace ProtonVPN.Client.Logic.Connection.Wrappers;

public abstract class RequestWrapperBase
{
    protected readonly ISettings Settings;

    protected readonly IEntityMapper EntityMapper;

    protected RequestWrapperBase(
        ISettings settings,
        IEntityMapper entityMapper)
    {
        Settings = settings;
        EntityMapper = entityMapper;
    }

    protected MainSettingsIpcEntity GetSettings()
    {
        return new MainSettingsIpcEntity()
        {
            VpnProtocol = EntityMapper.Map<VpnProtocol, VpnProtocolIpcEntity>(Settings.VpnProtocol),
            KillSwitchMode = Settings.IsKillSwitchEnabled
                ? EntityMapper.Map<KillSwitchMode, KillSwitchModeIpcEntity>(Settings.KillSwitchMode)
                : KillSwitchModeIpcEntity.Off,
            SplitTunnel = new SplitTunnelSettingsIpcEntity
            {
                Mode = Settings.IsSplitTunnelingEnabled
                    ? EntityMapper.Map<SplitTunnelingMode, SplitTunnelModeIpcEntity>(Settings.SplitTunnelingMode)
                    : SplitTunnelModeIpcEntity.Disabled,
                AppPaths = Settings.SplitTunnelingAppsList.Where(app => app.IsActive).SelectMany(app => app.GetAllAppFilePaths()).ToArray(),
                Ips = Settings.SplitTunnelingIpAddressesList.Where(ip => ip.IsActive).Select(ip => ip.IpAddress).ToArray()
            },
            ModerateNat = Settings.NatType == NatType.Moderate,
            NetShieldMode = Settings.IsNetShieldEnabled ? 2 : 0,
            AllowNonStandardPorts = Settings.AllowNonStandardPorts,
            Ipv6LeakProtection = Settings.IsIpv6LeakProtectionEnabled,
            PortForwarding = Settings.IsPortForwardingEnabled,
            SplitTcp = Settings.IsVpnAcceleratorEnabled,
            OpenVpnAdapter = EntityMapper.Map<OpenVpnAdapter, OpenVpnAdapterIpcEntity>(Settings.OpenVpnAdapter),
        };
    }
}