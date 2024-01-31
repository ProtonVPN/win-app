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
using Microsoft.UI.Xaml.Media;
using ProtonVPN.Client.Common.UI.Assets.Icons;
using ProtonVPN.Client.Contracts;
using ProtonVPN.Client.Helpers;
using ProtonVPN.Client.UI;
using ProtonVPN.Client.UI.Login;

namespace ProtonVPN.Client;

public sealed partial class MainWindow
{
    private readonly IShellPage _shell;
    private readonly IShellPage _loginShell;

    public MainWindow()
    {
        InitializeComponent();

        _shell = new ShellPage();
        _shell.Initialize(this);

        _loginShell = new LoginShellPage();
        _loginShell.Initialize(this);

        AppWindow.Title = App.APPLICATION_NAME;
        AppWindow.TitleBar.ExtendsContentIntoTitleBar = true;

        UpdateApplicationIcon(false);
    }

    public void SwitchToLoadingScreen(string? loadingMessage = null)
    {
        Container.Content = null;

        Container.Visibility = Visibility.Collapsed;
        LoadingLogo.Visibility = Visibility.Visible;
        LoadingMessage.Text = loadingMessage;

        HideTitleBar();
    }

    public void SwitchToLoginShell()
    {
        Container.Content = _loginShell;

        LoadingLogo.Visibility = Visibility.Collapsed;
        Container.Visibility = Visibility.Visible;

        HideTitleBar();
    }

    public void SwitchToMainShell()
    {
        Container.Content = _shell;

        LoadingLogo.Visibility = Visibility.Collapsed;
        Container.Visibility = Visibility.Visible;

        ShowTitleBar();
    }

    public void UpdateApplicationIcon(bool isProtected)
    {
        ImageSource icon = isProtected
            ? ResourceHelper.GetIcon("ProtonVpnProtectedApplicationIcon")
            : ResourceHelper.GetIcon("ProtonVpnUnprotectedApplicationIcon");

        ApplicationIcon.Source = icon;

        // Note: This doesn't work when the App is pinned to the task bar.
        //       When pinned, the app simply use the default icon from the project properties.
        //       Consider switching to badge to reflect the current connection status
        AppWindow.SetIcon(icon.GetFullImagePath());
    }

    private void OnActivated(object sender, WindowActivatedEventArgs args)
    {
        AppTitleBar.Opacity = args.WindowActivationState.GetTitleBarOpacity();
    }

    private void OnAppTitleBarSizeChanged(object sender, SizeChangedEventArgs e)
    {
        this.SetDragAreaFromTitleBar(AppTitleBar);
    }

    private void HideTitleBar()
    {
        ApplicationIcon.Visibility = Visibility.Collapsed;
        AppTitleBarText.Visibility = Visibility.Collapsed;

        IsMinimizable = false;
        IsMaximizable = false;
        IsResizable = false;
    }

    private void ShowTitleBar()
    {
        ApplicationIcon.Visibility = Visibility.Visible;
        AppTitleBarText.Visibility = Visibility.Visible;

        IsMinimizable = true;
        IsMaximizable = true;
        IsResizable = true;
    }
}