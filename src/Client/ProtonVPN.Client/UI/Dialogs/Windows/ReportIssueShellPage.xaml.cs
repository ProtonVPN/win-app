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
using ProtonVPN.Client.Contracts;

namespace ProtonVPN.Client.UI.Dialogs.Windows;

public sealed partial class ReportIssueShellPage : IShellPage
{
    public ReportIssueShellViewModel ViewModel { get; }

    public ReportIssueShellPage()
    {
        ViewModel = App.GetService<ReportIssueShellViewModel>();
        InitializeComponent();
    }

    public void Initialize(Window window)
    {
        // Set Title bar
        window.ExtendsContentIntoTitleBar = true;
        window.SetTitleBar(WindowTitleBar);
        WindowTitleBarText.Text = ViewModel.Title;

        // Set Frame
        ViewModel.InitializeViewNavigator(window, NavigationFrame);
    }
}