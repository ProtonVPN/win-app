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

using System.Windows.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ProtonVPN.Client.UI.Countries.Controls;

public class ItemsListControlBase : UserControl
{
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
        nameof(Title),
        typeof(string),
        typeof(ItemsListControlBase),
        new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty InfoLabelProperty = DependencyProperty.Register(
        nameof(InfoLabel),
        typeof(string),
        typeof(ItemsListControlBase),
        new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty ShowInfoOverlayCommandProperty = DependencyProperty.Register(
        nameof(ShowInfoOverlayCommand),
        typeof(ICommand),
        typeof(ItemsListControlBase),
        new PropertyMetadata(null));

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string InfoLabel
    {
        get => (string)GetValue(InfoLabelProperty);
        set => SetValue(InfoLabelProperty, value);
    }

    public ICommand? ShowInfoOverlayCommand
    {
        get => (ICommand)GetValue(ShowInfoOverlayCommandProperty);
        set => SetValue(ShowInfoOverlayCommandProperty, value);
    }
}