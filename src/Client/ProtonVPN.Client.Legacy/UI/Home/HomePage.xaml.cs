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

using CommunityToolkit.WinUI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ProtonVPN.Client.Legacy.UI.Home;

public sealed partial class HomePage
{
    private const double INLINE_MODE_WIDTH_THRESHOLD = 750.0;
    private const double WIDTH_THRESHOLD = 1400;
    private const double PANE_MIN_WIDTH = 300;
    private const double PANE_MAX_WIDTH = 500;

    private readonly DispatcherQueueTimer _timer = DispatcherQueue.GetForCurrentThread().CreateTimer();

    public HomeViewModel ViewModel { get; }

    public HomePage()
    {
        ViewModel = App.GetService<HomeViewModel>();
        InitializeComponent();

        InvalidatePaneLayout(SplitViewArea.ActualWidth);
    }

    private void OnSplitViewAreaSizeChanged(object sender, SizeChangedEventArgs e)
    {
        TriggerInvalidatePaneLayout(e.NewSize.Width);
    }

    private void TriggerInvalidatePaneLayout(double totalWidth)
    {
        // Glitch happens if resizing the pane while resizing the split view.
        // Debouncing the call fixes the issue (even with no delay)
        _timer.Debounce(
            () => InvalidatePaneLayout(totalWidth),
            TimeSpan.Zero);
    }

    private void InvalidatePaneLayout(double totalWidth)
    {
        ViewModel.ConnectionDetailsPaneDisplayMode = totalWidth >= INLINE_MODE_WIDTH_THRESHOLD
            ? SplitViewDisplayMode.Inline
            : SplitViewDisplayMode.Overlay;

        ViewModel.ConnectionDetailsPaneWidth = totalWidth <= WIDTH_THRESHOLD
            ? PANE_MIN_WIDTH
            : PANE_MAX_WIDTH;
    }
}