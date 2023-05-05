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
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using ProtonVPN.Localization.Contracts;
using ProtonVPN.Localization.Messages;
using WinUI3Localizer;

namespace ProtonVPN.Localization.Services;

public class LocalizationService : ILocalizationService
{
    private readonly ILocalizer _localizer = Localizer.Get();

    public IEnumerable<string> GetAvailableLanguages()
    {
        return _localizer.GetAvailableLanguages();
    }

    public string GetCurrentLanguage()
    {
        return _localizer.GetCurrentLanguage();
    }

    public async Task SetLanguageAsync(string language)
    {
        await _localizer.SetLanguage(language);

        WeakReferenceMessenger.Default.Send(new LanguageChangedMessage(language));
    }
}