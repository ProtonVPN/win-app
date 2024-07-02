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

using ProtonVPN.Client.Logic.Connection.Contracts.RequestCreators;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Observers;
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Settings;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;

namespace ProtonVPN.Client.Logic.Connection.RequestCreators;

public abstract class ConnectionRequestCreatorBase : RequestCreatorBase
{
    private readonly IFeatureFlagsObserver _featureFlagsObserver;

    private readonly IReadOnlyList<VpnProtocolIpcEntity> _smartPreferredProtocols = [
        VpnProtocolIpcEntity.WireGuardUdp,
        VpnProtocolIpcEntity.WireGuardTcp,
        VpnProtocolIpcEntity.OpenVpnUdp,
        VpnProtocolIpcEntity.OpenVpnTcp,
        VpnProtocolIpcEntity.WireGuardTls,
    ];

    protected ConnectionRequestCreatorBase(
        ISettings settings,
        IEntityMapper entityMapper,
        IFeatureFlagsObserver featureFlagsObserver,
        IMainSettingsRequestCreator mainSettingsRequestCreator)
        : base(settings, entityMapper, mainSettingsRequestCreator)
    {
        _featureFlagsObserver = featureFlagsObserver;
    }

    protected abstract Task<VpnCredentialsIpcEntity> GetVpnCredentialsAsync();

    protected virtual VpnConfigIpcEntity GetVpnConfig(MainSettingsIpcEntity settings)
    {
        return new VpnConfigIpcEntity
        {
            VpnProtocol = settings.VpnProtocol,
            SplitTunnelMode = settings.SplitTunnel.Mode,
            SplitTunnelIPs = settings.SplitTunnel.Ips.ToList(),
            ModerateNat = settings.ModerateNat,
            NetShieldMode = settings.NetShieldMode,
            PortForwarding = settings.PortForwarding,
            SplitTcp = settings.SplitTcp,
            PreferredProtocols = settings.VpnProtocol == VpnProtocolIpcEntity.Smart
                ? GetPreferredSmartProtocols()
                : [settings.VpnProtocol],
            Ports = GetPorts(),
            CustomDns = Settings.IsCustomDnsServersEnabled
                ? Settings.CustomDnsServersList.Where(s => s.IsActive).Select(s => s.IpAddress).ToList()
                : [],
        };
    }

    protected IList<VpnProtocolIpcEntity> GetPreferredSmartProtocols()
    {
        List<VpnProtocolIpcEntity> preferredProtocols = _smartPreferredProtocols.ToList();
        if (!_featureFlagsObserver.IsStealthEnabled)
        {
            preferredProtocols.Remove(VpnProtocolIpcEntity.WireGuardTls);
        }

        return preferredProtocols;
    }

    private Dictionary<VpnProtocolIpcEntity, int[]> GetPorts()
    {
        Dictionary<VpnProtocolIpcEntity, int[]> ports = new()
        {
            { VpnProtocolIpcEntity.WireGuardUdp, Settings.WireGuardUdpPorts },
            { VpnProtocolIpcEntity.WireGuardTcp, Settings.WireGuardTcpPorts },
            { VpnProtocolIpcEntity.WireGuardTls, Settings.WireGuardTlsPorts },
            { VpnProtocolIpcEntity.OpenVpnUdp, Settings.OpenVpnUdpPorts },
            { VpnProtocolIpcEntity.OpenVpnTcp, Settings.OpenVpnTcpPorts },
        };

        if (!_featureFlagsObserver.IsStealthEnabled)
        {
            ports.Remove(VpnProtocolIpcEntity.WireGuardTls);
        }

        return ports;
    }
}