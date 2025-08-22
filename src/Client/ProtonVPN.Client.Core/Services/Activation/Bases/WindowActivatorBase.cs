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
using Microsoft.UI.Xaml.Input;
using ProtonVPN.Client.Common.Dispatching;
using ProtonVPN.Client.Common.UI.Controls.Custom;
using ProtonVPN.Client.Common.UI.Keyboards;
using ProtonVPN.Client.Contracts.Services.Activation.Bases;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Extensions;
using ProtonVPN.Client.Core.Services.Selection;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;
using Windows.System;
using WinUIEx;

namespace ProtonVPN.Client.Core.Services.Activation.Bases;

public abstract class WindowActivatorBase<TWindow> : WindowHostActivatorBase<TWindow>, IWindowActivator
    where TWindow : BaseWindow
{
    protected readonly ILocalizationProvider Localizer;
    protected readonly IApplicationIconSelector IconSelector;

    private uint _currentHostDpi;

    public abstract string WindowTitle { get; }

    protected bool HandleClosedEvent { get; private set; }

    protected WindowState CurrentWindowState { get; private set; }

    protected WindowActivationState CurrentActivationState { get; private set; } = WindowActivationState.Deactivated;

    public bool IsWindowVisible { get; private set; }

    public bool EnableExitOnEsc { get; protected set; }

    public bool IsWindowFocused { get; private set; }

    private bool _isActivationPending;

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

    private void CreateWindowInstanceAndSetAutoActivation()
    {
        Logger.Info<AppLog>($"Creating instance of {HostTypeName}.");

        _isActivationPending = true;

        Activator.CreateInstance<TWindow>();
    }

    public void Activate()
    {
        if (Host == null)
        {
            CreateWindowInstanceAndSetAutoActivation();
            return;
        }

        Logger.Info<AppLog>($"Activating {HostTypeName}.");

        Host.Activate();
        Host.SetForegroundWindow();
    }

    public void Hide()
    {
        if (Host == null)
        {
            return;
        }

        Logger.Info<AppLog>($"Hiding {HostTypeName}.");

        Host.Hide();
    }

    public void Exit()
    {
        if (Host == null)
        {
            return;
        }

        Logger.Info<AppLog>($"Exiting {HostTypeName}.");

        DisableHandleClosedEvent();

        Host.Close();
    }

    public void DisableHandleClosedEvent()
    {
        HandleClosedEvent = false;
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

            if (Host.Content is FrameworkElement content && EnableExitOnEsc)
            {
                content.KeyboardAccelerators.Add(KeyboardAcceleratorBuilder.Build(OnEscapeAccelerator, VirtualKey.Escape));
            }
        }
    }

    protected virtual void OnEscapeAccelerator(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        Logger.Info<AppLog>($"Escape accelerator invoked in '{typeof(TWindow)?.Name}'.");
        args.Handled = true;
        Host?.Close();
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

            if (Host.Content is FrameworkElement content && EnableExitOnEsc)
            {
                content.KeyboardAccelerators.Clear();
            }
        }
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        if (Host != null)
        {
            Host.ExtendsContentIntoTitleBar = true;

            InvalidateWindowIcon();
            InvalidateWindowTitle();
            InvalidateWindowPosition();
            InvalidateWindowState();
        }
    }

    protected override void OnWindowLoaded()
    {
        base.OnWindowLoaded();

        if (_isActivationPending)
        {
            _isActivationPending = false;

            Activate();
        }

        InvalidateWindowFocusState();
    }

    protected virtual void OnWindowOpened()
    {
        Logger.Info<AppLog>($"Window '{HostTypeName}' is opened.");

        InvalidateWindowState();
    }

    protected virtual void OnWindowFocused()
    {
        Logger.Info<AppLog>($"Window '{HostTypeName}' is focused.");
    }

    protected virtual void OnWindowUnfocused()
    {
        Logger.Info<AppLog>($"Window '{HostTypeName}' is unfocused.");
    }

    protected virtual void OnWindowHidden()
    {
        Logger.Info<AppLog>($"Window '{HostTypeName}' is hidden.");
    }

    protected virtual void OnWindowClosing(WindowEventArgs e)
    {
        Logger.Info<AppLog>($"Closing window '{HostTypeName}' requested.");
    }

    protected virtual void OnWindowCloseAborted()
    {
        Logger.Info<AppLog>($"Closing window '{HostTypeName}' aborted.");
    }

    protected virtual void OnWindowClosed()
    {
        Logger.Info<AppLog>($"Window '{HostTypeName}' is closed.");

        // Current window instance is closed. Reset handle closed event flag for the next instance.
        HandleClosedEvent = true;
    }

    protected virtual void OnWindowStateChanged()
    {
        Logger.Info<AppLog>($"Window '{HostTypeName}' state has changed to {CurrentWindowState}.");
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

    protected virtual void InvalidateWindowIcon()
    {
        Host?.AppWindow?.SetIcon(IconSelector.GetAppIconPath());
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

    private void InvalidateWindowVisibilityState()
    {
        bool isWindowVisible = Host != null 
            && Host.Visible
            && CurrentWindowState is not WindowState.Minimized;

        if (IsWindowVisible != isWindowVisible)
        {
            IsWindowVisible = isWindowVisible;

            if (IsWindowVisible)
            {
                OnWindowOpened();
            }
            else
            {
                OnWindowHidden();
            }
        }
    }

    private void InvalidateWindowFocusState()
    {
        bool isWindowFocused = Host != null
            && CurrentActivationState is not WindowActivationState.Deactivated;

        if (IsWindowFocused != isWindowFocused)
        {
            IsWindowFocused = isWindowFocused;

            if (IsWindowFocused)
            {
                InvalidateWindowVisibilityState();
                if (IsWindowVisible)
                {
                    OnWindowFocused();
                }
            }
            else
            {
                if (IsWindowVisible)
                {
                    OnWindowUnfocused();
                }
                InvalidateWindowVisibilityState();
            }
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

            InvalidateWindowVisibilityState();
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

        if (IsWindowLoaded)
        {
            OnWindowStateChanged();

            InvalidateWindowFocusState();
        }
    }

    private void OnWindowActivationStateChanged(object sender, WindowActivatedEventArgs e)
    {
        CurrentActivationState = e.WindowActivationState;

        if (IsWindowLoaded)
        {
            InvalidateWindowFocusState();

            if (Host is IFocusAware focusAware)
            {
                focusAware.OnFocusChanged();
            }
        }
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