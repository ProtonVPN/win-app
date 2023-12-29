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

using H.NotifyIcon;
using Microsoft.UI.Xaml;
using ProtonVPN.Client.Common.Models;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Helpers;
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Logic.Services.Contracts;
using ProtonVPN.Client.Messages;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.Models.Themes;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.UI.Home;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.Models.Activation;

public class MainWindowActivator : 
    WindowActivatorBase, 
    IMainWindowActivator, 
    IEventMessageReceiver<LoggingInMessage>, 
    IEventMessageReceiver<LoggedInMessage>, 
    IEventMessageReceiver<LoggingOutMessage>, 
    IEventMessageReceiver<LoggedOutMessage>, 
    IEventMessageReceiver<ConnectionStatusChanged>
{
    private const int LOGIN_WINDOW_WIDTH = 620;
    private const int LOGIN_WINDOW_HEIGHT = 640;

    private readonly IDialogActivator _dialogActivator;
    private readonly IOverlayActivator _overlayActivator;
    private readonly IMainViewNavigator _mainViewNavigator;
    private readonly ILoginViewNavigator _loginViewNavigator;
    private readonly IServiceManager _serviceManager;
    private readonly IUserAuthenticator _userAuthenticator;
    private readonly IConnectionManager _connectionManager;
    private readonly ISettings _settings;
    private readonly IEventMessageSender _eventMessageSender;

    private bool _handleClosedEvents = true;

    public MainWindowActivator(
        ILogger logger,
        IThemeSelector themeSelector,
        IDialogActivator dialogActivator,
        IOverlayActivator overlayActivator,
        IMainViewNavigator mainViewNavigator,
        ILoginViewNavigator loginViewNavigator,
        IServiceManager serviceManager,
        IUserAuthenticator userAuthenticator,
        IConnectionManager connectionManager,
        ISettings settings,
        IEventMessageSender eventMessageSender)
        : base(logger, themeSelector)
    {
        _dialogActivator = dialogActivator;
        _overlayActivator = overlayActivator;
        _mainViewNavigator = mainViewNavigator;
        _loginViewNavigator = loginViewNavigator;
        _serviceManager = serviceManager;
        _userAuthenticator = userAuthenticator;
        _connectionManager = connectionManager;
        _settings = settings;
        _eventMessageSender = eventMessageSender;
    }

    public void Show()
    {
        App.MainWindow.Closed += OnMainWindowClosed;
        App.MainWindow.WindowStateChanged += OnMainWindowStateChanged;

        InvalidateWindowPosition();
        InvalidateWindowContent();
        InvalidateAppTheme();

        App.MainWindow.Show();
        App.MainWindow.BringToFront();
    }

    public void Activate()
    {
        App.MainWindow.Activate();
        App.MainWindow.BringToFront();

        _dialogActivator.ActivateAllDialogs();
    }

    public void Hide()
    {
        App.MainWindow.Hide(true);

        _dialogActivator.HideAllDialogs();
    }

    public void Exit()
    {
        _handleClosedEvents = false;

        App.MainWindow.Close();
    }

    public void Receive(LoggingInMessage message)
    {
        App.MainWindow.SwitchToLoadingScreen();
    }

    public void Receive(LoggedInMessage message)
    {
        InvalidateWindowPosition();
        InvalidateWindowContent();

        InvalidateAppIcon();

        // Apply theme based on user settings
        InvalidateAppTheme();
    }

    public void Receive(LoggingOutMessage message)
    {
        App.MainWindow.SwitchToLoadingScreen();

        SaveWindowPosition();
    }

    public void Receive(LoggedOutMessage message)
    {
        InvalidateWindowPosition();
        InvalidateWindowContent();

        InvalidateAppIcon();

        // Theme is saved in user settings which cannot be retrieved until user logged in.
        // When user logged out, app applies the default theme (Dark)
        InvalidateAppTheme();
    }

    public void Receive(ConnectionStatusChanged message)
    {
        InvalidateAppIcon();
    }

    protected override void OnThemeChanged(ElementTheme theme)
    {
        InvalidateAppTheme();
    }

    private void OnMainWindowClosed(object sender, WindowEventArgs args)
    {
        SaveWindowPosition();

        if (_handleClosedEvents)
        {
            // Hide window to tray
            args.Handled = true;
            Hide();
            return;
        }

        App.MainWindow.Closed -= OnMainWindowClosed;

        _eventMessageSender.Send(new MainWindowClosedMessage());

        _dialogActivator.CloseAllDialogs();
        _overlayActivator.CloseAllOverlays();

        _serviceManager.Stop();
    }
    private void OnMainWindowStateChanged(object? sender, WindowState e)
    {
        SaveWindowState();
    }

    private void InvalidateAppTheme()
    {
        App.MainWindow.ApplyTheme(ThemeSelector.GetTheme().Theme);
    }

    private void InvalidateAppIcon()
    {
        App.MainWindow.UpdateApplicationIcon(_userAuthenticator.IsLoggedIn && _connectionManager.IsConnected);
    }

    private void InvalidateWindowContent()
    {
        if (_userAuthenticator.IsLoggedIn)
        {
            _mainViewNavigator.NavigateToAsync<HomeViewModel>();
            App.MainWindow.SwitchToMainShell();
            return;
        }

        _loginViewNavigator.NavigateToLoginAsync();
        App.MainWindow.SwitchToLoginShell();
    }

    private void InvalidateWindowPosition()
    {
        WindowPositionParameters parameters = _userAuthenticator.IsLoggedIn
            ? new()
            {
                Width = _settings.WindowWidth ?? App.MainWindow.Width,
                Height = _settings.WindowHeight ?? App.MainWindow.Height,
                XPosition = _settings.WindowXPosition,
                YPosition = _settings.WindowYPosition,
                IsMaximized = _settings.IsWindowMaximized
            }
            : new()
            {
                Width = LOGIN_WINDOW_WIDTH,
                Height = LOGIN_WINDOW_HEIGHT,
                IsMaximized = false,
            };

        // Window size and position are saved for the normal state. Reset state to normal before applying them.
        App.MainWindow.WindowState = WindowState.Normal;

        App.MainWindow.SetPosition(parameters);

        if (parameters.IsMaximized)
        {
            App.MainWindow.WindowState = WindowState.Maximized;
        }
    }

    private void SaveWindowState()
    {
        if (_userAuthenticator.IsLoggedIn)
        {
            _settings.IsWindowMaximized = App.MainWindow.WindowState == WindowState.Maximized;
        }
    }

    private void SaveWindowPosition()
    {
        if (_userAuthenticator.IsLoggedIn && App.MainWindow.WindowState == WindowState.Normal)
        {
            _settings.WindowXPosition = App.MainWindow.AppWindow.Position.X;
            _settings.WindowYPosition = App.MainWindow.AppWindow.Position.Y;
            _settings.WindowWidth = Convert.ToInt32(App.MainWindow.Width);
            _settings.WindowHeight = Convert.ToInt32(App.MainWindow.Height);
        }
    }
}