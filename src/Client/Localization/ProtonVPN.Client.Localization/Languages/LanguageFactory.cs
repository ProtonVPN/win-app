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
            ?? new Language() { Id = language, Description = language };
    }

    private IList<Language> CreateLanguages()
    {
        return _localizer.GetAvailableLanguages().Select(CreateLanguage).ToList();
    }

    private Language CreateLanguage(string id)
    {
        return new Language() { Id = id, Description = CreateLanguageDescription(id) };
    }

    private static string CreateLanguageDescription(string id)
    {
        return id switch
        {
            "be-BY" => "беларуская - Belarusian",
            "cs-CZ" => "Čeština - Czech",
            "de-DE" => "Deutsch - German",
            "el-GR" => "ελληνικά - Greek",
            "en-US" => "English",
            "es-419" => "Español (América Latina) - Spanish",
            "es-ES" => "Español (España) - Spanish",
            "fa-IR" => "فارسی - Persian",
            "fi-FI" => "Suomi - Finnish",
            "fr-FR" => "Français - French",
            "hr-HR" => "Hrvatski - Croatian",
            "id-ID" => "Bahasa Indonesia - Indonesian",
            "it-IT" => "Italiano - Italian",
            "ja-JP" => "日本語 - Japanese",
            "ka-GE" => "ქართული - Georgian",
            "ko-KR" => "한국어 - Korean",
            "nb-NO" => "Norsk bokmål - Norwegian",
            "nl-NL" => "Nederlands - Dutch",
            "pl-PL" => "Polski - Polish",
            "pt-BR" => "Português (Brasil) - Portuguese",
            "pt-PT" => "Português (Portugal) - Portuguese",
            "ro-RO" => "Română - Romanian",
            "ru-RU" => "Русский - Russian",
            "sk-SK" => "Slovenčina - Slovak",
            "sl-SI" => "Slovenščina - Slovenian",
            "sv-SE" => "Svenska - Swedish",
            "tr-TR" => "Türkçe - Turkish",
            "uk-UA" => "Українська - Ukrainian",
            "zh-TW" => "繁體中文 - Chinese (Traditional)",
            _ => id,
        };
    }
}
