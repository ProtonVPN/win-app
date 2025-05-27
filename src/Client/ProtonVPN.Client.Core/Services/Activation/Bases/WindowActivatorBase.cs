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
using ProtonVPN.Client.Contracts.Services.Activation.Bases;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Extensions;
using ProtonVPN.Client.Core.Services.Selection;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;
using WinUIEx;

namespace ProtonVPN.Client.Core.Services.Activation.Bases;

public abstract class WindowActivatorBase<TWindow> : WindowHostActivatorBase<TWindow>, IWindowActivator
    where TWindow : WindowEx
{
    protected readonly ILocalizationProvider Localizer;
    protected readonly IApplicationIconSelector IconSelector;

    private uint _currentHostDpi;

    public abstract string WindowTitle { get; }

    protected bool HandleClosedEvent { get; private set; }

    protected WindowState CurrentWindowState { get; private set; }

    protected WindowActivationState CurrentActivationState { get; private set; }

    public bool IsWindowFocused => CurrentActivationState != WindowActivationState.Deactivated;

    public event EventHandler? HostDpiChanged;

    public event EventHandler? HostSizeChanged;

    protected WindowActivatorBase(
        ILogger logger,
        IUIThreadDispatcher uiThreadDispatcher,
        IApplicationThemeSelector themeSelector,
        ISettings settings,
        ILocalizationProvider localizer,
        IApplicationIconSelector iconSelector)
        : base(logger, uiThreadDispatcher, themeSelector, settings)
    {
        Localizer = localizer;
        IconSelector = iconSelector;

        HandleClosedEvent = true;
    }

    public void Activate()
    {
        Logger.Debug<AppLog>($"Activating {typeof(TWindow)?.Name}.");

        if (Host == null)
        {
            Logger.Info<AppLog>($"No active instance for {typeof(TWindow)?.Name}, create one.");
            Activator.CreateInstance<TWindow>();
        }

        Host?.Show();
        Host?.SetForegroundWindow();

        OnWindowActivated();
    }

    public void Hide()
    {
        Logger.Debug<AppLog>($"Hiding {typeof(TWindow)?.Name}.");

        if (Host == null)
        {
            Logger.Info<AppLog>($"Cannot hide {typeof(TWindow)?.Name}, the window has not been initialized");
            return;
        }

        Host.Hide();

        OnWindowHidden();
    }

    public void Exit()
    {
        Logger.Debug<AppLog>($"Exiting {typeof(TWindow)?.Name}.");

        if (Host == null)
        {
            Logger.Info<AppLog>($"Cannot exit {typeof(TWindow)?.Name}, the window has not been initialized");
            return;
        }

        DisableHandleClosedEvent();

        Host.Close();
    }

    public void DisableHandleClosedEvent()
    {
        HandleClosedEvent = false;
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        if (Host != null)
        {
            Host.ExtendsContentIntoTitleBar = true;
            Host.AppWindow?.SetIcon(IconSelector.GetAppIconPath());

            InvalidateWindowTitle();
            InvalidateWindowPosition();
            InvalidateWindowState();
        }
    }

    protected override void RegisterToHostEvents()
    {
        base.RegisterToHostEvents();

        if (Host != null)
        {
            _currentHostDpi = Host.GetDpiForWindow();

            Host.Closed += OnWindowClosed;
            Host.WindowStateChanged += OnWindowStateChanged;
            Host.Activated += OnWindowActivationStateChanged;
            Host.SizeChanged += OnWindowSizeChanged;
        }
    }

    protected override void UnregisterFromHostEvents()
    {
        base.UnregisterFromHostEvents();

        if (Host != null)
        {
            Host.Closed -= OnWindowClosed;
            Host.WindowStateChanged -= OnWindowStateChanged;
            Host.Activated -= OnWindowActivationStateChanged;
            Host.SizeChanged -= OnWindowSizeChanged;
        }
    }

    protected virtual void OnWindowActivated()
    {
        Logger.Info<AppLog>($"Window '{typeof(TWindow)?.Name}' is activated.");
    }

    protected virtual void OnWindowClosing(WindowEventArgs e)
    {
        Logger.Info<AppLog>($"Closing window '{typeof(TWindow)?.Name}' requested.");
    }

    protected virtual void OnWindowCloseAborted()
    {
        Logger.Info<AppLog>($"Closing window '{typeof(TWindow)?.Name}' aborted.");
    }

    protected virtual void OnWindowClosed()
    {
        Logger.Info<AppLog>($"Window '{typeof(TWindow)?.Name}' is closed.");

        HandleClosedEvent = true;
    }

    protected virtual void OnWindowHidden()
    {
        Logger.Info<AppLog>($"Window '{typeof(TWindow)?.Name}' is hidden.");
    }

    protected virtual void OnWindowStateChanged()
    {
        Logger.Info<AppLog>($"Window '{typeof(TWindow)?.Name}' state has changed to {CurrentWindowState}.");
    }

    protected virtual void OnWindowActivationStateChanged()
    {
        if (Host is IActivationStateAware activationStateAware)
        {
            activationStateAware.InvalidateTitleBarOpacity(CurrentActivationState);
        }
    }

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();

        InvalidateWindowTitle();
    }

    protected override void OnFlowDirectionChanged()
    {
        base.OnFlowDirectionChanged();

        Host?.ApplyFlowDirection(CurrentFlowDirection);
    }

    protected override void OnAppThemeChanged()
    {
        base.OnAppThemeChanged();

        Host?.ApplyTheme(CurrentAppTheme);
    }

    protected virtual void InvalidateWindowPosition()
    {
        Host?.CenterOnScreen();
    }

    protected virtual void InvalidateWindowState()
    {
        if (Host != null)
        {
            Host.WindowState = WindowState.Normal;
        }
    }

    protected virtual void OnDpiChanged()
    {
        HostDpiChanged?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void OnSizeChanged()
    {
        HostSizeChanged?.Invoke(this, EventArgs.Empty);
    }

    private void InvalidateWindowTitle()
    {
        if (Host != null)
        {
            Host.Title = WindowTitle;
        }
    }

    private void OnWindowClosed(object sender, WindowEventArgs e)
    {
        if (HandleClosedEvent)
        {
            OnWindowClosing(e);
        }

        if (e.Handled)
        {
            OnWindowCloseAborted();
        }
        else
        {
            OnWindowClosed();
            Reset();
        }
    }

    private void OnWindowStateChanged(object? sender, WindowState windowState)
    {
        CurrentWindowState = windowState;

        OnWindowStateChanged();
    }

    private void OnWindowActivationStateChanged(object sender, WindowActivatedEventArgs e)
    {
        CurrentActivationState = e.WindowActivationState;

        OnWindowActivationStateChanged();
    }

    private void OnWindowSizeChanged(object sender, WindowSizeChangedEventArgs e)
    {
        uint hostDpi = Host?.GetDpiForWindow() ?? default;
        if (_currentHostDpi != default && _currentHostDpi != hostDpi)
        {
            OnDpiChanged();
        }
        _currentHostDpi = hostDpi;

        OnSizeChanged();
    }
}