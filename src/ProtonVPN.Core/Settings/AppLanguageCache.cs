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

using System.ComponentModel;
using System.Globalization;

namespace ProtonVPN.Core.Settings
{
    public class AppLanguageCache : IAppLanguageCache, ISettingsAware
    {
        private readonly IAppSettings _appSettings;
        private readonly string _operativeSystemLanguageIetfTag;
        private string _locale;

        public AppLanguageCache(IAppSettings appSettings)
        {
            _appSettings = appSettings;
            _operativeSystemLanguageIetfTag = GetOperativeSystemLanguageIetfTag();
        }

        private string GetOperativeSystemLanguageIetfTag()
        {
            return CultureInfo.CurrentUICulture?.IetfLanguageTag ?? CultureInfo.CurrentCulture?.IetfLanguageTag;
        }

        public string GetCurrentSelectedLanguageIetfTag()
        {
            return _locale ?? UpdateLocaleFromAppSettingsIfExists() ?? _operativeSystemLanguageIetfTag;
        }

        private string UpdateLocaleFromAppSettingsIfExists()
        {
            string appSettingsLanguage = _appSettings?.Language;
            if (string.IsNullOrEmpty(appSettingsLanguage))
            {
                return null;
            }
            _locale = appSettingsLanguage;
            return appSettingsLanguage;
        }

        public void OnAppSettingsChanged(PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(nameof(IAppSettings.Language)))
            {
                UpdateLocaleFromAppSettingsIfExists();
            }
        }
    }
}