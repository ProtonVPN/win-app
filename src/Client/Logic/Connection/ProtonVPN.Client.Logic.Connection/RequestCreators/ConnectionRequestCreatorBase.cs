﻿/*
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

using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using ProtonVPN.Client.Logic.Connection.Contracts.RequestCreators;
using ProtonVPN.Client.Logic.Profiles.Contracts.Models;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Settings;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;

namespace ProtonVPN.Client.Logic.Connection.RequestCreators;

public abstract class ConnectionRequestCreatorBase : RequestCreatorBase
{
    protected ConnectionRequestCreatorBase(
        ILogger logger, 
        ISettings settings,
        IEntityMapper entityMapper,
        IMainSettingsRequestCreator mainSettingsRequestCreator)
        : base(logger, settings, entityMapper, mainSettingsRequestCreator)
    { }

    protected abstract Task<VpnCredentialsIpcEntity> GetVpnCredentialsAsync();

    protected virtual VpnConfigIpcEntity GetVpnConfig(MainSettingsIpcEntity settings, IConnectionIntent? connectionIntent = null)
    {
        bool isPortForwardingEnabled = settings.PortForwarding && (connectionIntent is null || connectionIntent.IsPortForwardingSupported());
        bool isCustomDnsEnabled = connectionIntent is IConnectionProfile profile && profile.Settings.IsCustomDnsServersEnabled.HasValue
            ? profile.Settings.IsCustomDnsServersEnabled.Value
            : Settings.IsCustomDnsServersEnabled;

        return new VpnConfigIpcEntity
        {
            VpnProtocol = settings.VpnProtocol,
            SplitTunnelMode = settings.SplitTunnel.Mode,
            SplitTunnelIPs = settings.SplitTunnel.Ips.ToList(),
            ModerateNat = settings.ModerateNat,
            NetShieldMode = settings.NetShieldMode,
            PortForwarding = isPortForwardingEnabled,
            SplitTcp = settings.SplitTcp,
            PreferredProtocols = settings.VpnProtocol == VpnProtocolIpcEntity.Smart
                ? GetPreferredSmartProtocols()
                : [settings.VpnProtocol],
            Ports = GetPorts(),
            CustomDns = isCustomDnsEnabled
                ? Settings.CustomDnsServersList.Where(s => s.IsActive).Select(s => s.IpAddress).ToList()
                : [],
        };
    }

    protected IList<VpnProtocolIpcEntity> GetPreferredSmartProtocols()
    {
        List<VpnProtocol> preferredProtocols = new();
        List<VpnProtocol> fallbackProtocols = new();

        SetProtocolBucket(VpnProtocol.WireGuardUdp, preferredProtocols, fallbackProtocols);
        SetProtocolBucket(VpnProtocol.WireGuardTcp, preferredProtocols, fallbackProtocols);
        SetProtocolBucket(VpnProtocol.WireGuardTls, preferredProtocols, fallbackProtocols);
        SetProtocolBucket(VpnProtocol.OpenVpnUdp, preferredProtocols, fallbackProtocols);
        SetProtocolBucket(VpnProtocol.OpenVpnTcp, preferredProtocols, fallbackProtocols);

        List<VpnProtocol> result = preferredProtocols.Count > 0 ? preferredProtocols : fallbackProtocols;
        return EntityMapper.Map<VpnProtocol, VpnProtocolIpcEntity>(result);
    }

    private void SetProtocolBucket(VpnProtocol protocol,
        List<VpnProtocol> preferredProtocols, List<VpnProtocol> fallbackProtocols)
    {
        if (Settings.DisabledSmartProtocols.Contains(protocol))
        {
            fallbackProtocols.Add(protocol);
        }
        else
        {
            preferredProtocols.Add(protocol);
        }
    }

    private Dictionary<VpnProtocolIpcEntity, int[]> GetPorts()
    {
        return new()
        {
            { VpnProtocolIpcEntity.WireGuardUdp, Settings.WireGuardUdpPorts },
            { VpnProtocolIpcEntity.WireGuardTcp, Settings.WireGuardTcpPorts },
            { VpnProtocolIpcEntity.WireGuardTls, Settings.WireGuardTlsPorts },
            { VpnProtocolIpcEntity.OpenVpnUdp, Settings.OpenVpnUdpPorts },
            { VpnProtocolIpcEntity.OpenVpnTcp, Settings.OpenVpnTcpPorts },
        };
    }

    protected bool IsToBypassSmartServerListGenerator(IConnectionIntent connectionIntent)
    {
        return !Settings.IsSmartReconnectEnabled ||
               connectionIntent is IConnectionProfile ||
               connectionIntent.Feature is B2BFeatureIntent ||
               (connectionIntent.Location is FreeServerLocationIntent fsli && fsli.Kind == ConnectionIntentKind.Random);
    }
}