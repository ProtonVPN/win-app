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

using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents;
using ProtonVPN.Client.Logic.Connection.Contracts.RequestCreators;
using ProtonVPN.Client.Logic.Profiles.Contracts.Models;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Enums;
using ProtonVPN.Client.Settings.Contracts.Models;
using ProtonVPN.Client.Settings.Contracts.Observers;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Settings;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;

namespace ProtonVPN.Client.Logic.Connection.RequestCreators;

public class MainSettingsRequestCreator : IMainSettingsRequestCreator
{
    private readonly ISettings _settings;
    private readonly IEntityMapper _entityMapper;
    private readonly IFeatureFlagsObserver _featureFlagsObserver;
    private readonly ILogger _logger;

    public MainSettingsRequestCreator(
        ISettings settings, 
        IEntityMapper entityMapper, 
        IFeatureFlagsObserver featureFlagsObserver,
        ILogger logger)
    {
        _settings = settings;
        _entityMapper = entityMapper;
        _featureFlagsObserver = featureFlagsObserver;
        _logger = logger;
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

        if (!_featureFlagsObserver.IsStealthEnabled &&
            (settings.VpnProtocol is VpnProtocolIpcEntity.WireGuardTls or VpnProtocolIpcEntity.WireGuardTcp))
        {
            _logger.Warn<AppLog>("Stealth protocol is currently disabled. Switch to Smart protocol instead.");

            settings.VpnProtocol = _entityMapper.Map<VpnProtocol, VpnProtocolIpcEntity>(DefaultSettings.VpnProtocol);
        }

        return settings;
    }

    private MainSettingsIpcEntity Create()
    {
        return new MainSettingsIpcEntity
        {
            VpnProtocol = _entityMapper.Map<VpnProtocol, VpnProtocolIpcEntity>(_settings.VpnProtocol),
            KillSwitchMode = _settings.IsKillSwitchEnabled
                ? _entityMapper.Map<KillSwitchMode, KillSwitchModeIpcEntity>(_settings.KillSwitchMode)
                : KillSwitchModeIpcEntity.Off,
            SplitTunnel = new SplitTunnelSettingsIpcEntity
            {
                Mode = _settings.IsSplitTunnelingEnabled
                    ? _entityMapper.Map<SplitTunnelingMode, SplitTunnelModeIpcEntity>(_settings.SplitTunnelingMode)
                    : SplitTunnelModeIpcEntity.Disabled,
                AppPaths = GetSplitTunnelingApps(),
                Ips = GetSplitTunnelingIpAddresses()
            },
            ModerateNat = _settings.NatType == NatType.Moderate,
            NetShieldMode = _settings.IsNetShieldEnabled ? (int)_settings.NetShieldMode : 0,
            Ipv6LeakProtection = _settings.IsIpv6LeakProtectionEnabled,
            IsShareCrashReportsEnabled = _settings.IsShareCrashReportsEnabled,
            PortForwarding = _settings.IsPortForwardingEnabled,
            SplitTcp = _settings.IsVpnAcceleratorEnabled,
            OpenVpnAdapter = _entityMapper.Map<OpenVpnAdapter, OpenVpnAdapterIpcEntity>(_settings.OpenVpnAdapter),
        };
    }

    private string[] GetSplitTunnelingApps()
    {
        return _settings.SplitTunnelingMode == SplitTunnelingMode.Standard
            ? GetSplitTunnelingApps(_settings.SplitTunnelingStandardAppsList)
            : GetSplitTunnelingApps(_settings.SplitTunnelingInverseAppsList);
    }

    private string[] GetSplitTunnelingApps(List<SplitTunnelingApp> settingsApps)
    {
        return settingsApps.Where(app => app.IsActive).SelectMany(app => app.GetAllAppFilePaths()).ToArray();
    }

    private string[] GetSplitTunnelingIpAddresses()
    {
        return _settings.SplitTunnelingMode == SplitTunnelingMode.Standard
            ? GetSplitTunnelingIpAddresses(_settings.SplitTunnelingStandardIpAddressesList)
            : GetSplitTunnelingIpAddresses(_settings.SplitTunnelingInverseIpAddressesList);
    }

    private string[] GetSplitTunnelingIpAddresses(List<SplitTunnelingIpAddress> settingsIpAddresses)
    {
        return settingsIpAddresses.Where(ip => ip.IsActive).Select(ip => ip.IpAddress).ToArray();
    }
}