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
using Microsoft.UI.Xaml;
using ProtonVPN.Client.Common.Dispatching;
using ProtonVPN.Client.Common.UI.Controls.Custom;
using ProtonVPN.Client.Core.Extensions;
using ProtonVPN.Client.Core.Messages;
using ProtonVPN.Client.Core.Services.Selection;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts.Messages;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Logging.Contracts;
using WinUIEx;

namespace ProtonVPN.Client.Core.Services.Activation.Bases;

public abstract class WindowHostActivatorBase<TWindow> : ActivatorBase<TWindow>,
    IEventMessageReceiver<LanguageChangedMessage>,
    IEventMessageReceiver<ThemeChangedMessage>
    where TWindow : BaseWindow
{
    protected readonly IUIThreadDispatcher UIThreadDispatcher;
    protected readonly ISettings Settings;
    protected readonly IApplicationThemeSelector ThemeSelector;

    protected FlowDirection CurrentFlowDirection { get; private set; }

    protected ElementTheme CurrentAppTheme { get; private set; }

    protected bool IsWindowLoaded { get; private set; }

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

    protected override void RegisterToHostEvents()
    {
        base.RegisterToHostEvents();

        if (Host != null)
        {
            Host.Loaded += OnWindowLoaded;    
        }
    }

    protected override void UnregisterFromHostEvents()
    {
        base.UnregisterFromHostEvents();

        if (Host != null)
        {
            Host.Loaded -= OnWindowLoaded;
        }
    }
    protected override void OnInitialized()
    {
        base.OnInitialized();

        InvalidateFlowDirection();
        InvalidateAppTheme();
    }

    protected virtual void OnWindowLoaded()
    { }

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

    private void OnWindowLoaded(object? sender, EventArgs e)
    {
        IsWindowLoaded = true;

        OnWindowLoaded();
    }
}