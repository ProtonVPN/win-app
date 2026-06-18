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

using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using ProtonVPN.Client.Common.Dispatching;
using ProtonVPN.Client.Common.Messages;
using ProtonVPN.Client.Common.Models;
using ProtonVPN.Client.Contracts.Messages;
using ProtonVPN.Client.Core.Extensions;
using ProtonVPN.Client.Core.Messages;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.Core.Services.Activation.Bases;
using ProtonVPN.Client.Core.Services.Enabling;
using ProtonVPN.Client.Core.Services.Selection;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Enums;
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;
using Windows.Foundation;
using Windows.Graphics;
using WinUIEx;

namespace ProtonVPN.Client.Services.Activation;

public class MainWindowActivator : WindowActivatorBase<MainWindow>, IMainWindowActivator,
    IEventMessageReceiver<AuthenticationStatusChanged>,
    IEventMessageReceiver<WindowsSessionEndingMessage>,
    IEventMessageReceiver<AppIconStatusChangedMessage>
{
    private const int LOGIN_WINDOW_WIDTH = 1016;
    private const int LOGIN_WINDOW_HEIGHT = 659;

    private SizeInt32? _lastKnownWindowSize;
    private PointInt32? _lastKnownWindowPosition;
    private bool _isWindowMovable = true;

    private readonly IUserAuthenticator _userAuthenticator;
    private readonly IEventMessageSender _eventMessageSender;
    private readonly IEfficiencyModeEnabler _efficiencyModeEnabler;

    public Size CurrentWindowSize => new(Host?.Width ?? 0, Host?.Height ?? 0);

    public Window? Window => Host;

    public override string WindowTitle { get; } = App.APPLICATION_NAME;

    protected bool IsUserInScope => _userAuthenticator.AuthenticationStatus is AuthenticationStatus.LoggedIn or AuthenticationStatus.LoggingOut;

    protected bool IsWindowInRestoredState => Host?.AppWindow?.Presenter is OverlappedPresenter presenter
                                           && presenter.State == OverlappedPresenterState.Restored;

    protected override bool ShouldCreateInstanceIfMissing => false;

    public MainWindowActivator(
        ILogger logger,
        IUIThreadDispatcher uiThreadDispatcher,
        IApplicationThemeSelector themeSelector,
        ISettings settings,
        ILocalizationService localizationService,
        ILocalizationProvider localizer,
        IApplicationIconSelector iconSelector,
        IUserAuthenticator userAuthenticator,
        IEventMessageSender eventMessageSender,
        IEfficiencyModeEnabler efficiencyModeEnabler)
        : base(logger, 
               uiThreadDispatcher,
               themeSelector,
               settings,
               localizationService,
               localizer,
               iconSelector)
    {
        _userAuthenticator = userAuthenticator;
        _eventMessageSender = eventMessageSender;
        _efficiencyModeEnabler = efficiencyModeEnabler;
    }

    public void Receive(AuthenticationStatusChanged message)
    {
        bool shouldInvalidateWindowPosition = 
            _isWindowMovable &&
            _userAuthenticator.AuthenticationStatus is AuthenticationStatus.LoggingOut or AuthenticationStatus.LoggedIn;

        UIThreadDispatcher.TryEnqueue(() =>
        {
            if (_userAuthenticator.AuthenticationStatus is AuthenticationStatus.LoggingOut)
            {
                SaveWindowPosition();
            }

            InvalidateWindowTitleBarVisibility();

            if (shouldInvalidateWindowPosition)
            {
                InvalidateWindowPosition();
            }
            InvalidateWindowState();
        });
    }

    public void SetWindowMovable(bool isMovable)
    {
        _isWindowMovable = isMovable;
    }

    public void Receive(AppIconStatusChangedMessage message)
    {
        UIThreadDispatcher.TryEnqueue(InvalidateBadgeIcon);
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        InvalidateBadgeIcon();
        InvalidateWindowTitleBarVisibility();

        _eventMessageSender.Send(new ApplicationStartingMessage());

        _efficiencyModeEnabler.TryEnableEfficiencyMode();
    }

    protected override void RegisterToHostEvents()
    {
        base.RegisterToHostEvents();

        if (Host?.AppWindow != null)
        {
            Host.AppWindow.Changed += OnAppWindowChanged;
        }
    }

    protected override void UnregisterFromHostEvents()
    {
        base.UnregisterFromHostEvents();

        if (Host?.AppWindow != null)
        {
            Host.AppWindow.Changed -= OnAppWindowChanged;
        }
    }

    protected override void InvalidateWindowPosition()
    {
        if (Host == null)
        {
            Logger.Info<AppLog>($"Cannot invalidate {typeof(MainWindow)?.Name} position, the window has not been initialized");
            return;
        }

        WindowPositionParameters parameters =
            _userAuthenticator.IsLoggedIn
                ? new()
                {
                    Width = Settings.WindowLocation.WindowWidth,
                    Height = Settings.WindowLocation.WindowHeight,
                    XPosition = Settings.WindowLocation.WindowXPosition,
                    YPosition = Settings.WindowLocation.WindowYPosition,
                }
                : new()
                {
                    Width = LOGIN_WINDOW_WIDTH,
                    Height = LOGIN_WINDOW_HEIGHT,
                    XPosition = Settings.WindowLocation.WindowXPosition,
                    YPosition = Settings.WindowLocation.WindowYPosition,
                    IsCentered = true,
                };

        // Set window state to normal to apply size and position.
        Host.WindowState = WindowState.Normal;
        Host.MoveAndResize(parameters);
    }

    protected override void InvalidateWindowState()
    {
        if (Host == null)
        {
            Logger.Info<AppLog>($"Cannot invalidate {typeof(MainWindow)?.Name} state, the window has not been initialized");
            return;
        }

        bool isToMaximize = _userAuthenticator.IsLoggedIn && Settings.WindowLocation.IsWindowMaximized;

        if (isToMaximize && (IsWindowVisible || Host.WindowState == WindowState.Minimized))
        {
            Host.WindowState = WindowState.Maximized;
        }
    }

    protected override void OnWindowOpened()
    {
        base.OnWindowOpened();

        _efficiencyModeEnabler.TryDisableEfficiencyMode();

        _eventMessageSender.Send(new MainWindowVisibilityChangedMessage { IsMainWindowVisible = true });

        InvalidateBadgeIcon();
    }

    protected override void OnWindowHidden()
    {
        base.OnWindowHidden();

        _eventMessageSender.Send(new MainWindowVisibilityChangedMessage { IsMainWindowVisible = false });

        SaveWindowPosition();

        _efficiencyModeEnabler.TryEnableEfficiencyMode();
    }

    protected override void OnWindowStateChanged()
    {
        base.OnWindowStateChanged();

        SaveWindowState();
        SaveWindowPosition();
    }

    protected override void OnWindowClosing(WindowEventArgs e)
    {
        base.OnWindowClosing(e);

        if (_userAuthenticator.IsLoggedIn)
        {
            e.Handled = true;
        }
    }

    protected override void OnWindowCloseAborted()
    {
        base.OnWindowCloseAborted();

        Hide();
    }

    protected override void OnWindowClosed()
    {
        base.OnWindowClosed();

        Host?.DisposeTrayIcon();

        SaveWindowPosition();

        _eventMessageSender.Send(new ApplicationStoppedMessage());
    }

    protected override void OnWindowFocused()
    {
        base.OnWindowFocused();

        _eventMessageSender.Send(new MainWindowFocusedMessage());
    }

    private void OnAppWindowChanged(AppWindow sender, AppWindowChangedEventArgs args)
    {
        if (args.DidSizeChange)
        {
            UpdateLastKnownWindowSize();
        }

        if (args.DidPositionChange)
        {
            UpdateLastKnownWindowPosition();
        }
    }

    private void UpdateLastKnownWindowSize()
    {
        if (IsUserInScope && IsWindowInRestoredState)
        {
            _lastKnownWindowSize = new SizeInt32(
                Convert.ToInt32(Host!.Width), 
                Convert.ToInt32(Host!.Height));
        }
    }

    private void UpdateLastKnownWindowPosition()
    {
        if (IsUserInScope && IsWindowInRestoredState)
        {
            _lastKnownWindowPosition = Host!.AppWindow.Position;
        }
    }

    private void InvalidateBadgeIcon()
    {
        Host?.SetBadge(IconSelector.GetTaskbarBadgeIcon());
    }

    private void SaveWindowState()
    {
        if (_userAuthenticator.IsLoggedIn && CurrentWindowState != WindowState.Minimized)
        {
            Settings.WindowLocation = Settings.WindowLocation with
            { 
                IsWindowMaximized = CurrentWindowState == WindowState.Maximized
            };
        }
    }

    private void SaveWindowPosition()
    {
        try
        {
            if (IsUserInScope)
            {
                if (_lastKnownWindowPosition.HasValue)
                {
                    if (_lastKnownWindowSize.HasValue)
                    {
                        Settings.WindowLocation = Settings.WindowLocation with
                        {
                            WindowXPosition = _lastKnownWindowPosition.Value.X,
                            WindowYPosition = _lastKnownWindowPosition.Value.Y,
                            WindowWidth = _lastKnownWindowSize.Value.Width,
                            WindowHeight = _lastKnownWindowSize.Value.Height,
                        };
                    }
                    else
                    {
                        Settings.WindowLocation = Settings.WindowLocation with
                        {
                            WindowXPosition = _lastKnownWindowPosition.Value.X,
                            WindowYPosition = _lastKnownWindowPosition.Value.Y,
                        };
                    }
                }
                else if (_lastKnownWindowSize.HasValue)
                {
                    Settings.WindowLocation = Settings.WindowLocation with
                    {
                        WindowWidth = _lastKnownWindowSize.Value.Width,
                        WindowHeight = _lastKnownWindowSize.Value.Height,
                    };
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error<AppLog>("An exception occurred when saving the window position.", ex);
        }
    }

    private void InvalidateWindowTitleBarVisibility()
    {
        Host?.InvalidateTitleBarVisibility(isTitleBarVisible: _userAuthenticator.IsLoggedIn);
    }

    public void Receive(WindowsSessionEndingMessage message)
    {
        SaveWindowPosition();
    }
}