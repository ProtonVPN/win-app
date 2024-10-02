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
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using ProtonVPN.Client.Legacy.Contracts;
using ProtonVPN.Client.Legacy.Models.Navigation;
using ProtonVPN.Client.Legacy.UI.Sidebar.Bases;
using Windows.System;
using DispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue;
using DispatcherQueueTimer = Microsoft.UI.Dispatching.DispatcherQueueTimer;

namespace ProtonVPN.Client.Legacy.UI;

public sealed partial class ShellPage : IShellPage
{
    private const double EXPANDED_MODE_WIDTH_THRESHOLD = 700.0;
    private const double WIDTH_THRESHOLD = 1400;
    private const double PANE_MIN_WIDTH = 250;
    private const double PANE_MAX_WIDTH = 350;

    private readonly DispatcherQueueTimer _timer = DispatcherQueue.GetForCurrentThread().CreateTimer();

    public ShellViewModel ViewModel { get; }

    public ShellPage()
    {
        ViewModel = App.GetService<ShellViewModel>();
        InitializeComponent();

        InvalidatePaneLayout(NavigationViewControl.ActualWidth);
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
                // Force selecting the proper item on the sidebar, so it matches the actual page.
                NavigationViewControl.SelectedItem = ViewModel.SelectedMenuItem;
            }
        }
    }

    private void OnNavigationViewControlSizeChanged(object sender, SizeChangedEventArgs e)
    {
        TriggerInvalidatePaneLayout(e.NewSize.Width);
    }

    private void TriggerInvalidatePaneLayout(double totalWidth)
    {
        // Resizing the panel while transitioning to compact mode makes all menu items to disappear.
        // Similarly, editing the pane width while the panel is collapsed creates a glitch effect.
        // In both cases, adding a delay before setting the width prevent these issues.
        TimeSpan delay = totalWidth < EXPANDED_MODE_WIDTH_THRESHOLD || !ViewModel.IsNavigationPaneOpened
            ? TimeSpan.FromMilliseconds(500)
            : TimeSpan.Zero;

        _timer.Debounce(
            () => InvalidatePaneLayout(totalWidth),
            delay);
    }

    private void InvalidatePaneLayout(double totalWidth)
    {
        ViewModel.NavigationPaneWidth = totalWidth <= WIDTH_THRESHOLD
            ? PANE_MIN_WIDTH
            : PANE_MAX_WIDTH;
    }
}