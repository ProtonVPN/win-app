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
using ProtonVPN.Client.Settings.Repositories.Contracts;
using ProtonVPN.Common.Core.Enums;
using ProtonVPN.Common.Core.Models;

namespace ProtonVPN.Client.Settings;

public class Settings : SettingsBase, ISettings
{
    public string Theme
    {
        get => GetReferenceType<string>(SettingScope.Global, SettingEncryption.Unencrypted) ?? DefaultSettings.Theme;
        set => SetReferenceType(value, SettingScope.Global, SettingEncryption.Unencrypted);
    }

    public string Language
    {
        get => GetReferenceType<string>(SettingScope.Global, SettingEncryption.Unencrypted) ?? DefaultSettings.Language;
        set => SetReferenceType(value, SettingScope.Global, SettingEncryption.Unencrypted);
    }

    public VpnProtocol VpnProtocol
    {
        get => GetValueType<VpnProtocol>(SettingScope.User, SettingEncryption.Unencrypted) ?? DefaultSettings.VpnProtocol;
        set => SetValueType<VpnProtocol>(value, SettingScope.User, SettingEncryption.Unencrypted);
    }

    public string? Username
    {
        get => GetReferenceType<string>(SettingScope.User, SettingEncryption.Encrypted);
        set => SetReferenceType(value, SettingScope.User, SettingEncryption.Encrypted);
    }

    public string? AccessToken
    {
        get => GetReferenceType<string>(SettingScope.User, SettingEncryption.Encrypted);
        set => SetReferenceType(value, SettingScope.User, SettingEncryption.Encrypted);
    }

    public string? RefreshToken
    {
        get => GetReferenceType<string>(SettingScope.User, SettingEncryption.Encrypted);
        set => SetReferenceType(value, SettingScope.User, SettingEncryption.Encrypted);
    }

    public string? UniqueSessionId
    {
        get => GetReferenceType<string>(SettingScope.User, SettingEncryption.Encrypted);
        set => SetReferenceType(value, SettingScope.User, SettingEncryption.Encrypted);
    }

    public string? VpnPlanTitle
    {
        get => GetReferenceType<string>(SettingScope.User, SettingEncryption.Unencrypted);
        set => SetReferenceType(value, SettingScope.User, SettingEncryption.Unencrypted);
    }

    public string? AuthenticationPublicKey
    {
        get => GetReferenceType<string>(SettingScope.User, SettingEncryption.Encrypted);
        set => SetReferenceType(value, SettingScope.User, SettingEncryption.Encrypted);
    }

    public string? AuthenticationSecretKey
    {
        get => GetReferenceType<string>(SettingScope.User, SettingEncryption.Encrypted);
        set => SetReferenceType(value, SettingScope.User, SettingEncryption.Encrypted);
    }

    public string? AuthenticationCertificatePem
    {
        get => GetReferenceType<string>(SettingScope.User, SettingEncryption.Encrypted);
        set => SetReferenceType(value, SettingScope.User, SettingEncryption.Encrypted);
    }

    public string? CertificationServerPublicKey
    {
        get => GetReferenceType<string>(SettingScope.User, SettingEncryption.Encrypted);
        set => SetReferenceType(value, SettingScope.User, SettingEncryption.Encrypted);
    }

    public DateTimeOffset? AuthenticationCertificateRequestUtcDate
    {
        get => GetValueType<DateTimeOffset>(SettingScope.User, SettingEncryption.Encrypted);
        set => SetValueType(value, SettingScope.User, SettingEncryption.Encrypted);
    }

    public DateTimeOffset? AuthenticationCertificateExpirationUtcDate
    {
        get => GetValueType<DateTimeOffset>(SettingScope.User, SettingEncryption.Encrypted);
        set => SetValueType(value, SettingScope.User, SettingEncryption.Encrypted);
    }

    public DateTimeOffset? AuthenticationCertificateRefreshUtcDate
    {
        get => GetValueType<DateTimeOffset>(SettingScope.User, SettingEncryption.Encrypted);
        set => SetValueType(value, SettingScope.User, SettingEncryption.Encrypted);
    }

    public NatType NatType
    {
        get => GetValueType<NatType>(SettingScope.User, SettingEncryption.Unencrypted) ?? DefaultSettings.NatType;
        set => SetValueType<NatType>(value, SettingScope.User, SettingEncryption.Unencrypted);
    }

    public bool IsVpnAcceleratorEnabled
    {
        get => GetValueType<bool>(SettingScope.User, SettingEncryption.Unencrypted) ?? DefaultSettings.IsVpnAcceleratorEnabled;
        set => SetValueType<bool>(value, SettingScope.User, SettingEncryption.Unencrypted);
    }

    public int? WindowWidth
    {
        get => GetValueType<int>(SettingScope.Global, SettingEncryption.Unencrypted);
        set => SetValueType<int>(value, SettingScope.Global, SettingEncryption.Unencrypted);
    }

    public int? WindowHeight
    {
        get => GetValueType<int>(SettingScope.Global, SettingEncryption.Unencrypted);
        set => SetValueType<int>(value, SettingScope.Global, SettingEncryption.Unencrypted);
    }

    public int? WindowXPosition
    {
        get => GetValueType<int>(SettingScope.Global, SettingEncryption.Unencrypted);
        set => SetValueType<int>(value, SettingScope.Global, SettingEncryption.Unencrypted);
    }

    public int? WindowYPosition
    {
        get => GetValueType<int>(SettingScope.Global, SettingEncryption.Unencrypted);
        set => SetValueType<int>(value, SettingScope.Global, SettingEncryption.Unencrypted);
    }

    public bool IsWindowMaximized
    {
        get => GetValueType<bool>(SettingScope.Global, SettingEncryption.Unencrypted) ?? DefaultSettings.IsWindowMaximized;
        set => SetValueType<bool>(value, SettingScope.Global, SettingEncryption.Unencrypted);
    }

    public bool IsNotificationEnabled
    {
        get => GetValueType<bool>(SettingScope.User, SettingEncryption.Unencrypted) ?? DefaultSettings.IsNotificationEnabled;
        set => SetValueType<bool>(value, SettingScope.User, SettingEncryption.Unencrypted);
    }

    public bool IsBetaAccessEnabled
    {
        get => GetValueType<bool>(SettingScope.User, SettingEncryption.Unencrypted) ?? DefaultSettings.IsBetaAccessEnabled;
        set => SetValueType<bool>(value, SettingScope.User, SettingEncryption.Unencrypted);
    }

    public bool IsHardwareAccelerationEnabled
    {
        get => GetValueType<bool>(SettingScope.User, SettingEncryption.Unencrypted) ?? DefaultSettings.IsHardwareAccelerationEnabled;
        set => SetValueType<bool>(value, SettingScope.User, SettingEncryption.Unencrypted);
    }

    public bool IsShareStatisticsEnabled
    {
        get => GetValueType<bool>(SettingScope.User, SettingEncryption.Unencrypted) ?? DefaultSettings.IsShareStatisticsEnabled;
        set => SetValueType<bool>(value, SettingScope.User, SettingEncryption.Unencrypted);
    }

    public bool IsShareCrashReportsEnabled
    {
        get => GetValueType<bool>(SettingScope.User, SettingEncryption.Unencrypted) ?? DefaultSettings.IsShareCrashReportsEnabled;
        set => SetValueType<bool>(value, SettingScope.User, SettingEncryption.Unencrypted);
    }

    public bool IsAlternativeRoutingEnabled
    {
        get => GetValueType<bool>(SettingScope.User, SettingEncryption.Unencrypted) ?? DefaultSettings.IsAlternativeRoutingEnabled;
        set => SetValueType<bool>(value, SettingScope.User, SettingEncryption.Unencrypted);
    }

    public bool IsCustomDnsServersEnabled
    {
        get => GetValueType<bool>(SettingScope.User, SettingEncryption.Unencrypted) ?? DefaultSettings.IsCustomDnsServersEnabled;
        set => SetValueType<bool>(value, SettingScope.User, SettingEncryption.Unencrypted);
    }

    public List<CustomDnsServer> CustomDnsServersList
    {
        get => GetListValueType<CustomDnsServer>(SettingScope.User, SettingEncryption.Unencrypted) ?? DefaultSettings.CustomDnsServersList;
        set => SetListValueType<CustomDnsServer>(value, SettingScope.User, SettingEncryption.Unencrypted);
    }

    public Settings(ISettingsRepository settingsRepository)
        : base(settingsRepository)
    { }
}