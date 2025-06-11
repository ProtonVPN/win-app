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

namespace ProtonVPN.Client.UI.Main.Components;

public sealed partial class CurrentProfileIconView : IContextAware
{
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(string), typeof(CurrentProfileIconView), new PropertyMetadata(default));

    public static readonly DependencyProperty TextWrappingProperty =
        DependencyProperty.Register(nameof(TextWrapping), typeof(TextWrapping), typeof(CurrentProfileIconView), new PropertyMetadata(TextWrapping.NoWrap));

    public static readonly DependencyProperty TextTrimmingProperty =
        DependencyProperty.Register(nameof(TextTrimming), typeof(TextTrimming), typeof(CurrentProfileIconView), new PropertyMetadata(TextTrimming.CharacterEllipsis));

    public static readonly DependencyProperty IconHeightProperty =
        DependencyProperty.Register(nameof(IconHeight), typeof(double), typeof(CurrentProfileIconView), new PropertyMetadata(20.0));

    public static readonly DependencyProperty SpacingProperty =
        DependencyProperty.Register(nameof(Spacing), typeof(double), typeof(CurrentProfileIconView), new PropertyMetadata(0.0));

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public TextWrapping TextWrapping
    {
        get => (TextWrapping)GetValue(TextWrappingProperty);
        set => SetValue(TextWrappingProperty, value);
    }

    public TextTrimming TextTrimming
    {
        get => (TextTrimming)GetValue(TextTrimmingProperty);
        set => SetValue(TextTrimmingProperty, value);
    }

    public double IconHeight
    {
        get => (double)GetValue(IconHeightProperty);
        set => SetValue(IconHeightProperty, value);
    }

    public double Spacing
    {
        get => (double)GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    public CurrentProfileIconViewModel ViewModel { get; }

    public CurrentProfileIconView()
    {
        ViewModel = App.GetService<CurrentProfileIconViewModel>();

        InitializeComponent();
    }

    public object GetContext()
    {
        return ViewModel;
    }
}