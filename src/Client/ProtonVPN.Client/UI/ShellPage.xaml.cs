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

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.UI.Login;
using Windows.System;
using Microsoft.UI.Xaml;

namespace ProtonVPN.Client.UI;

public sealed partial class ShellPage
{
    public ShellPage(ShellViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();

        ViewModel.PageNavigator.Frame = NavigationFrame;
        ViewModel.PageNavigator.Navigated += PageNavigator_Navigated;
        ViewModel.ViewNavigator.Initialize(NavigationViewControl);

        // TODO: Set the title bar icon by updating /Assets/WindowIcon.ico.
        // A custom title bar is required for full window theme and Mica support.
        // https://docs.microsoft.com/windows/apps/develop/title-bar?tabs=winui3#full-customization
        App.MainWindow.ExtendsContentIntoTitleBar = true;
        App.MainWindow.SetTitleBar(AppTitleBar);
        AppTitleBarText.Text = App.APPLICATION_NAME;
    }

    private void PageNavigator_Navigated(object sender, Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
    {
        ToggleNavigationViewElementVisibility();
    }

    public ShellViewModel ViewModel { get; }

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
        IPageNavigator pageNavigator = App.GetService<IPageNavigator>();

        bool result = pageNavigator.GoBack();

        args.Handled = result;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        KeyboardAccelerators.Add(BuildKeyboardAccelerator(VirtualKey.Left, VirtualKeyModifiers.Menu));
        KeyboardAccelerators.Add(BuildKeyboardAccelerator(VirtualKey.GoBack));
        ToggleNavigationViewElementVisibility();
    }

    private void ToggleNavigationViewElementVisibility()
    {
        bool isLoginPage = ViewModel.CurrentPage is LoginViewModel;
        NavigationViewControl.IsPaneVisible = !isLoginPage;
        NavigationViewControl.IsPaneToggleButtonVisible = !isLoginPage;
        NavigationViewControl.PaneDisplayMode =
            isLoginPage ? NavigationViewPaneDisplayMode.LeftMinimal : NavigationViewPaneDisplayMode.Left;
        ApplicationIcon.Visibility = isLoginPage ? Visibility.Collapsed : Visibility.Visible;
        AppTitleBarText.Visibility = isLoginPage ? Visibility.Collapsed : Visibility.Visible;
        App.MainWindow.IsResizable = !isLoginPage;
        App.MainWindow.IsMaximizable = !isLoginPage;
    }

    private void OnNavigationViewDisplayModeChanged(NavigationView sender, NavigationViewDisplayModeChangedEventArgs args)
    {
        ViewModel.OnNavigationDisplayModeChanged(args.DisplayMode);
    }

    private void Logout(object sender, RoutedEventArgs e)
    {
        AccountFlyout.Hide();
        ViewModel.PageNavigator.NavigateTo(typeof(LoginViewModel).FullName!);
    }
}