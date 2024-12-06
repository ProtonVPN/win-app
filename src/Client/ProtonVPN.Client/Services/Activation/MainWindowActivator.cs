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
using ProtonVPN.Client.Common.Messages;
using ProtonVPN.Client.Common.Models;
using ProtonVPN.Client.Contracts.Messages;
using ProtonVPN.Client.Core.Extensions;
using ProtonVPN.Client.Core.Helpers;
using ProtonVPN.Client.Core.Messages;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.Core.Services.Activation.Bases;
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
using WinUIEx;

namespace ProtonVPN.Client.Services.Activation;

public class MainWindowActivator : WindowActivatorBase<MainWindow>, IMainWindowActivator,
    IEventMessageReceiver<AuthenticationStatusChanged>
{
    private const int LOGIN_WINDOW_WIDTH = 1016;
    private const int LOGIN_WINDOW_HEIGHT = 659;

    private readonly IUserAuthenticator _userAuthenticator;
    private readonly IEventMessageSender _eventMessageSender;

    public bool IsWindowVisible { get; private set; }

    public Size CurrentWindowSize => new(Host?.Width ?? 0, Host?.Height ?? 0);

    public Window? Window => Host;

    public override string WindowTitle { get; } = App.APPLICATION_NAME;

    public MainWindowActivator(
        ILogger logger,
        IUIThreadDispatcher uiThreadDispatcher,
        IApplicationThemeSelector themeSelector,
        ISettings settings,
        ILocalizationProvider localizer,
        IApplicationIconSelector iconSelector,
        IUserAuthenticator userAuthenticator,
        IEventMessageSender eventMessageSender)
        : base(logger, uiThreadDispatcher, themeSelector, settings, localizer, iconSelector)
    {
        _userAuthenticator = userAuthenticator;
        _eventMessageSender = eventMessageSender;
    }

    public void Receive(AuthenticationStatusChanged message)
    {
        UIThreadDispatcher.TryEnqueue(() =>
        {
            if (_userAuthenticator.AuthenticationStatus is AuthenticationStatus.LoggingOut)
            {
                SaveWindowPosition();
            }

            InvalidateWindowTitleBarVisibility();

            InvalidateWindowPosition();
        });
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        InvalidateWindowTitleBarVisibility();

        _eventMessageSender.Send(new ApplicationStartingMessage());
    }

    protected override void InvalidateWindowPosition()
    {
        if (Host == null)
        {
            Logger.Error<AppLog>("Window has not been initialized");
            return;
        }

        WindowPositionParameters parameters =
            _userAuthenticator.IsLoggedIn
                ? new()
                {
                    Width = Settings.WindowWidth,
                    Height = Settings.WindowHeight,
                    XPosition = Settings.WindowXPosition,
                    YPosition = Settings.WindowYPosition,
                    IsMaximized = Settings.IsWindowMaximized
                }
                : new()
                {
                    // TODO: Check if setting width and height on the window xaml is enough. Only overrides if logged in.
                    Width = LOGIN_WINDOW_WIDTH,
                    Height = LOGIN_WINDOW_HEIGHT,
                    IsMaximized = false,
                };

        // Set window state to normal to apply size and position.
        Host.WindowState = WindowState.Normal;
        Host.SetPosition(parameters);

        if (parameters.IsMaximized)
        {
            Host.WindowState = WindowState.Maximized;
        }
    }

    protected override void OnWindowActivated()
    {
        base.OnWindowActivated();

        EfficiencyModeHelper.DisableEfficiencyMode();

        InvalidateWindowVisibilityState(true);
    }

    protected override void OnWindowHidden()
    {
        base.OnWindowHidden();

        EfficiencyModeHelper.EnableEfficiencyMode();

        SaveWindowPosition();

        InvalidateWindowVisibilityState(false);
    }

    protected override void OnWindowStateChanged()
    {
        base.OnWindowStateChanged();

        SaveWindowState();
        SaveWindowPosition();

        InvalidateWindowVisibilityState(CurrentWindowState is not WindowState.Minimized);
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

        SaveWindowPosition();

        _eventMessageSender.Send(new ApplicationStoppedMessage());
    }

    private void SaveWindowState()
    {
        if (_userAuthenticator.IsLoggedIn && CurrentWindowState != WindowState.Minimized)
        {
            Settings.IsWindowMaximized = CurrentWindowState == WindowState.Maximized;
        }
    }

    private void SaveWindowPosition()
    {
        try
        {
            bool isUserInScope = _userAuthenticator.AuthenticationStatus
                is AuthenticationStatus.LoggedIn
                or AuthenticationStatus.LoggingOut;

            if (isUserInScope && Host != null && CurrentWindowState == WindowState.Normal)
            {
                Settings.WindowXPosition = Host.AppWindow.Position.X;
                Settings.WindowYPosition = Host.AppWindow.Position.Y;
                Settings.WindowWidth = Convert.ToInt32(Host.Width);
                Settings.WindowHeight = Convert.ToInt32(Host.Height);
            }
        }
        catch (Exception ex)
        {
            Logger.Error<AppLog>("An exception occurred when saving the window position.", ex);
        }
    }

    private void InvalidateWindowVisibilityState(bool isWindowVisible)
    {
        if (IsWindowVisible != isWindowVisible)
        {
            IsWindowVisible = isWindowVisible;

            _eventMessageSender.Send(new MainWindowVisibilityChangedMessage { IsMainWindowVisible = isWindowVisible });
        }
    }

    private void InvalidateWindowTitleBarVisibility()
    {
        Host?.InvalidateTitleBarVisibility(isTitleBarVisible: _userAuthenticator.IsLoggedIn);
    }
}