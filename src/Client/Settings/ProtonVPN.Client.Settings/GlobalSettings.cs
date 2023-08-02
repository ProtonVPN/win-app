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

using ProtonVPN.Client.Settings.Repositories.Contracts;
using ProtonVPN.Common.Core.Models;

namespace ProtonVPN.Client.Settings.Contracts;

public class GlobalSettings : IGlobalSettings
{
    private readonly IGlobalSettingsRepository _globalRepository;

    public GlobalSettings(IGlobalSettingsRepository globalSettingsRepository)
    {
        _globalRepository = globalSettingsRepository;
    }

    public string Theme
    {
        get => _globalRepository.GetReferenceType<string>(SettingEncryption.Unencrypted) ?? DefaultSettings.Theme;
        set => _globalRepository.SetReferenceType(value, SettingEncryption.Unencrypted);
    }

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

    public int? WindowWidth
    {
        get => _globalRepository.GetValueType<int>(SettingEncryption.Unencrypted);
        set => _globalRepository.SetValueType<int>(value, SettingEncryption.Unencrypted);
    }

    public int? WindowHeight
    {
        get => _globalRepository.GetValueType<int>(SettingEncryption.Unencrypted);
        set => _globalRepository.SetValueType<int>(value, SettingEncryption.Unencrypted);
    }

    public int? WindowXPosition
    {
        get => _globalRepository.GetValueType<int>(SettingEncryption.Unencrypted);
        set => _globalRepository.SetValueType<int>(value, SettingEncryption.Unencrypted);
    }

    public int? WindowYPosition
    {
        get => _globalRepository.GetValueType<int>(SettingEncryption.Unencrypted);
        set => _globalRepository.SetValueType<int>(value, SettingEncryption.Unencrypted);
    }

    public bool IsWindowMaximized
    {
        get => _globalRepository.GetValueType<bool>(SettingEncryption.Unencrypted) ?? DefaultSettings.IsWindowMaximized;
        set => _globalRepository.SetValueType<bool>(value, SettingEncryption.Unencrypted);
    }

    public bool IsHardwareAccelerationEnabled
    {
        get => _globalRepository.GetValueType<bool>(SettingEncryption.Unencrypted) ?? DefaultSettings.IsHardwareAccelerationEnabled;
        set => _globalRepository.SetValueType<bool>(value, SettingEncryption.Unencrypted);
    }

    public List<CustomDnsServer> CustomDnsServersList
    {
        get => _globalRepository.GetListValueType<CustomDnsServer>(SettingEncryption.Unencrypted) ?? DefaultSettings.CustomDnsServersList;
        set => _globalRepository.SetListValueType<CustomDnsServer>(value, SettingEncryption.Unencrypted);
    }
}