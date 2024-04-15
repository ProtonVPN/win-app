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
using ProtonVPN.Client.Common.Dispatching;
using ProtonVPN.Client.Common.Models;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Helpers;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Enums;
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Logic.Services.Contracts;
using ProtonVPN.Client.Messages;
using ProtonVPN.Client.Models.Icons;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.Models.Themes;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.UI.Home;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;

namespace ProtonVPN.Client.Models.Activation;

public class MainWindowActivator : 
    WindowActivatorBase, 
    IMainWindowActivator, 
    IEventMessageReceiver<AuthenticationStatusChanged>,
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
    private readonly IApplicationIconSelector _applicationIconSelector;
    private readonly ISettings _settings;
    private readonly IEventMessageSender _eventMessageSender;
    private readonly IUIThreadDispatcher _uiThreadDispatcher;

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
        IApplicationIconSelector applicationIconSelector,
        ISettings settings,
        IEventMessageSender eventMessageSender,
        IUIThreadDispatcher uIThreadDispatcher)
        : base(logger, themeSelector)
    {
        _dialogActivator = dialogActivator;
        _overlayActivator = overlayActivator;
        _mainViewNavigator = mainViewNavigator;
        _loginViewNavigator = loginViewNavigator;
        _serviceManager = serviceManager;
        _userAuthenticator = userAuthenticator;
        _applicationIconSelector = applicationIconSelector;
        _settings = settings;
        _eventMessageSender = eventMessageSender;
        _uiThreadDispatcher = uIThreadDispatcher;
    }

    public void Initialize()
    {
        Logger.Info<AppLog>("Initializing Main Window.");

        App.MainWindow.Closed += OnMainWindowClosed;
        App.MainWindow.WindowStateChanged += OnMainWindowStateChanged;

        InvalidateWindowPosition();
        InvalidateWindowContent();
        InvalidateAppTheme();
        InvalidateFlowDirection();

        _eventMessageSender.Send(new ApplicationStartedMessage());
    }

    public void Hide()
    {
        Logger.Info<AppLog>("Hide application to tray and enable efficiency mode.");

        App.MainWindow.Hide(enableEfficiencyMode: true);

        _dialogActivator.HideAllDialogs();
    }

    public void Exit()
    {
        Logger.Info<AppLog>("Exit application.");

        _handleClosedEvents = false;

        App.MainWindow.Close();
    }

    public void DisableHandleClosedEvents()
    {
        _handleClosedEvents = false;
    }

    private void InvalidateFlowDirection()
    {
        App.MainWindow.ApplyFlowDirection(_settings.Language);
    }

    public void Activate()
    {
        Logger.Info<AppLog>("Activate application. Disable efficiency mode if enabled.");

        App.MainWindow.Show(disableEfficiencyMode: true);
        App.MainWindow.BringToFront();

        _dialogActivator.ActivateAllDialogs();
    }

    public void Receive(AuthenticationStatusChanged message)
    {
        _uiThreadDispatcher.TryEnqueue(() =>
        {
            if (message.AuthenticationStatus is AuthenticationStatus.LoggingOut)
            {
                SaveWindowPosition();
            }


            InvalidateWindowPosition();
            InvalidateWindowContent();

            InvalidateAppIcon();

            // Theme is saved in user settings which cannot be retrieved until user logged in.
            // When user logged out, app applies the default theme (Dark)
            InvalidateAppTheme();
        });
    }

    public void Receive(ConnectionStatusChanged message)
    {
        _uiThreadDispatcher.TryEnqueue(InvalidateAppIcon);
    }

    protected override void OnThemeChanged(ElementTheme theme)
    {
        InvalidateAppTheme();
    }

    protected override void OnLanguageChanged(string language)
    {
        InvalidateFlowDirection();
    }

    private void OnMainWindowClosed(object sender, WindowEventArgs args)
    {
        Logger.Info<AppLog>("The main window was requested to close.");

        SaveWindowPosition();

        if (_handleClosedEvents && _userAuthenticator.IsLoggedIn)
        {
            args.Handled = true; // Do not exit the app
            Hide();
            return;
        }

        Logger.Info<AppLog>("The client was requested to exit.");

        App.MainWindow.Closed -= OnMainWindowClosed;

        _eventMessageSender.Send(new MainWindowClosedMessage());

        _dialogActivator.CloseAllDialogs();
        _overlayActivator.CloseAllOverlays();

        Logger.Info<AppLog>("Stop Proton VPN service.");

        _serviceManager.Stop();
    }

    private void OnMainWindowStateChanged(object? sender, WindowState e)
    {
        SaveWindowState();
    }

    private void InvalidateAppTheme()
    {
        // Theme is saved in user settings which cannot be retrieved until user logged in.
        // When user is logged out, app applies the default theme (Dark)
        // When user is logged in, app applies theme based on user preferences
        App.MainWindow.ApplyTheme(ThemeSelector.GetTheme().Theme);
    }

    private void InvalidateAppIcon()
    {
        App.MainWindow.UpdateApplicationIcon(_applicationIconSelector.Get());
    }

    private void InvalidateWindowContent()
    {
        switch (_userAuthenticator.AuthenticationStatus)
        {
            case AuthenticationStatus.LoggedIn:
                _mainViewNavigator.NavigateToAsync<HomeViewModel>();
                App.MainWindow.SwitchToMainShell();
                break;

            case AuthenticationStatus.LoggingOut:
            case AuthenticationStatus.LoggingIn:
                _loginViewNavigator.NavigateToLoadingAsync();
                App.MainWindow.SwitchToLoginShell();
                break;

            default:
                _loginViewNavigator.NavigateToLoginAsync();
                App.MainWindow.SwitchToLoginShell();
                break;
        }
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
        try
        {
            if (_userAuthenticator.AuthenticationStatus is AuthenticationStatus.LoggedIn or AuthenticationStatus.LoggingOut)
            {
                if (App.MainWindow.WindowState == WindowState.Normal)
                {
                    _settings.WindowXPosition = App.MainWindow.AppWindow.Position.X;
                    _settings.WindowYPosition = App.MainWindow.AppWindow.Position.Y;
                    _settings.WindowWidth = Convert.ToInt32(App.MainWindow.Width);
                    _settings.WindowHeight = Convert.ToInt32(App.MainWindow.Height);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error<AppLog>("An exception occurred when saving the window position.", ex);
        }
    }
}