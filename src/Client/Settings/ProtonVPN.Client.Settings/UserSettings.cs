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

using ProtonVPN.Client.Logic.Auth.Contracts.Models;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Enums;
using ProtonVPN.Client.Settings.Contracts.Models;
using ProtonVPN.Client.Settings.Repositories.Contracts;
using ProtonVPN.Common.Core.Networking;

namespace ProtonVPN.Client.Settings;

public class UserSettings : GlobalSettings, IUserSettings
{
    private readonly IUserSettingsCache _userCache;

    public string? Username
    {
        get => _userCache.GetReferenceType<string>(SettingEncryption.Encrypted);
        set => _userCache.SetReferenceType(value, SettingEncryption.Encrypted);
    }

    public string? UserDisplayName
    {
        get => _userCache.GetReferenceType<string>(SettingEncryption.Encrypted);
        set => _userCache.SetReferenceType(value, SettingEncryption.Encrypted);
    }

    public string Theme
    {
        get => _userCache.GetReferenceType<string>(SettingEncryption.Unencrypted) ?? DefaultSettings.Theme;
        set => _userCache.SetReferenceType(value, SettingEncryption.Unencrypted);
    }

    public int? WindowWidth
    {
        get => _userCache.GetValueType<int>(SettingEncryption.Unencrypted);
        set => _userCache.SetValueType<int>(value, SettingEncryption.Unencrypted);
    }

    public int? WindowHeight
    {
        get => _userCache.GetValueType<int>(SettingEncryption.Unencrypted);
        set => _userCache.SetValueType<int>(value, SettingEncryption.Unencrypted);
    }

    public int? WindowXPosition
    {
        get => _userCache.GetValueType<int>(SettingEncryption.Unencrypted);
        set => _userCache.SetValueType<int>(value, SettingEncryption.Unencrypted);
    }

    public int? WindowYPosition
    {
        get => _userCache.GetValueType<int>(SettingEncryption.Unencrypted);
        set => _userCache.SetValueType<int>(value, SettingEncryption.Unencrypted);
    }

    public bool IsWindowMaximized
    {
        get => _userCache.GetValueType<bool>(SettingEncryption.Unencrypted) ?? DefaultSettings.IsWindowMaximized;
        set => _userCache.SetValueType<bool>(value, SettingEncryption.Unencrypted);
    }

    public bool IsNavigationPaneOpened
    {
        get => _userCache.GetValueType<bool>(SettingEncryption.Unencrypted) ?? DefaultSettings.IsNavigationPaneOpened;
        set => _userCache.SetValueType<bool>(value, SettingEncryption.Unencrypted);
    }

    public bool IsConnectionDetailsPaneOpened
    {
        get => _userCache.GetValueType<bool>(SettingEncryption.Unencrypted) ?? DefaultSettings.IsConnectionDetailsPaneOpened;
        set => _userCache.SetValueType<bool>(value, SettingEncryption.Unencrypted);
    }

    public VpnProtocol VpnProtocol
    {
        get => _userCache.GetValueType<VpnProtocol>(SettingEncryption.Unencrypted) ?? DefaultSettings.VpnProtocol;
        set => _userCache.SetValueType<VpnProtocol>(value, SettingEncryption.Unencrypted);
    }

    public OpenVpnAdapter OpenVpnAdapter
    {
        get => _userCache.GetValueType<OpenVpnAdapter>(SettingEncryption.Unencrypted) ?? DefaultSettings.OpenVpnAdapter;
        set => _userCache.SetValueType<OpenVpnAdapter>(value, SettingEncryption.Unencrypted);
    }

    public string? VpnPlanTitle
    {
        get => _userCache.GetReferenceType<string>(SettingEncryption.Encrypted);
        set => _userCache.SetReferenceType(value, SettingEncryption.Encrypted);
    }

    public ConnectionAsymmetricKeyPair? ConnectionKeyPair
{
        get => _userCache.GetValueType<ConnectionAsymmetricKeyPair>(SettingEncryption.Encrypted);
        set => _userCache.SetValueType(value, SettingEncryption.Encrypted);
    }

    public ConnectionCertificate? ConnectionCertificate
    {
        get => _userCache.GetValueType<ConnectionCertificate>(SettingEncryption.Encrypted);
        set => _userCache.SetValueType(value, SettingEncryption.Encrypted);
    }

    public NatType NatType
    {
        get => _userCache.GetValueType<NatType>(SettingEncryption.Unencrypted) ?? DefaultSettings.NatType;
        set => _userCache.SetValueType<NatType>(value, SettingEncryption.Unencrypted);
    }

    public bool IsPaid
    {
        get => _userCache.GetValueType<bool>(SettingEncryption.Encrypted) ?? DefaultSettings.IsPaid;
        set => _userCache.SetValueType<bool>(value, SettingEncryption.Encrypted);
    }

    public sbyte MaxTier
    {
        get => _userCache.GetValueType<sbyte>(SettingEncryption.Encrypted) ?? DefaultSettings.MaxTier;
        set => _userCache.SetValueType<sbyte>(value, SettingEncryption.Encrypted);
    }

    public bool IsVpnAcceleratorEnabled
    {
        get => _userCache.GetValueType<bool>(SettingEncryption.Unencrypted) ?? DefaultSettings.IsVpnAcceleratorEnabled;
        set => _userCache.SetValueType<bool>(value, SettingEncryption.Unencrypted);
    }

    public bool IsNotificationEnabled
    {
        get => _userCache.GetValueType<bool>(SettingEncryption.Unencrypted) ?? DefaultSettings.IsNotificationEnabled;
        set => _userCache.SetValueType<bool>(value, SettingEncryption.Unencrypted);
    }

    public bool IsShareStatisticsEnabled
    {
        get => _userCache.GetValueType<bool>(SettingEncryption.Unencrypted) ?? DefaultSettings.IsShareStatisticsEnabled;
        set => _userCache.SetValueType<bool>(value, SettingEncryption.Unencrypted);
    }

    public bool IsShareCrashReportsEnabled
    {
        get => _userCache.GetValueType<bool>(SettingEncryption.Unencrypted) ?? DefaultSettings.IsShareCrashReportsEnabled;
        set => _userCache.SetValueType<bool>(value, SettingEncryption.Unencrypted);
    }

    public bool IsAlternativeRoutingEnabled
    {
        get => _userCache.GetValueType<bool>(SettingEncryption.Unencrypted) ?? DefaultSettings.IsAlternativeRoutingEnabled;
        set => _userCache.SetValueType<bool>(value, SettingEncryption.Unencrypted);
    }

    public bool IsIpv6LeakProtectionEnabled
    {
        get => _userCache.GetValueType<bool>(SettingEncryption.Unencrypted) ?? DefaultSettings.IsIpv6LeakProtectionEnabled;
        set => _userCache.SetValueType<bool>(value, SettingEncryption.Unencrypted);
    }

    public bool IsCustomDnsServersEnabled
    {
        get => _userCache.GetValueType<bool>(SettingEncryption.Unencrypted) ?? DefaultSettings.IsCustomDnsServersEnabled;
        set => _userCache.SetValueType<bool>(value, SettingEncryption.Unencrypted);
    }

    public List<CustomDnsServer> CustomDnsServersList
    {
        get => _userCache.GetListValueType<CustomDnsServer>(SettingEncryption.Unencrypted) ?? DefaultSettings.CustomDnsServersList;
        set => _userCache.SetListValueType<CustomDnsServer>(value, SettingEncryption.Unencrypted);
    }

    public bool IsAutoConnectEnabled
    {
        get => _userCache.GetValueType<bool>(SettingEncryption.Unencrypted) ?? DefaultSettings.IsAutoConnectEnabled;
        set => _userCache.SetValueType<bool>(value, SettingEncryption.Unencrypted);
    }

    public AutoConnectMode AutoConnectMode
    {
        get
        {
            if (IsPaid)
            {
                return _userCache.GetValueType<AutoConnectMode>(SettingEncryption.Unencrypted) ?? DefaultSettings.GetAutoConnectMode(true);
            }

            return DefaultSettings.GetAutoConnectMode(false);
        }
        set => _userCache.SetValueType<AutoConnectMode>(value, SettingEncryption.Unencrypted);
    }

    public bool IsNetShieldEnabled
    {
        get
        {
            if (IsPaid)
            {
                return _userCache.GetValueType<bool>(SettingEncryption.Unencrypted) ?? DefaultSettings.IsNetShieldEnabled(true);
            }

            return DefaultSettings.IsNetShieldEnabled(false);
        }
        set => _userCache.SetValueType<bool>(value, SettingEncryption.Unencrypted);
    }

    public bool IsPortForwardingEnabled
    {
        get => _userCache.GetValueType<bool>(SettingEncryption.Unencrypted) ?? DefaultSettings.IsPortForwardingEnabled;
        set => _userCache.SetValueType<bool>(value, SettingEncryption.Unencrypted);
    }

    public bool IsPortForwardingNotificationEnabled
    {
        get => _userCache.GetValueType<bool>(SettingEncryption.Unencrypted) ?? DefaultSettings.IsPortForwardingNotificationEnabled;
        set => _userCache.SetValueType<bool>(value, SettingEncryption.Unencrypted);
    }

    public bool IsSplitTunnelingEnabled
    {
        get => _userCache.GetValueType<bool>(SettingEncryption.Unencrypted) ?? DefaultSettings.IsSplitTunnelingEnabled;
        set => _userCache.SetValueType<bool>(value, SettingEncryption.Unencrypted);
    }

    public bool IsSmartReconnectEnabled
    {
        get => _userCache.GetValueType<bool>(SettingEncryption.Unencrypted) ?? DefaultSettings.IsSmartReconnectEnabled;
        set => _userCache.SetValueType<bool>(value, SettingEncryption.Unencrypted);
    }

    public bool IsUserSettingsMigrationDone
    {
        get => _userCache.GetValueType<bool>(SettingEncryption.Unencrypted) ?? DefaultSettings.IsUserSettingsMigrationDone;
        set => _userCache.SetValueType<bool>(value, SettingEncryption.Unencrypted);
    }

    public SplitTunnelingMode SplitTunnelingMode
    {
        get => _userCache.GetValueType<SplitTunnelingMode>(SettingEncryption.Unencrypted) ?? DefaultSettings.SplitTunnelingMode;
        set => _userCache.SetValueType<SplitTunnelingMode>(value, SettingEncryption.Unencrypted);
    }

    public List<SplitTunnelingApp> SplitTunnelingStandardAppsList
    {
        get => _userCache.GetListValueType<SplitTunnelingApp>(SettingEncryption.Unencrypted) ?? DefaultSettings.SplitTunnelingAppsList();
        set => _userCache.SetListValueType<SplitTunnelingApp>(value, SettingEncryption.Unencrypted);
    }

    public List<SplitTunnelingApp> SplitTunnelingInverseAppsList
    {
        get => _userCache.GetListValueType<SplitTunnelingApp>(SettingEncryption.Unencrypted) ?? DefaultSettings.SplitTunnelingAppsList();
        set => _userCache.SetListValueType<SplitTunnelingApp>(value, SettingEncryption.Unencrypted);
    }

    public List<SplitTunnelingIpAddress> SplitTunnelingStandardIpAddressesList
    {
        get => _userCache.GetListValueType<SplitTunnelingIpAddress>(SettingEncryption.Unencrypted) ?? DefaultSettings.SplitTunnelingIpAddressesList;
        set => _userCache.SetListValueType<SplitTunnelingIpAddress>(value, SettingEncryption.Unencrypted);
    }

    public List<SplitTunnelingIpAddress> SplitTunnelingInverseIpAddressesList
    {
        get => _userCache.GetListValueType<SplitTunnelingIpAddress>(SettingEncryption.Unencrypted) ?? DefaultSettings.SplitTunnelingIpAddressesList;
        set => _userCache.SetListValueType<SplitTunnelingIpAddress>(value, SettingEncryption.Unencrypted);
    }

    public ChangeServerAttempts ChangeServerAttempts
    {
        get => _userCache.GetValueType<ChangeServerAttempts>(SettingEncryption.Encrypted) ?? DefaultSettings.ChangeServerAttempts;
        set => _userCache.SetValueType<ChangeServerAttempts>(value, SettingEncryption.Encrypted);
    }

    public UserSettings(IGlobalSettingsCache globalSettingsCache, IUserSettingsCache userSettingsCache)
            : base(globalSettingsCache)
    {
        _userCache = userSettingsCache;
    }
}