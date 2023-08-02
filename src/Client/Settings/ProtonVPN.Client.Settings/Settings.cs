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

namespace ProtonVPN.Client.Settings;

public class Settings : GlobalSettings, ISettings
{
    private readonly IUserSettingsRepository _userRepository;

    public Settings(IGlobalSettingsRepository globalSettingsRepository, IUserSettingsRepository userSettingsRepository)
        : base(globalSettingsRepository)
    {
        _userRepository = userSettingsRepository;
    }

    public VpnProtocol VpnProtocol
    {
        get => _userRepository.GetValueType<VpnProtocol>(SettingEncryption.Unencrypted) ?? DefaultSettings.VpnProtocol;
        set => _userRepository.SetValueType<VpnProtocol>(value, SettingEncryption.Unencrypted);
    }

    public string? AccessToken
    {
        get => _userRepository.GetReferenceType<string>(SettingEncryption.Encrypted);
        set => _userRepository.SetReferenceType(value, SettingEncryption.Encrypted);
    }

    public string? RefreshToken
    {
        get => _userRepository.GetReferenceType<string>(SettingEncryption.Encrypted);
        set => _userRepository.SetReferenceType(value, SettingEncryption.Encrypted);
    }

    public string? UniqueSessionId
    {
        get => _userRepository.GetReferenceType<string>(SettingEncryption.Encrypted);
        set => _userRepository.SetReferenceType(value, SettingEncryption.Encrypted);
    }

    public string? VpnPlanTitle
    {
        get => _userRepository.GetReferenceType<string>(SettingEncryption.Unencrypted);
        set => _userRepository.SetReferenceType(value, SettingEncryption.Unencrypted);
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

    public bool IsVpnAcceleratorEnabled
    {
        get => _userRepository.GetValueType<bool>(SettingEncryption.Unencrypted) ?? DefaultSettings.IsVpnAcceleratorEnabled;
        set => _userRepository.SetValueType<bool>(value, SettingEncryption.Unencrypted);
    }

    public bool IsNotificationEnabled
    {
        get => _userRepository.GetValueType<bool>(SettingEncryption.Unencrypted) ?? DefaultSettings.IsNotificationEnabled;
        set => _userRepository.SetValueType<bool>(value, SettingEncryption.Unencrypted);
    }

    public bool IsBetaAccessEnabled
    {
        get => _userRepository.GetValueType<bool>(SettingEncryption.Unencrypted) ?? DefaultSettings.IsBetaAccessEnabled;
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

    public bool IsCustomDnsServersEnabled
    {
        get => _userRepository.GetValueType<bool>(SettingEncryption.Unencrypted) ?? DefaultSettings.IsCustomDnsServersEnabled;
        set => _userRepository.SetValueType<bool>(value, SettingEncryption.Unencrypted);
    }
}