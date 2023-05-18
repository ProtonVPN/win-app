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

using System.Linq;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Windows.ApplicationModel.DataTransfer;

namespace ProtonVPN.Client.Common.UI.Gallery.Controls;

public sealed partial class ColorsControl : UserControl
{
    public static readonly DependencyProperty ColorBrushProperty =
        DependencyProperty.Register(nameof(ColorBrush), typeof(Brush), typeof(ColorsControl), new PropertyMetadata(new SolidColorBrush(Colors.Transparent)));

    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register(nameof(Header), typeof(string), typeof(TypographyControl), new PropertyMetadata(""));

    public static readonly DependencyProperty ResourceNameProperty =
        DependencyProperty.Register(nameof(ResourceName), typeof(string), typeof(TypographyControl), new PropertyMetadata(""));

    public ColorsControl()
    {
        InitializeComponent();
    }

    public Brush ColorBrush
    {
        get => (Brush)GetValue(ColorBrushProperty);
        set => SetValue(ColorBrushProperty, value);
    }

    public string ColorCode =>
        ColorBrush switch
        {
            SolidColorBrush solidColorBrush => solidColorBrush.Color.ToString(),
            GradientBrush gradientBrush => string.Join(" - ", gradientBrush.GradientStops.Select(c => c.Color.ToString())),
            _ => string.Empty,
        };

    public string Header
    {
        get => (string)GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public string ResourceName
    {
        get => (string)GetValue(ResourceNameProperty);
        set => SetValue(ResourceNameProperty, value);
    }

    private void CopyBrushStyleToClipboardButton_Click(object sender, RoutedEventArgs e)
    {
        DataPackage package = new();
        package.SetText(ResourceName);
        Clipboard.SetContent(package);
    }

    private void OnPointerEntered(object sender, PointerRoutedEventArgs e)
    {
        CopyBrushStyleToClipboardButton.Visibility = Visibility.Visible;
    }

    private void OnPointerExited(object sender, PointerRoutedEventArgs e)
    {
        CopyBrushStyleToClipboardButton.Visibility = Visibility.Collapsed;
    }
}