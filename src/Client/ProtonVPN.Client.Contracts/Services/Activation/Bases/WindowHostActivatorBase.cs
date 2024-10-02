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
using ProtonVPN.Client.Common.Dispatching;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts.Messages;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Client.Contracts.Extensions;
using ProtonVPN.Client.Contracts.Services.Selection;
using ProtonVPN.Client.Contracts.Messages;
using WinUIEx;

namespace ProtonVPN.Client.Contracts.Services.Activation.Bases;

public abstract class WindowHostActivatorBase<TWindow> : ActivatorBase<TWindow>,
    IEventMessageReceiver<LanguageChangedMessage>,
    IEventMessageReceiver<ThemeChangedMessage>
    where TWindow : WindowEx
{
    protected readonly IUIThreadDispatcher UIThreadDispatcher;
    protected readonly ISettings Settings;
    protected readonly IApplicationThemeSelector ThemeSelector;
    protected FlowDirection CurrentFlowDirection { get; private set; }

    protected ElementTheme CurrentAppTheme { get; private set; }

    protected WindowHostActivatorBase(
        ILogger logger,
        IUIThreadDispatcher uiThreadDispatcher,
        IApplicationThemeSelector themeSelector,
        ISettings settings)
        : base(logger)
    {
        UIThreadDispatcher = uiThreadDispatcher;
        ThemeSelector = themeSelector;
        Settings = settings;
    }

    public void Receive(LanguageChangedMessage message)
    {
        UIThreadDispatcher.TryEnqueue(OnLanguageChanged);
    }

    public void Receive(ThemeChangedMessage message)
    {
        UIThreadDispatcher.TryEnqueue(InvalidateAppTheme);
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        InvalidateFlowDirection();
        InvalidateAppTheme();
    }

    protected virtual void OnLanguageChanged()
    {
        InvalidateFlowDirection();
    }

    protected virtual void OnFlowDirectionChanged()
    { }

    protected virtual void OnAppThemeChanged()
    { }

    private void InvalidateFlowDirection()
    {
        CurrentFlowDirection = Settings.Language.GetFlowDirection();

        OnFlowDirectionChanged();
    }

    private void InvalidateAppTheme()
    {
        CurrentAppTheme = ThemeSelector.GetTheme();

        OnAppThemeChanged();
    }
}