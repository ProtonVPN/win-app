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
using ProtonVPN.Client.Legacy.Helpers;

namespace ProtonVPN.Client.Legacy.UI.ReportIssue;

public sealed partial class ReportIssueWindow
{
    private const int WINDOW_WIDTH = 456;
    private const int WINDOW_HEIGHT = 620;

    public ReportIssueWindow()
    {
        InitializeComponent();
        WindowConfigurator.Set(this, width: WINDOW_WIDTH, height: WINDOW_HEIGHT);

        Shell.Initialize(this);
    }

    private void OnActivated(object sender, WindowActivatedEventArgs args)
    {
        WindowContainer.TitleBarOpacity = args.WindowActivationState.GetTitleBarOpacity();
    }
}