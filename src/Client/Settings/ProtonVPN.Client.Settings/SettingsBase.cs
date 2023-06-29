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
using ProtonVPN.Client.Settings.Repositories.Contracts;

namespace ProtonVPN.Client.Settings
{
    public abstract class SettingsBase
    {
        private readonly ISettingsRepository _settingsRepository;

        protected SettingsBase(ISettingsRepository settingsRepository)
        {
            _settingsRepository = settingsRepository;
        }

        protected T? GetValueType<T>(SettingScope scope, SettingEncryption encryption,
            [CallerMemberName] string propertyName = "")
            where T : struct
        {
            return _settingsRepository.GetValueType<T>(propertyName, scope, encryption);
        }

        protected void SetValueType<T>(T? value, SettingScope scope, SettingEncryption encryption,
            [CallerMemberName] string propertyName = "")
            where T : struct
        {
            _settingsRepository.SetValueType<T>(propertyName, value, scope, encryption);
        }

        protected T? GetReferenceType<T>(SettingScope scope, SettingEncryption encryption,
            [CallerMemberName] string propertyName = "")
            where T : class
        {
            return _settingsRepository.GetReferenceType<T>(propertyName, scope, encryption);
        }

        protected void SetReferenceType<T>(T? value, SettingScope scope, SettingEncryption encryption,
            [CallerMemberName] string propertyName = "")
            where T : class
        {
            _settingsRepository.SetReferenceType<T>(propertyName, value, scope, encryption);
        }
    }
}