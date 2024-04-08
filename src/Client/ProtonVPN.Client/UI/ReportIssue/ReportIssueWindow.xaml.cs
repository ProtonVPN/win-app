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
using ProtonVPN.Client.Common.Models;
using ProtonVPN.Client.Helpers;

namespace ProtonVPN.Client.UI.ReportIssue;

public sealed partial class ReportIssueWindow
{
    private const int REPORT_ISSUE_WINDOW_WIDTH = 456;
    private const int REPORT_ISSUE_WINDOW_HEIGHT = 620;

    public ReportIssueWindow()
    {
        InitializeComponent();

        AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets/ProtonVPN.ico"));
        AppWindow.TitleBar.ExtendsContentIntoTitleBar = true;

        Shell.Initialize(this);

        InvalidateWindowPosition();
    }

    private void OnActivated(object sender, WindowActivatedEventArgs args)
    {
        WindowContainer.TitleBarOpacity = args.WindowActivationState.GetTitleBarOpacity();
    }

    private void InvalidateWindowPosition()
    {
        WindowPositionParameters parameters = new()
        {
            Width = REPORT_ISSUE_WINDOW_WIDTH,
            Height = REPORT_ISSUE_WINDOW_HEIGHT
        };

        WindowState = WindowState.Normal;

        this.SetPosition(parameters);
    }
}