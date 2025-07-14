/*
 * Copyright (c) 2024 Proton AG
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
using System.Linq;
using ProtonVPN.Client.Localization.Building;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Common.Core.Extensions;
using WinUI3Localizer;

namespace ProtonVPN.Client.Localization.Languages;

public class LanguageFactory : ILanguageFactory
{
    private readonly ILocalizer _localizer;
    private readonly Lazy<IList<Language>> _languages;
    private readonly Dictionary<string, string> _languageNames = new()
    {
        { "en-US",  "English" },
        { "de-DE",  "Deutsch - German" },
        { "fr-FR",  "Français - French" },
        { "nl-NL",  "Nederlands - Dutch" },
        { "es-ES",  "Español (España) - Spanish (Spain)" },
        { "es-419",  "Español (Latinoamérica) - Spanish (Latin America)" },
        { "it-IT",  "Italiano - Italian" },
        { "pl-PL",  "Polski - Polish" },
        { "pt-BR",  "Português (Brasil) - Portuguese (Brazil)" },
        { "ru-RU",  "Русский - Russian" },
        { "tr-TR",  "Türkçe - Turkish" },
        { "ca-ES",  "Català - Catalan" },
        { "cs-CZ",  "Čeština - Czech" },
        { "fi-FI",  "Suomi - Finnish" },
        { "hu-HU",  "Magyar - Hungarian" },
        { "id-ID",  "Bahasa Indonesia - Indonesian" },
        { "nb-NO",  "Norsk (bokmål) - Norwegian (Bokmal)" },
        { "pt-PT",  "Português (Portugal) - Portuguese (Portugal)" },
        { "ro-RO",  "Română - Romanian" },
        { "sk-SK",  "Slovenčina - Slovak" },
        { "sl-SI",  "Slovenščina - Slovenian" },
        { "sv-SE",  "Svenska - Swedish" },
        { "el-GR",  "Ελληνικά - Greek" },
        { "be-BY",  "Беларуская - Belarusian" },
        { "uk-UA",  "Українська - Ukrainian" },
        { "ka-GE",  "Ქართული - Georgian" },
        { "ko-KR",  "한국어 - Korean" },
        { "ja-JP",  "日本語 - Japanese" },
        { "zh-CN",  "简体中文 - Chinese (Simplified)" },
        { "zh-TW",  "繁體中文 - Chinese (Traditional)" },
        { "fa-IR",  "فارسی - Persian" },
    };

    public LanguageFactory(ILocalizerFactory localizerFactory)
    {
        _localizer = localizerFactory.GetOrCreate();
        _languages = new Lazy<IList<Language>>(CreateLanguages);
    }

    public IEnumerable<Language> GetAvailableLanguages()
    {
        return _languages.Value;
    }

    public Language GetLanguage(string language)
    {
        return _languages.Value.FirstOrDefault(l => l.Id.EqualsIgnoringCase(language))
            ?? _languages.Value.FirstOrDefault(l => l.Id.StartsWith(language, StringComparison.OrdinalIgnoreCase))
            ?? _languages.Value.FirstOrDefault();
    }

    private IList<Language> CreateLanguages()
    {
        IEnumerable<string> availableLanguages = _localizer.GetAvailableLanguages();
        return _languageNames.Where(ln => availableLanguages.Contains(ln.Key)).Select(CreateLanguage).ToList();
    }

    private Language CreateLanguage(KeyValuePair<string, string> languageName)
    {
        return new Language() { Id = languageName.Key, Description = languageName.Value };
    }
}
