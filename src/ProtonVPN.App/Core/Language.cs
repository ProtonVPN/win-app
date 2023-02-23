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

using System.Globalization;
using System.Linq;
using System.Threading;
using ProtonVPN.Common.Cli;
using ProtonVPN.Core.Auth;
using ProtonVPN.Core.Settings;
using ProtonVPN.Translations;

namespace ProtonVPN.Core
{
    public class Language : ILoggedInAware
    {
        private readonly IAppSettings _appSettings;
        private readonly ILanguageProvider _languageProvider;
        private readonly string _defaultLocale;
        private string _startupLanguage;

        public Language(IAppSettings appSettings, ILanguageProvider languageProvider, string defaultLocale)
        {
            _defaultLocale = defaultLocale;
            _languageProvider = languageProvider;
            _appSettings = appSettings;
        }

        public void Initialize(string[] args)
        {
            string lang = GetCommandLineLanguage(args);
            if (_languageProvider.GetAll().Contains(lang))
            {
                _startupLanguage = lang;
            }
            else
            {
                lang = GetStartupLanguage();
            }

            TranslationSource.Instance.CurrentCulture = new CultureInfo(lang);
        }

        public void OnUserLoggedIn()
        {
            if (string.IsNullOrEmpty(_appSettings.Language))
            {
                _appSettings.Language = !string.IsNullOrEmpty(_startupLanguage)
                    ? _startupLanguage
                    : GetCurrentLanguage();
            }
        }

        private string GetCommandLineLanguage(string[] args)
        {
            CommandLineOption option = new("lang", args);
            return option.Params().FirstOrDefault();
        }

        private string GetStartupLanguage()
        {
            return string.IsNullOrEmpty(_appSettings.Language) ? GetCurrentLanguage() : _appSettings.Language;
        }

        private string GetCurrentLanguage()
        {
            string osLanguage = Thread.CurrentThread.CurrentUICulture.Name;
            return _languageProvider.GetAll().Contains(osLanguage) ? osLanguage : _defaultLocale;
        }
    }
}