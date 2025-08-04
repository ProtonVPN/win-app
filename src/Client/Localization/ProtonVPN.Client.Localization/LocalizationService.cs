/*
 * Copyright (c) 2025 Proton AG
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
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Building;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Localization.Contracts.Messages;
using ProtonVPN.Client.Localization.Languages;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Messages;
using WinUI3Localizer;

namespace ProtonVPN.Client.Localization;

public class LocalizationService : ILocalizationService, IEventMessageReceiver<SettingChangedMessage>
{
    private readonly IEventMessageSender _eventMessageSender;
    private readonly ISettings _settings;
    private readonly ILanguageFactory _languageFactory;
    private readonly ILocalizer _localizer;
    private readonly ILocalizationProvider _localizationProvider;

    public LocalizationService(IEventMessageSender eventMessageSender,
        ISettings settings,
        ILanguageFactory languageFactory,
        ILocalizerFactory localizerFactory,
        ILocalizationProvider localizationProvider)
    {
        _eventMessageSender = eventMessageSender;
        _settings = settings;
        _languageFactory = languageFactory;
        _localizer = localizerFactory.GetOrCreate();
        _localizer.SetLanguage(settings.Language);
        _localizationProvider = localizationProvider;
    }

    private void SetLanguage(string language)
    {
        _localizer.SetLanguage(language);
        _localizationProvider.ForceCurrentLanguageForPluralProvider();
        _eventMessageSender.Send(new LanguageChangedMessage(language));
    }

    public IEnumerable<Language> GetAvailableLanguages()
    {
        return _languageFactory.GetAvailableLanguages();
    }

    public Language GetLanguage(string language)
    {
        return _languageFactory.GetLanguage(language);
    }

    public void Receive(SettingChangedMessage message)
    {
        if (message.PropertyName == nameof(ISettings.Language))
        {
            SetLanguage(_settings.Language);
        }
    }
}