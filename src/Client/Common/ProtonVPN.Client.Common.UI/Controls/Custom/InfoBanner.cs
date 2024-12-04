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

using System.Windows.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace ProtonVPN.Client.Common.UI.Controls.Custom;

public class InfoBanner : Control
{
    public static readonly DependencyProperty IllustrationSourceProperty =
        DependencyProperty.Register(nameof(IllustrationSource), typeof(ImageSource), typeof(InfoBanner), new PropertyMetadata(default));

    public static readonly DependencyProperty DescriptionProperty =
        DependencyProperty.Register(nameof(Description), typeof(string), typeof(InfoBanner), new PropertyMetadata(default));

    public static readonly DependencyProperty DismissCommandProperty =
        DependencyProperty.Register(nameof(DismissCommand), typeof(ICommand), typeof(InfoBanner), new PropertyMetadata(default));

    public static readonly DependencyProperty DismissButtonTextProperty =
        DependencyProperty.Register(nameof(DismissButtonText), typeof(string), typeof(InfoBanner), new PropertyMetadata(default));

    public static readonly DependencyProperty IsDismissButtonVisibleProperty =
        DependencyProperty.Register(nameof(IsDismissButtonVisible), typeof(bool), typeof(InfoBanner), new PropertyMetadata(true));

    public ImageSource IllustrationSource
    {
        get => (ImageSource)GetValue(IllustrationSourceProperty);
        set => SetValue(IllustrationSourceProperty, value);
    }

    public string Description
    {
        get => (string)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public ICommand DismissCommand
    {
        get => (ICommand)GetValue(DismissCommandProperty);
        set => SetValue(DismissCommandProperty, value);
    }

    public string DismissButtonText
    {
        get => (string)GetValue(DismissButtonTextProperty);
        set => SetValue(DismissButtonTextProperty, value);
    }

    public bool IsDismissButtonVisible
    {
        get => (bool)GetValue(IsDismissButtonVisibleProperty);
        set => SetValue(IsDismissButtonVisibleProperty, value);
    }
}