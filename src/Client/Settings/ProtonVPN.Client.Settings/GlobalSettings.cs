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

using System.Collections.Concurrent;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Enums;
using ProtonVPN.Client.Settings.Contracts.Models;
using ProtonVPN.Client.Settings.Repositories.Contracts;
using ProtonVPN.Dns.Contracts;

namespace ProtonVPN.Client.Settings;

public class GlobalSettings : IGlobalSettings
{
    private readonly IGlobalSettingsRepository _globalRepository;

    public string Language
    {
        get => _globalRepository.GetReferenceType<string>(SettingEncryption.Unencrypted) ?? DefaultSettings.Language;
        set => _globalRepository.SetReferenceType(value, SettingEncryption.Unencrypted);
    }

    public string? Username
    {
        get => _globalRepository.GetReferenceType<string>(SettingEncryption.Encrypted);
        set => _globalRepository.SetReferenceType(value, SettingEncryption.Encrypted);
    }

    public string? AccessToken
    {
        get => _globalRepository.GetReferenceType<string>(SettingEncryption.Encrypted);
        set => _globalRepository.SetReferenceType(value, SettingEncryption.Encrypted);
    }

    public string? RefreshToken
    {
        get => _globalRepository.GetReferenceType<string>(SettingEncryption.Encrypted);
        set => _globalRepository.SetReferenceType(value, SettingEncryption.Encrypted);
    }

    public string? UniqueSessionId
    {
        get => _globalRepository.GetReferenceType<string>(SettingEncryption.Encrypted);
        set => _globalRepository.SetReferenceType(value, SettingEncryption.Encrypted);
    }

    public string? UnauthAccessToken
    {
        get => _globalRepository.GetReferenceType<string>(SettingEncryption.Encrypted);
        set => _globalRepository.SetReferenceType(value, SettingEncryption.Encrypted);
    }

    public string? UnauthRefreshToken
    {
        get => _globalRepository.GetReferenceType<string>(SettingEncryption.Encrypted);
        set => _globalRepository.SetReferenceType(value, SettingEncryption.Encrypted);
    }

    public string? UnauthUniqueSessionId
    {
        get => _globalRepository.GetReferenceType<string>(SettingEncryption.Encrypted);
        set => _globalRepository.SetReferenceType(value, SettingEncryption.Encrypted);
    }

    public bool IsAutoLaunchEnabled
    {
        get => _globalRepository.GetValueType<bool>(SettingEncryption.Unencrypted) ?? DefaultSettings.IsAutoLaunchEnabled;
        set => _globalRepository.SetValueType<bool>(value, SettingEncryption.Unencrypted);
    }

    public AutoLaunchMode AutoLaunchMode
    {
        get => _globalRepository.GetValueType<AutoLaunchMode>(SettingEncryption.Unencrypted) ?? DefaultSettings.AutoLaunchMode;
        set => _globalRepository.SetValueType<AutoLaunchMode>(value, SettingEncryption.Unencrypted);
    }

    public int[] WireGuardPorts
    {
        get => _globalRepository.GetReferenceType<int[]>(SettingEncryption.Unencrypted) ?? DefaultSettings.WireGuardPorts;
        set => _globalRepository.SetReferenceType(value, SettingEncryption.Unencrypted);
    }

    public int[] OpenVpnTcpPorts
    {
        get => _globalRepository.GetReferenceType<int[]>(SettingEncryption.Unencrypted) ?? DefaultSettings.OpenVpnTcpPorts;
        set => _globalRepository.SetReferenceType(value, SettingEncryption.Unencrypted);
    }

    public int[] OpenVpnUdpPorts
    {
        get => _globalRepository.GetReferenceType<int[]>(SettingEncryption.Unencrypted) ?? DefaultSettings.OpenVpnUdpPorts;
        set => _globalRepository.SetReferenceType(value, SettingEncryption.Unencrypted);
    }

    public ConcurrentDictionary<string, DnsResponse>? DnsCache
    {
        get => _globalRepository.GetReferenceType<ConcurrentDictionary<string, DnsResponse>>(SettingEncryption.Unencrypted);
        set => _globalRepository.SetReferenceType(value, SettingEncryption.Unencrypted);
    }

    public bool IsDoHEnabled
    {
        get => _globalRepository.GetValueType<bool>(SettingEncryption.Unencrypted) ?? DefaultSettings.IsDoHEnabled;
        set => _globalRepository.SetValueType<bool>(value, SettingEncryption.Unencrypted);
    }

    public bool IsKillSwitchEnabled
    {
        get => _globalRepository.GetValueType<bool>(SettingEncryption.Unencrypted) ?? DefaultSettings.IsKillSwitchEnabled;
        set => _globalRepository.SetValueType<bool>(value, SettingEncryption.Unencrypted);
    }

    public KillSwitchMode KillSwitchMode
    {
        get => _globalRepository.GetValueType<KillSwitchMode>(SettingEncryption.Unencrypted) ?? DefaultSettings.KillSwitchMode;
        set => _globalRepository.SetValueType<KillSwitchMode>(value, SettingEncryption.Unencrypted);
    }

    public List<FeatureFlag> FeatureFlags 
    {
        get => _globalRepository.GetListValueType<FeatureFlag>(SettingEncryption.Encrypted) ?? DefaultSettings.FeatureFlags;
        set => _globalRepository.SetListValueType<FeatureFlag>(value, SettingEncryption.Encrypted);
    }

    public GlobalSettings(IGlobalSettingsRepository globalSettingsRepository)
    {
        _globalRepository = globalSettingsRepository;
    }
}