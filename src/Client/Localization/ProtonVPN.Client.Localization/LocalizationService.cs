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
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Building;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Localization.Contracts.Messages;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Messages;
using WinUI3Localizer;

namespace ProtonVPN.Client.Localization;

public class LocalizationService : ILocalizationService, IEventMessageReceiver<SettingChangedMessage>
{
    private readonly IEventMessageSender _eventMessageSender;
    private readonly ILocalizer _localizer;

    public LocalizationService(IEventMessageSender eventMessageSender,
        ISettings settings, ILocalizerFactory localizerFactory)
    {
        _eventMessageSender = eventMessageSender;

        _localizer = localizerFactory.GetOrCreate();
        _localizer.SetLanguage(settings.Language);
    }

    private void SetLanguage(string language)
    {
        _localizer.SetLanguage(language);
        _eventMessageSender.Send(new LanguageChangedMessage(language));
    }

    public IEnumerable<string> GetAvailableLanguages()
    {
        return _localizer.GetAvailableLanguages();
    }

    public void Receive(SettingChangedMessage message)
    {
        if (message.PropertyName == nameof(ISettings.Language) && message.NewValue is not null)
        {
            string language = (string)message.NewValue;
            SetLanguage(language);
        }
    }
}
