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
using Microsoft.UI.Xaml.Input;
using ProtonVPN.Client.Contracts;
using ProtonVPN.Client.Helpers;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.UI.Login;
using ProtonVPN.Client.UI.Settings;
using Windows.System;

namespace ProtonVPN.Client.UI;

public sealed partial class ShellPage : IShellPage
{
    public ShellViewModel ViewModel { get; }

    public ShellPage(ShellViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
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

    private static void OnKeyboardAcceleratorInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        IMainViewNavigator viewNavigator = App.GetService<IMainViewNavigator>();

        bool result = viewNavigator.GoBack();

        args.Handled = result;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        KeyboardAccelerators.Add(BuildKeyboardAccelerator(VirtualKey.Left, VirtualKeyModifiers.Menu));
        KeyboardAccelerators.Add(BuildKeyboardAccelerator(VirtualKey.GoBack));

        App.MainWindow.IsResizable = !ViewModel.IsLoginPage;
        App.MainWindow.IsMaximizable = !ViewModel.IsLoginPage;
    }

    private void OnNavigationViewDisplayModeChanged(NavigationView sender, NavigationViewDisplayModeChangedEventArgs args)
    {
        ViewModel.OnNavigationDisplayModeChanged(args.DisplayMode);
    }

    private void OnNavigationViewItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        if (args.IsSettingsInvoked)
        {
            ViewModel.NavigateTo(typeof(SettingsViewModel).FullName!);
        }
        else
        {
            NavigationViewItem? selectedItem = args.InvokedItemContainer as NavigationViewItem;

            if (selectedItem?.GetValue(NavigationHelper.NavigateToProperty) is string pageKey)
            {
                ViewModel.NavigateTo(pageKey);
            }
        }
    }

    public void Initialize(Window window)
    {
        // Set title bar
        window.ExtendsContentIntoTitleBar = true;
        window.SetTitleBar(AppTitleBar);
        AppTitleBarText.Text = ViewModel.Title;

        // Set navigation frame
        ViewModel.InitializeViewNavigator(window, NavigationFrame);
    }
}