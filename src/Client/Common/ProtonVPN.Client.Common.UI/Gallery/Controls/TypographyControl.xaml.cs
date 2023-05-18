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

using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Text;

namespace ProtonVPN.Client.Common.UI.Gallery.Controls;

public sealed partial class TypographyControl : UserControl
{
    public static readonly DependencyProperty ExampleProperty =
        DependencyProperty.Register(nameof(Example), typeof(string), typeof(TypographyControl), new PropertyMetadata(""));

    public static readonly DependencyProperty LineHeightProperty =
        DependencyProperty.Register(nameof(LineHeight), typeof(double), typeof(TypographyControl), new PropertyMetadata(14.0));

    public static readonly DependencyProperty SizeProperty =
        DependencyProperty.Register(nameof(Size), typeof(double), typeof(TypographyControl), new PropertyMetadata(12.0));

    public static readonly DependencyProperty TextResourceNameProperty =
        DependencyProperty.Register(nameof(TextResourceName), typeof(string), typeof(TypographyControl), new PropertyMetadata(""));

    public static readonly DependencyProperty TextStyleProperty =
        DependencyProperty.Register(nameof(TextStyle), typeof(Style), typeof(TypographyControl), new PropertyMetadata(null));

    public static readonly DependencyProperty TextWeightProperty =
        DependencyProperty.Register(nameof(TextWeight), typeof(FontWeight), typeof(TypographyControl), new PropertyMetadata(FontWeights.Normal));

    public TypographyControl()
    {
        InitializeComponent();
    }

    public string Example
    {
        get => (string)GetValue(ExampleProperty);
        set => SetValue(ExampleProperty, value);
    }

    public double LineHeight
    {
        get => (double)GetValue(LineHeightProperty);
        set => SetValue(LineHeightProperty, value);
    }

    public double Size
    {
        get => (double)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    public string TextResourceName
    {
        get => (string)GetValue(TextResourceNameProperty);
        set => SetValue(TextResourceNameProperty, value);
    }

    public Style TextStyle
    {
        get => (Style)GetValue(TextStyleProperty);
        set => SetValue(TextStyleProperty, value);
    }

    public FontWeight TextWeight
    {
        get => (FontWeight)GetValue(TextWeightProperty);
        set => SetValue(TextWeightProperty, value);
    }

    private void CopyTextStyleToClipboardButton_Click(object sender, RoutedEventArgs e)
    {
        DataPackage package = new();
        package.SetText(TextResourceName);
        Clipboard.SetContent(package);
    }

    private void OnPointerEntered(object sender, PointerRoutedEventArgs e)
    {
        CopyTextStyleToClipboardButton.Visibility = Visibility.Visible;
    }

    private void OnPointerExited(object sender, PointerRoutedEventArgs e)
    {
        CopyTextStyleToClipboardButton.Visibility = Visibility.Collapsed;
    }
}