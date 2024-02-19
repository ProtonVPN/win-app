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

using System.Runtime.CompilerServices;

namespace ProtonVPN.Client.Settings.Repositories.Contracts;

public interface ISettingsCache
{
    T? GetValueType<T>(SettingEncryption encryption, [CallerMemberName] string propertyName = "")
        where T : struct;

    void SetValueType<T>(T? newValue, SettingEncryption encryption, [CallerMemberName] string propertyName = "")
        where T : struct;

    T? GetReferenceType<T>(SettingEncryption encryption, [CallerMemberName] string propertyName = "")
        where T : class;

    void SetReferenceType<T>(T? newValue, SettingEncryption encryption, [CallerMemberName] string propertyName = "")
        where T : class;

    // Null - The setting was never set up. Useful to be filled with a default value.
    // Empty - The setting was set up as an empty list. Useful to not be filled with a default value.
    List<T>? GetListValueType<T>(SettingEncryption encryption, [CallerMemberName] string propertyName = "")
        where T : struct;

    void SetListValueType<T>(List<T>? newValue, SettingEncryption encryption, [CallerMemberName] string propertyName = "")
        where T : struct;

    List<T>? GetListReferenceType<T>(SettingEncryption encryption, [CallerMemberName] string propertyName = "")
        where T : class;

    void SetListReferenceType<T>(List<T>? newValue, SettingEncryption encryption, [CallerMemberName] string propertyName = "")
        where T : class;
}