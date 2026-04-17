/*
 * Copyright (c) 2026 Proton AG
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

using System.Collections;
using System.Windows.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;

namespace ProtonVPN.Client.UI.Controls;

public sealed partial class ExcludableLocationSelector : UserControl
{
    public static readonly DependencyProperty PlaceholderTextProperty = DependencyProperty.Register(
        nameof(PlaceholderText),
        typeof(string),
        typeof(ExcludableLocationSelector),
        new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty SearchPlaceholderTextProperty = DependencyProperty.Register(
        nameof(SearchPlaceholderText),
        typeof(string),
        typeof(ExcludableLocationSelector),
        new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty EmptyResultsTextProperty = DependencyProperty.Register(
        nameof(EmptyResultsText),
        typeof(string),
        typeof(ExcludableLocationSelector),
        new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty HasNoResultsProperty = DependencyProperty.Register(
        nameof(HasNoResults),
        typeof(bool),
        typeof(ExcludableLocationSelector),
        new PropertyMetadata(false));

    public static readonly DependencyProperty IsFlyoutOpenProperty = DependencyProperty.Register(
        nameof(IsFlyoutOpen),
        typeof(bool),
        typeof(ExcludableLocationSelector),
        new PropertyMetadata(false, OnIsFlyoutOpenChanged));

    public static readonly DependencyProperty SearchTextProperty = DependencyProperty.Register(
        nameof(SearchText),
        typeof(string),
        typeof(ExcludableLocationSelector),
        new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty LocationsSourceProperty = DependencyProperty.Register(
        nameof(LocationsSource),
        typeof(IEnumerable),
        typeof(ExcludableLocationSelector),
        new PropertyMetadata(null));

    public static readonly DependencyProperty OpenCommandProperty = DependencyProperty.Register(
        nameof(OpenCommand),
        typeof(ICommand),
        typeof(ExcludableLocationSelector),
        new PropertyMetadata(null));

    public static readonly DependencyProperty CloseCommandProperty = DependencyProperty.Register(
        nameof(CloseCommand),
        typeof(ICommand),
        typeof(ExcludableLocationSelector),
        new PropertyMetadata(null));

    public string PlaceholderText
    {
        get => (string)GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }

    public string SearchPlaceholderText
    {
        get => (string)GetValue(SearchPlaceholderTextProperty);
        set => SetValue(SearchPlaceholderTextProperty, value);
    }

    public string EmptyResultsText
    {
        get => (string)GetValue(EmptyResultsTextProperty);
        set => SetValue(EmptyResultsTextProperty, value);
    }

    public bool HasNoResults
    {
        get => (bool)GetValue(HasNoResultsProperty);
        set => SetValue(HasNoResultsProperty, value);
    }

    public bool IsFlyoutOpen
    {
        get => (bool)GetValue(IsFlyoutOpenProperty);
        set => SetValue(IsFlyoutOpenProperty, value);
    }

    public string SearchText
    {
        get => (string)GetValue(SearchTextProperty);
        set => SetValue(SearchTextProperty, value);
    }

    public IEnumerable? LocationsSource
    {
        get => (IEnumerable?)GetValue(LocationsSourceProperty);
        set => SetValue(LocationsSourceProperty, value);
    }

    public ICommand? OpenCommand
    {
        get => (ICommand?)GetValue(OpenCommandProperty);
        set => SetValue(OpenCommandProperty, value);
    }

    public ICommand? CloseCommand
    {
        get => (ICommand?)GetValue(CloseCommandProperty);
        set => SetValue(CloseCommandProperty, value);
    }

    public ExcludableLocationSelector()
    {
        InitializeComponent();
    }

    private static void OnIsFlyoutOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ExcludableLocationSelector selector && e.NewValue is bool isOpen)
        {
            if (isOpen)
            {
                FlyoutBase.ShowAttachedFlyout(selector.SelectorButton);
            }
            else
            {
                selector.LocationFlyout.Hide();
            }
        }
    }

    private void OnSelectorButtonClick(object sender, RoutedEventArgs e)
    {
        OpenCommand?.Execute(null);
    }

    private void OnFlyoutClosed(object sender, object e)
    {
        CloseCommand?.Execute(null);
    }
}