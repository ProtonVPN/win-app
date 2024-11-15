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
using Microsoft.UI.Xaml.Input;
using ProtonVPN.Client.Common.UI.Keyboards;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Services.Navigation;
using Windows.System;

namespace ProtonVPN.Client.UI.Main.Sidebar.Connections;

public sealed partial class ConnectionsPageView : IContextAware
{
    public ConnectionsPageViewModel ViewModel { get; }
    public ConnectionsViewNavigator Navigator { get; }

    public ConnectionsPageView()
    {
        ViewModel = App.GetService<ConnectionsPageViewModel>();
        Navigator = App.GetService<ConnectionsViewNavigator>();
         
        InitializeComponent();

        Navigator.Initialize(ConnectionsNavigationFrame);

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    public object GetContext()
    {
        return ViewModel;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Navigator.Load();
        ViewModel.Activate();

        KeyboardAccelerators.AddHandler(OnCtrl1Invoked, VirtualKey.Number1, VirtualKeyModifiers.Control);
        KeyboardAccelerators.AddHandler(OnCtrl2Invoked, VirtualKey.Number2, VirtualKeyModifiers.Control);
        KeyboardAccelerators.AddHandler(OnCtrl3Invoked, VirtualKey.Number3, VirtualKeyModifiers.Control);
        KeyboardAccelerators.AddHandler(OnCtrl4Invoked, VirtualKey.Number4, VirtualKeyModifiers.Control);
    }

    private void OnCtrl1Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        args.Handled = true;
        Navigator.NavigateToRecentsViewAsync();
    }

    private void OnCtrl2Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        args.Handled = true;
        Navigator.NavigateToCountriesViewAsync();
    }

    private void OnCtrl3Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        args.Handled = true;
        Navigator.NavigateToProfilesViewAsync();
    }

    private void OnCtrl4Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        args.Handled = true;
        Navigator.NavigateToGatewaysViewAsync();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        ViewModel.Deactivate();
        Navigator.Unload();
    }
}