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
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using ProtonVPN.Client.Contracts;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.UI.Countries;
using ProtonVPN.Client.UI.Features.KillSwitch;
using ProtonVPN.Client.UI.Features.NetShield;
using ProtonVPN.Client.UI.Features.PortForwarding;
using ProtonVPN.Client.UI.Features.SplitTunneling;
using ProtonVPN.Client.UI.Gallery;
using ProtonVPN.Client.UI.Gateways;
using ProtonVPN.Client.UI.Home;
using ProtonVPN.Client.UI.Settings;
using ProtonVPN.Client.UI.Sidebar.Bases;
using Windows.System;

namespace ProtonVPN.Client.UI;

public sealed partial class ShellPage : IShellPage
{
    private const double TIMER_INTERVAL_IN_MS = 400;
    private const double PANE_WIDTH_RATIO = 0.2;
    private const double PANE_MIN_WIDTH = 160;
    private const double PANE_MAX_WIDTH = 300;

    private DispatcherTimer _timer;

    public ShellViewModel ViewModel { get; }

    public ShellPage()
    {
        ViewModel = App.GetService<ShellViewModel>();
        InitializeComponent();

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(TIMER_INTERVAL_IN_MS),
        };
        _timer.Tick += OnTimerTick;

        InvalidatePaneLayout();
    }

    public void Initialize(Window window)
    {
        ViewModel.InitializeViewNavigator(window, NavigationFrame);
    }

    private static KeyboardAccelerator BuildKeyboardAccelerator(VirtualKey key, VirtualKeyModifiers? modifiers = null)
    {
        KeyboardAccelerator keyboardAccelerator = new() { Key = key };

        if (modifiers.HasValue)
        {
            keyboardAccelerator.Modifiers = modifiers.Value;
        }

        keyboardAccelerator.Invoked += OnKeyboardAcceleratorInvoked;

        return keyboardAccelerator;
    }

    private static async void OnKeyboardAcceleratorInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        IMainViewNavigator viewNavigator = App.GetService<IMainViewNavigator>();

        bool result = await viewNavigator.GoBackAsync();

        args.Handled = result;
    }

    private void OnTimerTick(object? sender, object e)
    {
        _timer.Stop();

        InvalidatePaneLayout();
    }

    private void OnNavigationViewDisplayModeChanged(NavigationView sender, NavigationViewDisplayModeChangedEventArgs args)
    {
        ViewModel.OnNavigationDisplayModeChanged(args.DisplayMode);
    }

    private async void OnNavigationViewItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        NavigationViewItem? selectedItem = args.InvokedItemContainer as NavigationViewItem;

        if (selectedItem?.Tag is SidebarInteractiveItemViewModelBase sidebarItem)
        {
            bool isNavigationCompleted = await sidebarItem.InvokeAsync();
            if (!isNavigationCompleted)
            {
                // Even though the navigation was canceled, the NavigationView still keep the clicked NavigationViewItem selected.
                // Force selecting the proper item on the side bar, so it matches the actual page.
                NavigationViewControl.SelectedItem = ViewModel.SelectedMenuItem;
            }
        }
    }

    private void OnNavigationViewControlSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (!_timer.IsEnabled)
        {
            // Delay InvalidatePaneLayout to prevent a glitch with the split view control
            _timer.Start();
        }
    }

    private void InvalidatePaneLayout()
    {
        double actualWidth = NavigationViewControl.ActualWidth;

        double paneWidth = Math.Min(PANE_MAX_WIDTH, Math.Max(PANE_MIN_WIDTH, actualWidth * PANE_WIDTH_RATIO));

        ViewModel.NavigationPaneWidth = paneWidth;
    }
}