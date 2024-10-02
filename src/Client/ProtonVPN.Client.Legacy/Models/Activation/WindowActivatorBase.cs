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
using ProtonVPN.Client.Localization.Contracts.Messages;
using ProtonVPN.Client.Legacy.Messages;
using ProtonVPN.Client.Legacy.Models.Themes;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.Legacy.Models.Activation;

public abstract class WindowActivatorBase : IEventMessageReceiver<ThemeChangedMessage>, IEventMessageReceiver<LanguageChangedMessage>
{
    protected ILogger Logger { get; }

    protected IThemeSelector ThemeSelector { get; }

    protected WindowActivatorBase(ILogger logger, IThemeSelector themeSelector)
    {
        Logger = logger;
        ThemeSelector = themeSelector;
    }

    public void Receive(ThemeChangedMessage message)
    {
        ElementTheme theme = ThemeSelector.GetTheme().Theme;

        OnThemeChanged(theme);
    }

    public void Receive(LanguageChangedMessage message)
    {
        OnLanguageChanged(message.Language);
    }

    protected abstract void OnThemeChanged(ElementTheme theme);

    protected abstract void OnLanguageChanged(string language);
}