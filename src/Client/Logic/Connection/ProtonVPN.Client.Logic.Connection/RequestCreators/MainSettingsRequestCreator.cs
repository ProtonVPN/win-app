/*
 * Copyright (c) 2025 Proton AG
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

using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;
using ProtonVPN.Client.Logic.Connection.Contracts.RequestCreators;
using ProtonVPN.Client.Logic.Profiles.Contracts.Models;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Enums;
using ProtonVPN.Client.Settings.Contracts.Models;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Dns;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Settings;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;

namespace ProtonVPN.Client.Logic.Connection.RequestCreators;

public class MainSettingsRequestCreator : IMainSettingsRequestCreator
{
    private readonly ISettings _settings;
    private readonly IEntityMapper _entityMapper;

    public MainSettingsRequestCreator(
        ISettings settings,
        IEntityMapper entityMapper)
    {
        _settings = settings;
        _entityMapper = entityMapper;
    }

    public MainSettingsIpcEntity Create(IConnectionIntent? connectionIntent)
    {
        MainSettingsIpcEntity settings = Create();

        if (connectionIntent is IConnectionProfile connectionProfile)
        {
            settings.VpnProtocol = _entityMapper.Map<VpnProtocol, VpnProtocolIpcEntity>(connectionProfile.Settings.VpnProtocol);
            settings.NetShieldMode = connectionProfile.Settings.IsNetShieldEnabled ? (int)connectionProfile.Settings.NetShieldMode : 0;
            settings.PortForwarding = connectionProfile.Settings.IsPortForwardingEnabled;
            settings.ModerateNat = connectionProfile.Settings.NatType == NatType.Moderate;
        }

        if (settings.NetShieldMode == (int)NetShieldMode.BlockAdsMalwareTrackersAdultContent && 
            (_settings.VpnPlan.IsB2B || connectionIntent?.Feature is B2BFeatureIntent))
        {
            settings.NetShieldMode = (int)NetShieldMode.BlockAdsMalwareTrackers;
        }

        return settings;
    }

    private MainSettingsIpcEntity Create()
    {
        return new MainSettingsIpcEntity
        {
            VpnProtocol = _entityMapper.Map<VpnProtocol, VpnProtocolIpcEntity>(_settings.VpnProtocol),
            KillSwitchMode = GetKillSwitchMode(_settings.IsKillSwitchEnabled, _settings.KillSwitchMode),
            SplitTunnel = GetSplitTunnelingSettings(_settings.IsSplitTunnelingEnabled, _settings.SplitTunnelingMode),
            ModerateNat = _settings.NatType == NatType.Moderate,
            NetShieldMode = GetNetShieldMode(_settings.IsNetShieldEnabled, _settings.NetShieldMode),
            Ipv6LeakProtection = _settings.IsIpv6LeakProtectionEnabled,
            IsIpv6Enabled = _settings.IsIpv6Enabled,
            Ipv6Fragments = _settings.Ipv6Fragments,
            IsShareCrashReportsEnabled = _settings.IsShareCrashReportsEnabled,
            IsLocalAreaNetworkAccessEnabled = _settings.IsLocalAreaNetworkAccessEnabled,
            PortForwarding = _settings.IsPortForwardingEnabled,
            SplitTcp = _settings.IsVpnAcceleratorEnabled,
            OpenVpnAdapter = _entityMapper.Map<OpenVpnAdapter, OpenVpnAdapterIpcEntity>(_settings.OpenVpnAdapter),
            WireGuardConnectionTimeout = _settings.WireGuardConnectionTimeout,
            DnsBlockMode = GetDnsBlockMode(_settings.IsLocalAreaNetworkAccessEnabled, _settings.IsLocalDnsEnabled),
            ShouldDisableWeakHostSetting = DefaultSettings.ShouldDisableWeakHostSetting,
        };
    }

    public MainSettingsIpcEntity CreateForGuestHole()
    {
        // For guest hole settings, consider the user as a non-paid user.
        bool isPaidUser = false;

        return new MainSettingsIpcEntity
        {
            VpnProtocol = _entityMapper.Map<VpnProtocol, VpnProtocolIpcEntity>(DefaultSettings.VpnProtocol),
            KillSwitchMode = GetKillSwitchMode(_settings.IsKillSwitchEnabled, _settings.KillSwitchMode),
            SplitTunnel = GetSplitTunnelingSettings(DefaultSettings.IsSplitTunnelingEnabled, DefaultSettings.SplitTunnelingMode),
            ModerateNat = DefaultSettings.NatType == NatType.Moderate,
            NetShieldMode = GetNetShieldMode(DefaultSettings.IsNetShieldEnabled(isPaidUser), DefaultSettings.NetShieldMode),
            Ipv6LeakProtection = DefaultSettings.IsIpv6LeakProtectionEnabled,
            IsIpv6Enabled = DefaultSettings.IsIpv6Enabled,
            Ipv6Fragments = DefaultSettings.Ipv6Fragments,
            IsShareCrashReportsEnabled = _settings.IsShareCrashReportsEnabled,
            IsLocalAreaNetworkAccessEnabled = DefaultSettings.IsLocalAreaNetworkAccessAllowed(isPaidUser),
            PortForwarding = DefaultSettings.IsPortForwardingEnabled,
            SplitTcp = DefaultSettings.IsVpnAcceleratorEnabled,
            OpenVpnAdapter = OpenVpnAdapterIpcEntity.Tap,
            WireGuardConnectionTimeout = DefaultSettings.ProlongedWireGuardConnectionTimeout,
            DnsBlockMode = GetDnsBlockMode(DefaultSettings.IsLocalAreaNetworkAccessAllowed(isPaidUser), DefaultSettings.IsLocalDnsEnabled),
            ShouldDisableWeakHostSetting = DefaultSettings.ShouldDisableWeakHostSetting,
        };
    }

    private KillSwitchModeIpcEntity GetKillSwitchMode(bool isKillSwitchEnabled, KillSwitchMode killSwitchMode)
    {
        return isKillSwitchEnabled
            ? _entityMapper.Map<KillSwitchMode, KillSwitchModeIpcEntity>(killSwitchMode)
            : KillSwitchModeIpcEntity.Off;
    }

    private int GetNetShieldMode(bool isNetShieldEnabled, NetShieldMode netShieldMode)
    {
        return isNetShieldEnabled ? (int)netShieldMode : 0;
    }

    private DnsBlockModeIpcEntity GetDnsBlockMode(bool isLocalAreaNetworkAccessEnabled, bool isLocalDnsEnabled)
    {
        return isLocalAreaNetworkAccessEnabled && isLocalDnsEnabled
            ? DnsBlockModeIpcEntity.Callout
            : DnsBlockModeIpcEntity.Nrpt;
    }

    private SplitTunnelSettingsIpcEntity GetSplitTunnelingSettings(bool isSplitTunnelingEnabled, SplitTunnelingMode splitTunnelingMode)
    {
        return new SplitTunnelSettingsIpcEntity
        {
            Mode = GetSplitTunnelingMode(isSplitTunnelingEnabled, splitTunnelingMode),
            AppPaths = GetSplitTunnelingApps(isSplitTunnelingEnabled, splitTunnelingMode),
            Ips = GetSplitTunnelingIpAddresses(isSplitTunnelingEnabled, splitTunnelingMode)
        };
    }

    private SplitTunnelModeIpcEntity GetSplitTunnelingMode(bool isSplitTunnelingEnabled, SplitTunnelingMode splitTunnelingMode)
    {
        return isSplitTunnelingEnabled
            ? _entityMapper.Map<SplitTunnelingMode, SplitTunnelModeIpcEntity>(splitTunnelingMode)
            : SplitTunnelModeIpcEntity.Disabled;
    }

    private string[] GetSplitTunnelingApps(bool isSplitTunnelingEnabled, SplitTunnelingMode splitTunnelingMode)
    {
        if (!isSplitTunnelingEnabled)
        {
            return [];
        }

        return splitTunnelingMode == SplitTunnelingMode.Standard
            ? GetSplitTunnelingApps(_settings.SplitTunnelingStandardAppsList)
            : GetSplitTunnelingApps(_settings.SplitTunnelingInverseAppsList);
    }

    private string[] GetSplitTunnelingApps(List<SplitTunnelingApp> settingsApps)
    {
        return settingsApps.Where(app => app.IsActive).SelectMany(app => app.GetAllAppFilePaths()).ToArray();
    }

    private string[] GetSplitTunnelingIpAddresses(bool isSplitTunnelingEnabled, SplitTunnelingMode splitTunnelingMode)
    {
        if (!isSplitTunnelingEnabled)
        {
            return [];
        }

        return splitTunnelingMode == SplitTunnelingMode.Standard
            ? GetSplitTunnelingIpAddresses(_settings.SplitTunnelingStandardIpAddressesList)
            : GetSplitTunnelingIpAddresses(_settings.SplitTunnelingInverseIpAddressesList);
    }

    private string[] GetSplitTunnelingIpAddresses(List<SplitTunnelingIpAddress> settingsIpAddresses)
    {
        return settingsIpAddresses.Where(ip => ip.IsActive).Select(ip => ip.IpAddress).ToArray();
    }
}
