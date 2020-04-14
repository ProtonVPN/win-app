/*
 * Copyright (c) 2020 Proton Technologies AG
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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Logging;
using ProtonVPN.Core.Auth;
using ProtonVPN.Core.Settings;

namespace ProtonVPN.Core.Language
{
    public class Language : ISettingsAware, ILoggedInAware
    {
        private readonly IAppSettings _appSettings;
        private readonly ILogger _logger;
        private readonly string _translationsFolder;

        private const string ResourceFile = "ProtonVPN.resources.dll";
        private const string DefaultLanguage = "en";

        public event EventHandler<string> LanguageChanged;

        public Language(IAppSettings appSettings, ILogger logger, string translationsFolder)
        {
            _appSettings = appSettings;
            _logger = logger;
            _translationsFolder = translationsFolder;
        }

        public void Initialize()
        {
            Change(string.IsNullOrEmpty(_appSettings.Language) ? GetCurrentLanguage() : _appSettings.Language);
        }

        public void OnAppSettingsChanged(PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(nameof(IAppSettings.Language)))
                Change(_appSettings.Language);
        }

        public List<string> GetAll()
        {
            try
            {
                return InternalGetAll();
            }
            catch (Exception e) when (e.IsFileAccessException())
            {
                _logger.Error(e);
                return new List<string> {DefaultLanguage};
            }
        }

        public void OnUserLoggedIn()
        {
            if (string.IsNullOrEmpty(_appSettings.Language))
            {
                _appSettings.Language = GetCurrentLanguage();
            }
        }

        private List<string> InternalGetAll()
        {
            var langs = new List<string> {DefaultLanguage};

            var files = Directory.GetFiles(_translationsFolder, ResourceFile, SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var dirInfo = new DirectoryInfo(file);
                if (dirInfo.Parent != null)
                {
                    langs.Add(dirInfo.Parent.Name);
                }
            }

            return langs;
        }

        private void Change(string lang)
        {
            LanguageChanged?.Invoke(this, lang);
        }

        private string GetCurrentLanguage()
        {
            var osLanguage = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;
            return GetAll().Contains(osLanguage) ? osLanguage : DefaultLanguage;
        }
    }
}
