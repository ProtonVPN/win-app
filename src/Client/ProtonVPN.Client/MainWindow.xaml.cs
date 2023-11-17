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
using ProtonVPN.Client.Common.Models;
using ProtonVPN.Client.Common.UI.Assets.Icons;
using ProtonVPN.Client.Contracts;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Helpers;
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.UI;
using ProtonVPN.Client.UI.Home;
using ProtonVPN.Client.UI.Login;

namespace ProtonVPN.Client;

public sealed partial class MainWindow : IEventMessageReceiver<LoggingInMessage>, IEventMessageReceiver<LoggedInMessage>, IEventMessageReceiver<LoggedOutMessage>, IEventMessageReceiver<LoggingOutMessage>
{
    private const int LOGIN_WINDOW_WIDTH = 620;
    private const int LOGIN_WINDOW_HEIGHT = 640;

    private readonly ISettings _settings;
    private readonly IUserAuthenticator _userAuthenticator;
    private readonly IMainViewNavigator _mainViewNavigator;
    private readonly ILoginViewNavigator _loginViewNavigator;

    private readonly IShellPage _shell;
    private readonly IShellPage _loginShell;

    public MainWindow(
        IMainViewNavigator mainViewNavigator,
        ILoginViewNavigator loginViewNavigator,
        IUserAuthenticator userAuthenticator,
        ISettings settings)
    {
        InitializeComponent();

        _shell = new ShellPage();
        _shell.Initialize(this);

        _loginShell = new LoginShellPage();
        _loginShell.Initialize(this);

        _mainViewNavigator = mainViewNavigator;
        _loginViewNavigator = loginViewNavigator;
        _userAuthenticator = userAuthenticator;
        _settings = settings;

        AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets/ProtonVPN.ico"));
        AppWindow.Title = App.APPLICATION_NAME;
        AppWindow.TitleBar.ExtendsContentIntoTitleBar = true;

        InvalidateWindowPosition();
        InvalidateWindowContent();
    }

    public void Receive(LoggingInMessage message)
    {
        Container.Visibility = Visibility.Collapsed;
        LoadingLogo.Visibility = Visibility.Visible;

        HideTitleBar();
    }

    public void Receive(LoggedInMessage message)
    {
        InvalidateWindowPosition();
        InvalidateWindowContent();

        LoadingLogo.Visibility = Visibility.Collapsed;
        Container.Visibility = Visibility.Visible;
    }

    public void Receive(LoggingOutMessage message)
    {
        Container.Visibility = Visibility.Collapsed;
        LoadingLogo.Visibility = Visibility.Visible;

        HideTitleBar();

        SaveWindowPosition();
    }

    public void Receive(LoggedOutMessage message)
    {
        InvalidateWindowPosition();
        InvalidateWindowContent();

        LoadingLogo.Visibility = Visibility.Collapsed;
        Container.Visibility = Visibility.Visible;
    }

    protected override void OnStateChanged(WindowState state)
    {
        base.OnStateChanged(state);

        SaveWindowState();
    }

    private void OnActivated(object sender, WindowActivatedEventArgs args)
    {
        AppTitleBar.Opacity = args.WindowActivationState.GetTitleBarOpacity();
    }

    private void OnWindowClosed(object sender, WindowEventArgs args)
    {
        SaveWindowPosition();
    }

    private void OnAppTitleBarSizeChanged(object sender, SizeChangedEventArgs e)
    {
        this.SetDragAreaFromTitleBar(AppTitleBar);
    }

    private void SaveWindowState()
    {
        if (_userAuthenticator.IsLoggedIn)
        {
            _settings.IsWindowMaximized = WindowState == WindowState.Maximized;
        }
    }

    private void SaveWindowPosition()
    {
        if (_userAuthenticator.IsLoggedIn && WindowState == WindowState.Normal)
        {
            _settings.WindowXPosition = AppWindow.Position.X;
            _settings.WindowYPosition = AppWindow.Position.Y;
            _settings.WindowWidth = Convert.ToInt32(Width);
            _settings.WindowHeight = Convert.ToInt32(Height);
        }
    }

    private void InvalidateWindowPosition()
    {
        WindowPositionParameters parameters = _userAuthenticator.IsLoggedIn
            ? new()
            {
                Width = _settings.WindowWidth ?? Width,
                Height = _settings.WindowHeight ?? Height,
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
        WindowState = WindowState.Normal;

        this.SetPosition(parameters);

        if (parameters.IsMaximized)
        {
            WindowState = WindowState.Maximized;
        }
    }

    private void InvalidateWindowContent()
    {
        if (_userAuthenticator.IsLoggedIn)
        {
            _mainViewNavigator.NavigateToAsync<HomeViewModel>();
            Container.Content = _shell;
            ShowTitleBar();
            return;
        }

        _loginViewNavigator.NavigateToSrpLoginAsync();
        Container.Content = _loginShell;
        HideTitleBar();
    }

    private void HideTitleBar()
    {
        ApplicationIcon.Visibility = Visibility.Collapsed;
        AppTitleBarText.Visibility = Visibility.Collapsed;
    }

    private void ShowTitleBar()
    {
        ApplicationIcon.Visibility = Visibility.Visible;
        AppTitleBarText.Visibility = Visibility.Visible;
    }
}