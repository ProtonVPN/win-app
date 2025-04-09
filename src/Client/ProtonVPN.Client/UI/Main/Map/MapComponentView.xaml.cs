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

namespace ProtonVPN.Client.UI.Main.Map;

public sealed partial class MapComponentView : IContextAware
{
    public static readonly DependencyProperty LeftOffsetProperty = DependencyProperty.Register(
        nameof(LeftOffset),
        typeof(double),
        typeof(MapComponentView),
        new PropertyMetadata(default));

    public static readonly DependencyProperty TopOffsetProperty = DependencyProperty.Register(
        nameof(TopOffset),
        typeof(double),
        typeof(MapComponentView),
        new PropertyMetadata(default));

    public static readonly DependencyProperty RightOffsetProperty = DependencyProperty.Register(
        nameof(RightOffset),
        typeof(double),
        typeof(MapComponentView),
        new PropertyMetadata(default));

    public static readonly DependencyProperty BottomOffsetProperty = DependencyProperty.Register(
        nameof(BottomOffset),
        typeof(double),
        typeof(MapComponentView),
        new PropertyMetadata(default));

    public double LeftOffset
    {
        get => (double)GetValue(LeftOffsetProperty);
        set => SetValue(LeftOffsetProperty, value);
    }

    public double TopOffset
    {
        get => (double)GetValue(TopOffsetProperty);
        set => SetValue(TopOffsetProperty, value);
    }

    public double RightOffset
    {
        get => (double)GetValue(RightOffsetProperty);
        set => SetValue(RightOffsetProperty, value);
    }

    public double BottomOffset
    {
        get => (double)GetValue(BottomOffsetProperty);
        set => SetValue(BottomOffsetProperty, value);
    }

    public MapComponentViewModel ViewModel { get; }

    public MapComponentView()
    {
        ViewModel = App.GetService<MapComponentViewModel>();

        InitializeComponent();
    }

    public object GetContext()
    {
        return ViewModel;
    }
}