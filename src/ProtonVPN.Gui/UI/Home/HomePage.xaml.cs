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
using Microsoft.UI.Xaml.Controls;

namespace ProtonVPN.Gui.UI.Home;

public sealed partial class HomePage
{
    private const double INLINE_MODE_THRESHOLD_WIDTH = 620.0;

    public HomePage()
    {
        ViewModel = App.GetService<HomeViewModel>();
        InitializeComponent();

        SplitViewArea.SizeChanged += OnSplitViewAreaSizeChanged;
    }

    public HomeViewModel ViewModel { get; }

    private void OnSplitViewAreaSizeChanged(object sender, SizeChangedEventArgs e)
    {
        SplitViewArea.DisplayMode = SplitViewArea.ActualWidth >= INLINE_MODE_THRESHOLD_WIDTH
            ? SplitViewDisplayMode.Inline
            : SplitViewDisplayMode.Overlay;
    }
}