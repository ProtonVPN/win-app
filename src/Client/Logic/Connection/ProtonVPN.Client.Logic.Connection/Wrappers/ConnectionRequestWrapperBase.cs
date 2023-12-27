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
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Settings;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;

namespace ProtonVPN.Client.Logic.Connection.Wrappers;

public abstract class ConnectionRequestWrapperBase : RequestWrapperBase
{
    protected ConnectionRequestWrapperBase(
        ISettings settings,
        IEntityMapper entityMapper)
        : base(settings, entityMapper)
    { }

    protected abstract VpnCredentialsIpcEntity GetVpnCredentials();

    protected VpnConfigIpcEntity GetVpnConfig(MainSettingsIpcEntity settings)
    {
        return new VpnConfigIpcEntity
        {
            VpnProtocol = settings.VpnProtocol,
            SplitTunnelMode = settings.SplitTunnel.Mode,
            SplitTunnelIPs = settings.SplitTunnel.Ips.ToList(),
            ModerateNat = settings.ModerateNat,
            NetShieldMode = settings.NetShieldMode,
            PortForwarding = settings.PortForwarding,
            AllowNonStandardPorts = settings.AllowNonStandardPorts,
            SplitTcp = settings.SplitTcp,
            PreferredProtocols = settings.VpnProtocol == VpnProtocolIpcEntity.Smart
                ? [VpnProtocolIpcEntity.WireGuardUdp, VpnProtocolIpcEntity.OpenVpnUdp, VpnProtocolIpcEntity.OpenVpnTcp]
                : new List<VpnProtocolIpcEntity> { settings.VpnProtocol },
            Ports = new Dictionary<VpnProtocolIpcEntity, int[]>
            {
                { VpnProtocolIpcEntity.WireGuardUdp, Settings.WireGuardPorts },
                { VpnProtocolIpcEntity.OpenVpnUdp, Settings.OpenVpnUdpPorts },
                { VpnProtocolIpcEntity.OpenVpnTcp, Settings.OpenVpnTcpPorts },
            },
            CustomDns = Settings.IsCustomDnsServersEnabled
                ? Settings.CustomDnsServersList.Where(s => s.IsActive).Select(s => s.IpAddress).ToList()
                : [],
        };
    }
}