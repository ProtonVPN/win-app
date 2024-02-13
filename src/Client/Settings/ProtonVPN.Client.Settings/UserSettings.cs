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
using ProtonVPN.Client.Settings.Contracts.Models;
using ProtonVPN.Client.Settings.Repositories.Contracts;
using ProtonVPN.Common.Core.Networking;

namespace ProtonVPN.Client.Settings;

public class UserSettings : GlobalSettings, IUserSettings
{
    private readonly IUserSettingsRepository _userRepository;

    public string? Username
    {
        get => _userRepository.GetReferenceType<string>(SettingEncryption.Encrypted);
        set => _userRepository.SetReferenceType(value, SettingEncryption.Encrypted);
    }

    public string? UserDisplayName
    {
        get => _userRepository.GetReferenceType<string>(SettingEncryption.Encrypted);
        set => _userRepository.SetReferenceType(value, SettingEncryption.Encrypted);
    }

    public string Theme
    {
        get => _userRepository.GetReferenceType<string>(SettingEncryption.Unencrypted) ?? DefaultSettings.Theme;
        set => _userRepository.SetReferenceType(value, SettingEncryption.Unencrypted);
    }

    public int? WindowWidth
    {
        get => _userRepository.GetValueType<int>(SettingEncryption.Unencrypted);
        set => _userRepository.SetValueType<int>(value, SettingEncryption.Unencrypted);
    }

    public int? WindowHeight
    {
        get => _userRepository.GetValueType<int>(SettingEncryption.Unencrypted);
        set => _userRepository.SetValueType<int>(value, SettingEncryption.Unencrypted);
    }

    public int? WindowXPosition
    {
        get => _userRepository.GetValueType<int>(SettingEncryption.Unencrypted);
        set => _userRepository.SetValueType<int>(value, SettingEncryption.Unencrypted);
    }

    public int? WindowYPosition
    {
        get => _userRepository.GetValueType<int>(SettingEncryption.Unencrypted);
        set => _userRepository.SetValueType<int>(value, SettingEncryption.Unencrypted);
    }

    public bool IsWindowMaximized
    {
        get => _userRepository.GetValueType<bool>(SettingEncryption.Unencrypted) ?? DefaultSettings.IsWindowMaximized;
        set => _userRepository.SetValueType<bool>(value, SettingEncryption.Unencrypted);
    }

    public bool IsNavigationPaneOpened
    {
        get => _userRepository.GetValueType<bool>(SettingEncryption.Unencrypted) ?? DefaultSettings.IsNavigationPaneOpened;
        set => _userRepository.SetValueType<bool>(value, SettingEncryption.Unencrypted);
    }

    public bool IsConnectionDetailsPaneOpened
    {
        get => _userRepository.GetValueType<bool>(SettingEncryption.Unencrypted) ?? DefaultSettings.IsConnectionDetailsPaneOpened;
        set => _userRepository.SetValueType<bool>(value, SettingEncryption.Unencrypted);
    }

    public VpnProtocol VpnProtocol
    {
        get => _userRepository.GetValueType<VpnProtocol>(SettingEncryption.Unencrypted) ?? DefaultSettings.VpnProtocol;
        set => _userRepository.SetValueType<VpnProtocol>(value, SettingEncryption.Unencrypted);
    }

    public OpenVpnAdapter OpenVpnAdapter
    {
        get => _userRepository.GetValueType<OpenVpnAdapter>(SettingEncryption.Unencrypted) ?? DefaultSettings.OpenVpnAdapter;
        set => _userRepository.SetValueType<OpenVpnAdapter>(value, SettingEncryption.Unencrypted);
    }

    public string? VpnPlanTitle
    {
        get => _userRepository.GetReferenceType<string>(SettingEncryption.Encrypted);
        set => _userRepository.SetReferenceType(value, SettingEncryption.Encrypted);
    }

    public string? AuthenticationPublicKey
    {
        get => _userRepository.GetReferenceType<string>(SettingEncryption.Encrypted);
        set => _userRepository.SetReferenceType(value, SettingEncryption.Encrypted);
    }

    public string? AuthenticationSecretKey
    {
        get => _userRepository.GetReferenceType<string>(SettingEncryption.Encrypted);
        set => _userRepository.SetReferenceType(value, SettingEncryption.Encrypted);
    }

    public string? AuthenticationCertificatePem
    {
        get => _userRepository.GetReferenceType<string>(SettingEncryption.Encrypted);
        set => _userRepository.SetReferenceType(value, SettingEncryption.Encrypted);
    }

    public string? CertificationServerPublicKey
    {
        get => _userRepository.GetReferenceType<string>(SettingEncryption.Encrypted);
        set => _userRepository.SetReferenceType(value, SettingEncryption.Encrypted);
    }

    public DateTimeOffset? AuthenticationCertificateRequestUtcDate
    {
        get => _userRepository.GetValueType<DateTimeOffset>(SettingEncryption.Encrypted);
        set => _userRepository.SetValueType(value, SettingEncryption.Encrypted);
    }

    public DateTimeOffset? AuthenticationCertificateExpirationUtcDate
    {
        get => _userRepository.GetValueType<DateTimeOffset>(SettingEncryption.Encrypted);
        set => _userRepository.SetValueType(value, SettingEncryption.Encrypted);
    }

    public DateTimeOffset? AuthenticationCertificateRefreshUtcDate
    {
        get => _userRepository.GetValueType<DateTimeOffset>(SettingEncryption.Encrypted);
        set => _userRepository.SetValueType(value, SettingEncryption.Encrypted);
    }

    public NatType NatType
    {
        get => _userRepository.GetValueType<NatType>(SettingEncryption.Unencrypted) ?? DefaultSettings.NatType;
        set => _userRepository.SetValueType<NatType>(value, SettingEncryption.Unencrypted);
    }

    public bool IsPaid
    {
        get => _userRepository.GetValueType<bool>(SettingEncryption.Encrypted) ?? DefaultSettings.IsPaid;
        set => _userRepository.SetValueType<bool>(value, SettingEncryption.Encrypted);
    }

    public sbyte MaxTier
    {
        get => _userRepository.GetValueType<sbyte>(SettingEncryption.Encrypted) ?? DefaultSettings.MaxTier;
        set => _userRepository.SetValueType<sbyte>(value, SettingEncryption.Encrypted);
    }

    public bool IsVpnAcceleratorEnabled
    {
        get => _userRepository.GetValueType<bool>(SettingEncryption.Unencrypted) ?? DefaultSettings.IsVpnAcceleratorEnabled;
        set => _userRepository.SetValueType<bool>(value, SettingEncryption.Unencrypted);
    }

    // TODO: App Notifications have not been implemented yet.  -->
    public bool IsNotificationEnabled
    {
        get => false; // _userRepository.GetValueType<bool>(SettingEncryption.Unencrypted) ?? DefaultSettings.IsNotificationEnabled;
        set => _userRepository.SetValueType<bool>(value, SettingEncryption.Unencrypted);
    }

    public bool IsShareStatisticsEnabled
    {
        get => _userRepository.GetValueType<bool>(SettingEncryption.Unencrypted) ?? DefaultSettings.IsShareStatisticsEnabled;
        set => _userRepository.SetValueType<bool>(value, SettingEncryption.Unencrypted);
    }

    public bool IsShareCrashReportsEnabled
    {
        get => _userRepository.GetValueType<bool>(SettingEncryption.Unencrypted) ?? DefaultSettings.IsShareCrashReportsEnabled;
        set => _userRepository.SetValueType<bool>(value, SettingEncryption.Unencrypted);
    }

    public bool IsAlternativeRoutingEnabled
    {
        get => _userRepository.GetValueType<bool>(SettingEncryption.Unencrypted) ?? DefaultSettings.IsAlternativeRoutingEnabled;
        set => _userRepository.SetValueType<bool>(value, SettingEncryption.Unencrypted);
    }

    public bool IsIpv6LeakProtectionEnabled
    {
        get => _userRepository.GetValueType<bool>(SettingEncryption.Unencrypted) ?? DefaultSettings.IsIpv6LeakProtectionEnabled;
        set => _userRepository.SetValueType<bool>(value, SettingEncryption.Unencrypted);
    }

    public bool IsCustomDnsServersEnabled
    {
        get => _userRepository.GetValueType<bool>(SettingEncryption.Unencrypted) ?? DefaultSettings.IsCustomDnsServersEnabled;
        set => _userRepository.SetValueType<bool>(value, SettingEncryption.Unencrypted);
    }

    public List<CustomDnsServer> CustomDnsServersList
    {
        get => _userRepository.GetListValueType<CustomDnsServer>(SettingEncryption.Unencrypted) ?? DefaultSettings.CustomDnsServersList;
        set => _userRepository.SetListValueType<CustomDnsServer>(value, SettingEncryption.Unencrypted);
    }

    public bool IsAutoConnectEnabled
    {
        get => _userRepository.GetValueType<bool>(SettingEncryption.Unencrypted) ?? DefaultSettings.IsAutoConnectEnabled;
        set => _userRepository.SetValueType<bool>(value, SettingEncryption.Unencrypted);
    }

    public AutoConnectMode AutoConnectMode
    {
        get => _userRepository.GetValueType<AutoConnectMode>(SettingEncryption.Unencrypted) ?? DefaultSettings.AutoConnectMode;
        set => _userRepository.SetValueType<AutoConnectMode>(value, SettingEncryption.Unencrypted);
    }

    public bool IsNetShieldEnabled
    {
        get => _userRepository.GetValueType<bool>(SettingEncryption.Unencrypted) ?? DefaultSettings.IsNetShieldEnabled;
        set => _userRepository.SetValueType<bool>(value, SettingEncryption.Unencrypted);
    }

    public bool IsPortForwardingEnabled
    {
        get => _userRepository.GetValueType<bool>(SettingEncryption.Unencrypted) ?? DefaultSettings.IsPortForwardingEnabled;
        set => _userRepository.SetValueType<bool>(value, SettingEncryption.Unencrypted);
    }

    public bool IsPortForwardingNotificationEnabled
    {
        get => _userRepository.GetValueType<bool>(SettingEncryption.Unencrypted) ?? DefaultSettings.IsPortForwardingNotificationEnabled;
        set => _userRepository.SetValueType<bool>(value, SettingEncryption.Unencrypted);
    }

    public bool IsSplitTunnelingEnabled
    {
        get => _userRepository.GetValueType<bool>(SettingEncryption.Unencrypted) ?? DefaultSettings.IsSplitTunnelingEnabled;
        set => _userRepository.SetValueType<bool>(value, SettingEncryption.Unencrypted);
    }

    public bool IsSmartReconnectEnabled
    {
        get => _userRepository.GetValueType<bool>(SettingEncryption.Unencrypted) ?? DefaultSettings.IsSmartReconnectEnabled;
        set => _userRepository.SetValueType<bool>(value, SettingEncryption.Unencrypted);
    }

    public bool IsUserSettingsMigrationDone
    {
        get => _userRepository.GetValueType<bool>(SettingEncryption.Unencrypted) ?? DefaultSettings.IsUserSettingsMigrationDone;
        set => _userRepository.SetValueType<bool>(value, SettingEncryption.Unencrypted);
    }

    public SplitTunnelingMode SplitTunnelingMode
    {
        get => _userRepository.GetValueType<SplitTunnelingMode>(SettingEncryption.Unencrypted) ?? DefaultSettings.SplitTunnelingMode;
        set => _userRepository.SetValueType<SplitTunnelingMode>(value, SettingEncryption.Unencrypted);
    }

    public List<SplitTunnelingApp> SplitTunnelingStandardAppsList
    {
        get => _userRepository.GetListValueType<SplitTunnelingApp>(SettingEncryption.Unencrypted) ?? DefaultSettings.SplitTunnelingAppsList();
        set => _userRepository.SetListValueType<SplitTunnelingApp>(value, SettingEncryption.Unencrypted);
    }

    public List<SplitTunnelingApp> SplitTunnelingInverseAppsList
    {
        get => _userRepository.GetListValueType<SplitTunnelingApp>(SettingEncryption.Unencrypted) ?? DefaultSettings.SplitTunnelingAppsList();
        set => _userRepository.SetListValueType<SplitTunnelingApp>(value, SettingEncryption.Unencrypted);
    }

    public List<SplitTunnelingIpAddress> SplitTunnelingStandardIpAddressesList
    {
        get => _userRepository.GetListValueType<SplitTunnelingIpAddress>(SettingEncryption.Unencrypted) ?? DefaultSettings.SplitTunnelingIpAddressesList;
        set => _userRepository.SetListValueType<SplitTunnelingIpAddress>(value, SettingEncryption.Unencrypted);
    }

    public List<SplitTunnelingIpAddress> SplitTunnelingInverseIpAddressesList
    {
        get => _userRepository.GetListValueType<SplitTunnelingIpAddress>(SettingEncryption.Unencrypted) ?? DefaultSettings.SplitTunnelingIpAddressesList;
        set => _userRepository.SetListValueType<SplitTunnelingIpAddress>(value, SettingEncryption.Unencrypted);
    }

    public UserSettings(IGlobalSettingsRepository globalSettingsRepository, IUserSettingsRepository userSettingsRepository)
            : base(globalSettingsRepository)
    {
        _userRepository = userSettingsRepository;
    }
}