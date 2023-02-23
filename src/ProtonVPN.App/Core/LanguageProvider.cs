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

using System;
using System.Collections.Generic;
using System.IO;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.AppLogs;

namespace ProtonVPN.Core
{
    public class LanguageProvider : ILanguageProvider
    {
        private const string RESOURCE_FILE = "ProtonVPN.Translations.resources.dll";

        private readonly ILogger _logger;
        private readonly string _translationsFolder;
        private readonly string _defaultLocale;

        public LanguageProvider(ILogger logger, string translationsFolder, string defaultLocale)
        {
            _logger = logger;
            _defaultLocale = defaultLocale;
            _translationsFolder = translationsFolder;
        }

        public List<string> GetAll()
        {
            try
            {
                return InternalGetAll();
            }
            catch (Exception e) when (e.IsFileAccessException())
            {
                _logger.Error<AppFileAccessFailedLog>("Couldn't get language file.", e);
                return new() { _defaultLocale };
            }
        }

        private List<string> InternalGetAll()
        {
            List<string> languageCodes = new();
            string[] files = Directory.GetFiles(_translationsFolder, RESOURCE_FILE, SearchOption.AllDirectories);

            foreach (string file in files)
            {
                DirectoryInfo dirInfo = new(file);
                if (dirInfo.Parent != null)
                {
                    languageCodes.Add(dirInfo.Parent.Name);
                }
            }

            if (!languageCodes.Contains(_defaultLocale))
            {
                languageCodes.Add(_defaultLocale);
            }

            return languageCodes;
        }
    }
}