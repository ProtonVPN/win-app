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
using ProtonVPN.Client.Legacy.Contracts;
using ProtonVPN.Client.Legacy.Helpers;
using ProtonVPN.Client.Legacy.Models.Icons;
using ProtonVPN.Client.Legacy.UI;
using ProtonVPN.Client.Legacy.UI.Login;
using Windows.Foundation;

namespace ProtonVPN.Client.Legacy;

public sealed partial class MainWindow
{
    private const double TITLE_BAR_HEIGHT = 36.0;

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

        UpdateApplicationIcon(ApplicationIconSelector.LoggedOutIcon);
    }

    public void SwitchToLoginShell()
    {
        Container.Content = _loginShell;

        HideTitleBar();
    }

    public void SwitchToMainShell()
    {
        Container.Content = _shell;

        ShowTitleBar();
    }

    public void UpdateApplicationIcon(ImageSource icon)
    {
        WindowContainer.IconSource = icon;

        // VPNWIN-1925 - This doesn't work when the App is pinned to the task bar.
        // When pinned, the app simply use the default icon from the project properties.
        // Consider switching to badge to reflect the current connection status
        AppWindow?.SetIcon(icon.GetFullImagePath());
    }

    private void OnActivated(object sender, WindowActivatedEventArgs args)
    {
        WindowContainer.TitleBarOpacity = args.WindowActivationState.GetTitleBarOpacity();
    }

    protected override bool OnSizeChanged(Size newSize)
    {
        this.SetDragArea(newSize.Width, TITLE_BAR_HEIGHT);

        return base.OnSizeChanged(newSize);
    }

    private void HideTitleBar()
    {
        WindowContainer.IsTitleBarVisible = false;

        IsMinimizable = false;
        IsMaximizable = false;
        IsResizable = false;
    }

    private void ShowTitleBar()
    {
        WindowContainer.IsTitleBarVisible = true;

        IsMinimizable = true;
        IsMaximizable = true;
        IsResizable = true;
    }
}