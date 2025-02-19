/*
 * Copyright (c) 2024 Proton AG
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
using ProtonVPN.Client.Core.Bases;

namespace ProtonVPN.Client.UI.Main.Home;

public sealed partial class HomeComponentView : IContextAware
{
    public static readonly DependencyProperty SidebarWidthProperty = DependencyProperty.Register(
        nameof(SidebarWidth),
        typeof(double),
        typeof(HomeComponentView),
        new PropertyMetadata(default));

    public static readonly DependencyProperty WidgetsBarWidthProperty = DependencyProperty.Register(
        nameof(WidgetsBarWidth),
        typeof(double),
        typeof(HomeComponentView),
        new PropertyMetadata(default));

    public static readonly DependencyProperty MapTopOffsetProperty = DependencyProperty.Register(
        nameof(MapTopOffset),
        typeof(double),
        typeof(HomeComponentView),
        new PropertyMetadata(default));

    public static readonly DependencyProperty MapBottomOffsetProperty = DependencyProperty.Register(
        nameof(MapBottomOffset),
        typeof(double),
        typeof(HomeComponentView),
        new PropertyMetadata(default));

    public static readonly DependencyProperty IsHomeDisplayedProperty = DependencyProperty.Register(
        nameof(IsHomeDisplayed),
        typeof(bool),
        typeof(HomeComponentView),
        new PropertyMetadata(default));

    public HomeComponentViewModel ViewModel { get; }

    public double SidebarWidth
    {
        get => (double)GetValue(SidebarWidthProperty);
        set => SetValue(SidebarWidthProperty, value);
    }

    public double WidgetsBarWidth
    {
        get => (double)GetValue(WidgetsBarWidthProperty);
        set => SetValue(WidgetsBarWidthProperty, value);
    }

    public double MapTopOffset
    {
        get => (double)GetValue(MapTopOffsetProperty);
        set => SetValue(MapTopOffsetProperty, value);
    }

    public double MapBottomOffset
    {
        get => (double)GetValue(MapBottomOffsetProperty);
        set => SetValue(MapBottomOffsetProperty, value);
    }

    public bool IsHomeDisplayed
    {
        get => (bool)GetValue(IsHomeDisplayedProperty);
        set => SetValue(IsHomeDisplayedProperty, value);
    }

    public HomeComponentView()
    {
        ViewModel = App.GetService<HomeComponentViewModel>();

        InitializeComponent();

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    public object GetContext()
    {
        return ViewModel;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        ViewModel.Activate();
        DetailsComponent.SizeChanged += InvalidateMapOffsets;
        ConnectionCardComponent.SizeChanged += InvalidateMapOffsets;
        BannersContainer.SizeChanged += InvalidateMapOffsets;
    }

    private void InvalidateMapOffsets(object sender, SizeChangedEventArgs e)
    {
        MapBottomOffset = DetailsComponent.ActualHeight + BannersContainer.ActualHeight;
        MapTopOffset = ConnectionCardComponent.ActualHeight;
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        ViewModel.Deactivate();
        DetailsComponent.SizeChanged -= InvalidateMapOffsets;
        ConnectionCardComponent.SizeChanged -= InvalidateMapOffsets;
        BannersContainer.SizeChanged -= InvalidateMapOffsets;
    }
}