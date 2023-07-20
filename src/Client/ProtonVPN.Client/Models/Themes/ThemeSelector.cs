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
using ProtonVPN.Client.Messages;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Messages;

namespace ProtonVPN.Client.Models.Themes;

public class ThemeSelector : IThemeSelector, IEventMessageReceiver<SettingChangedMessage>
{
    private readonly ISettings _settings;
    private readonly IEventMessageSender _eventMessageSender;
    private readonly IList<ApplicationElementTheme> _themes = new List<ApplicationElementTheme>()
    {
        new(ElementTheme.Light),
        new(ElementTheme.Dark),
        new(ElementTheme.Default)
    };

    public ThemeSelector(ISettings settings, IEventMessageSender eventMessageSender)
    {
        _settings = settings;
        _eventMessageSender = eventMessageSender;
    }

    public IList<ApplicationElementTheme> GetAvailableThemes()
    {
        return _themes;
    }

    public void Initialize()
    {
        ApplicationElementTheme currentTheme = GetTheme();

        // Force a new theme rendering, because if the theme being set already exists, it is going to be ignored and not fix the TitleBar bug.
        //SetWindowTheme(new(ElementTheme.Light));
        //SetWindowTheme(new(ElementTheme.Dark));

        SetWindowTheme(currentTheme);
    }

    public ApplicationElementTheme GetTheme()
    {
        return ConvertStringToApplicationElementTheme(_settings.Theme);
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

    private void SetWindowTheme(ApplicationElementTheme theme)
    {
        if (App.MainWindow.Content is FrameworkElement rootElement)
        {
            rootElement.RequestedTheme = theme.Theme;
        }
    }

    public void Receive(SettingChangedMessage message)
    {
        if (message.PropertyName == nameof(ISettings.Theme) && message.NewValue is not null)
        {
            string themeString = (string)message.NewValue;
            ApplicationElementTheme theme = ConvertStringToApplicationElementTheme(themeString);
            SetWindowTheme(theme);
            _eventMessageSender.Send(new ThemeChangedMessage(theme));
        }
    }

    public void SetTheme(ApplicationElementTheme theme)
    {
        _settings.Theme = theme.Theme.ToString();
    }
}