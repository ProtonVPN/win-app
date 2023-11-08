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
using Microsoft.Extensions.Localization;
using ProtonVPN.Client.Localization.Contracts;

namespace ProtonVPN.Client.Localization;

public class StringLocalizer : IStringLocalizer
{
    private readonly ILocalizationProvider _localizationProvider;

    public StringLocalizer(ILocalizationProvider localizationProvider)
    {
        _localizationProvider = localizationProvider;
    }

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
    {
        throw new NotImplementedException();
    }

    public LocalizedString this[string name] => new(name, _localizationProvider.Get(name));

    public LocalizedString this[string name, params object[] arguments] => new(name, _localizationProvider.GetFormat(name, arguments));
}