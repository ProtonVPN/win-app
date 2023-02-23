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

using System.Collections.Generic;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Core.Storage;

namespace ProtonVPN.Settings.Migrations.v2_0_6
{
    internal class AppSettingsMigration : BaseAppSettingsMigration
    {
        private const string LANGUAGE_KEY = "Language";
        private readonly List<KeyValuePair<string, string>> _languageCodeMap = new()
        {
            new("en", "en-US"),
            new("fa", "fa-IR"),
            new("fr", "fr-FR"),
            new("hr", "hr-HR"),
            new("id", "id-ID"),
            new("it", "it-IT"),
            new("nl", "nl-NL"),
            new("pl", "pl-PL"),
            new("ru", "ru-RU"),
            new("tr", "tr-TR"),
            new("uk", "uk-UA"),
        };

        public AppSettingsMigration(ISettingsStorage appSettings) : base(appSettings, "2.0.6")
        {
        }

        protected override void Migrate()
        {
            string language = Settings.Get<string>(LANGUAGE_KEY);
            if (!language.IsNullOrEmpty())
            {
                MigrateLanguage(language);
            }
        }

        private void MigrateLanguage(string language)
        {
            foreach (KeyValuePair<string, string> pair in _languageCodeMap)
            {
                if (language == pair.Key)
                {
                    Settings.Set(LANGUAGE_KEY, pair.Value);
                    break;
                }
            }
        }
    }
}