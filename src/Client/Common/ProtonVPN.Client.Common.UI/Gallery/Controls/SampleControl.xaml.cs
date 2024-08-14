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
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;

namespace ProtonVPN.Client.Common.UI.Gallery.Controls;

[ContentProperty(Name = "Sample")]
public sealed partial class SampleControl : UserControl
{
    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register(nameof(Header), typeof(string), typeof(SampleControl), new PropertyMetadata(default));

    public static readonly DependencyProperty SampleProperty =
        DependencyProperty.Register(nameof(Sample), typeof(object), typeof(SampleControl), new PropertyMetadata(default));

    public static readonly DependencyProperty OptionsProperty =
        DependencyProperty.Register(nameof(Options), typeof(object), typeof(SampleControl), new PropertyMetadata(default));

    public SampleControl()
    {
        InitializeComponent();
    }

    public string Header
    {
        get => (string)GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public object Sample
    {
        get => GetValue(SampleProperty);
        set => SetValue(SampleProperty, value);
    }

    public object Options
    {
        get => GetValue(OptionsProperty);
        set => SetValue(OptionsProperty, value);
    }

    private void OnToggleViewboxChecked(object sender, RoutedEventArgs e)
    {
        vbSample.Stretch = tgViewbox.IsChecked.GetValueOrDefault() ? Stretch.Uniform : Stretch.None;
    }

    private void OnToggleThemeChecked(object sender, RoutedEventArgs e)
    {
        ContentContainer.RequestedTheme = tgTheme.IsChecked.GetValueOrDefault() ? ElementTheme.Light : ElementTheme.Dark;
    }
}