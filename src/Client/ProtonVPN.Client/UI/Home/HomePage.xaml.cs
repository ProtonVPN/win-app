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

namespace ProtonVPN.Client.UI.Home;

public sealed partial class HomePage
{
    private const double TIMER_INTERVAL_IN_MS = 500;
    private const double INLINE_MODE_THRESHOLD_WIDTH = 750.0;
    private const double PANE_WIDTH_RATIO = 0.3;
    private const double PANE_MIN_WIDTH = 300;
    private const double PANE_MAX_WIDTH = 500;

    private DispatcherTimer _timer;

    public HomeViewModel ViewModel { get; }

    public HomePage()
    {
        ViewModel = App.GetService<HomeViewModel>();
        InitializeComponent();

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(TIMER_INTERVAL_IN_MS),
        };
        _timer.Tick += OnTimerTick;

        InvalidatePaneLayout();
    }

    private void OnTimerTick(object? sender, object e)
    {
        _timer.Stop();

        InvalidatePaneLayout();
    }

    private void OnSplitViewAreaSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (!_timer.IsEnabled)
        {
            // Delay InvalidatePaneLayout to prevent a glitch with the split view control
            _timer.Start();
        }
    }

    private void InvalidatePaneLayout()
    {
        double actualWidth = SplitViewArea.ActualWidth;

        ViewModel.ConnectionDetailsPaneDisplayMode = actualWidth >= INLINE_MODE_THRESHOLD_WIDTH
            ? SplitViewDisplayMode.Inline
            : SplitViewDisplayMode.Overlay;

        double paneWidth = Math.Min(PANE_MAX_WIDTH, Math.Max(PANE_MIN_WIDTH, actualWidth * PANE_WIDTH_RATIO));

        ViewModel.ConnectionDetailsPaneWidth = paneWidth;
    }
}