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

using Microsoft.UI.Xaml;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Messages;
using ProtonVPN.Client.Contracts.Messages;

namespace ProtonVPN.Client.Models.Themes;

[Obsolete("Use ProtonVPN.Client.Services.Selection.ApplicationThemeSelector instead")]
public class ThemeSelector : IThemeSelector, IEventMessageReceiver<SettingChangedMessage>
{
    private readonly ISettings _settings;
    private readonly IEventMessageSender _eventMessageSender;
    private readonly ILocalizationProvider _localizer;

    private readonly IList<ApplicationElementTheme> _themes;

    public ThemeSelector(ISettings settings, IEventMessageSender eventMessageSender,
        ILocalizationProvider localizer)
    {
        _settings = settings;
        _eventMessageSender = eventMessageSender;
        _localizer = localizer;

        _themes =
        [
            new(ElementTheme.Light, _localizer),
            new(ElementTheme.Dark, _localizer),
            new(ElementTheme.Default, _localizer)
        ];
    }

    public IList<ApplicationElementTheme> GetAvailableThemes()
    {
        return _themes;
    }

    public ApplicationElementTheme GetTheme()
    {
        return ConvertStringToApplicationElementTheme(_settings.Theme);
    }

    public void Receive(SettingChangedMessage message)
    {
        if (message.PropertyName == nameof(ISettings.Theme) && message.NewValue is not null)
        {
            string themeString = (string)message.NewValue;
            ApplicationElementTheme theme = ConvertStringToApplicationElementTheme(themeString);

            _eventMessageSender.Send(new ThemeChangedMessage(theme.Theme));
        }
    }

    public void SetTheme(ApplicationElementTheme theme)
    {
        if (theme is not null)
        {
            _settings.Theme = theme.Theme.ToString();
        }
    }

    private ApplicationElementTheme ConvertStringToApplicationElementTheme(string? theme)
    {
        return _themes.FirstOrDefault(t => t.Theme == MapElementTheme(theme)) ?? _themes.Last();
    }

    private ElementTheme MapElementTheme(string? theme)
    {
        if (Enum.TryParse(theme, out ElementTheme cacheTheme))
        {
            return cacheTheme;
        }

        return ElementTheme.Default;
    }
}