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
            ?? _languages.Value.FirstOrDefault(l => l.Id.StartsWith(language, StringComparison.OrdinalIgnoreCase))
            ?? _languages.Value.FirstOrDefault();
    }

    private IList<Language> CreateLanguages()
    {
        IEnumerable<string> availableLanguages = _localizer.GetAvailableLanguages();
        return Languages.All
            .Where(l => availableLanguages.Contains(l.Id))
            .ToList();
    }
}
