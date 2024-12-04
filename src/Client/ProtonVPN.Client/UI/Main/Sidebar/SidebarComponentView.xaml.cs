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

namespace ProtonVPN.Client.UI.Main.Sidebar;

public sealed partial class SidebarComponentView : IContextAware
{
    public static readonly DependencyProperty IsSidebarExpandedProperty =
        DependencyProperty.Register(nameof(IsSidebarExpanded), typeof(bool), typeof(SidebarComponentView), new PropertyMetadata(default));

    public bool IsSidebarExpanded
    {
        get => (bool)GetValue(IsSidebarExpandedProperty);
        set => SetValue(IsSidebarExpandedProperty, value);
    }

    public SidebarComponentViewModel ViewModel { get; }

    public SidebarViewNavigator Navigator { get; }

    public SidebarComponentView()
    {
        ViewModel = App.GetService<SidebarComponentViewModel>();
        Navigator = App.GetService<SidebarViewNavigator>();

        InitializeComponent();

        Navigator.Initialize(SidebarNavigationFrame);

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;

        KeyboardAccelerators.AddHandler(OnCtrlFInvoked, VirtualKey.F, VirtualKeyModifiers.Control);

        KeyboardAcceleratorPlacementMode = KeyboardAcceleratorPlacementMode.Hidden;
    }

    public object GetContext()
    {
        return ViewModel;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Navigator.Load();
        ViewModel.Activate();
    }

    private void OnCtrlFInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        if (IsLoaded)
        {
            args.Handled = true;
            SearchTextBox.Focus(FocusState.Programmatic);
        }
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        ViewModel.Deactivate();
        Navigator.Unload();
    }
}