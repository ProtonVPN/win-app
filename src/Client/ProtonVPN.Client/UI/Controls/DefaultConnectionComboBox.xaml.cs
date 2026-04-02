/*
 * Copyright (c) 2025 Proton AG
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
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using ProtonVPN.Client.Common.UI.Extensions;
using Windows.Foundation;

namespace ProtonVPN.Client.UI.Controls;

public sealed partial class DefaultConnectionComboBox : UserControl
{
    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
        nameof(ItemsSource),
        typeof(object),
        typeof(DefaultConnectionComboBox),
        new PropertyMetadata(null, OnItemsSourceChanged));

    public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
        nameof(SelectedItem),
        typeof(object),
        typeof(DefaultConnectionComboBox),
        new PropertyMetadata(null, OnSelectedItemChanged));

    public static readonly DependencyProperty DropdownAnchorAlignmentProperty = DependencyProperty.Register(
        nameof(DropdownAnchorAlignment),
        typeof(DropdownAnchorAlignment),
        typeof(DefaultConnectionComboBox),
        new PropertyMetadata(DropdownAnchorAlignment.Default));

    private Popup? _popup;

    private FrameworkElement? _anchorElement;

    public object ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public object SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public DropdownAnchorAlignment DropdownAnchorAlignment
    {
        get => (DropdownAnchorAlignment)GetValue(DropdownAnchorAlignmentProperty);
        set => SetValue(DropdownAnchorAlignmentProperty, value);
    }

    public DefaultConnectionComboBox()
    {
        InitializeComponent();
        InternalComboBox.SelectionChanged += OnSelectionChanged;
        Loaded += OnLoaded;
    }

    public event SelectionChangedEventHandler? SelectionChanged;

    public void Open(FrameworkElement? anchorElement = null)
    {
        _anchorElement = anchorElement;
        InternalComboBox.IsDropDownOpen = true;
    }

    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DefaultConnectionComboBox control)
        {
            control.InternalComboBox.ItemsSource = e.NewValue;
        }
    }

    private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DefaultConnectionComboBox control && control.InternalComboBox.SelectedItem != e.NewValue)
        {
            control.InternalComboBox.SelectedItem = e.NewValue;
        }
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _popup = InternalComboBox.FindChildrenOfType<Popup>()?.FirstOrDefault();
        if (_popup != null)
        {
            _popup.Opened += OnPopupOpened;
        }
    }

    private void OnPopupOpened(object? sender, object e)
    {
        if (DropdownAnchorAlignment != DropdownAnchorAlignment.Default && _popup?.Child is FrameworkElement popupContent)
        {
            ApplyPopupPlacement(popupContent);
        }
    }

    private void ApplyPopupPlacement(FrameworkElement popupContent)
    {
        FrameworkElement targetElement = _anchorElement ?? InternalComboBox;

        GeneralTransform transform = targetElement.TransformToVisual(InternalComboBox);
        Point targetPosition = transform.TransformPoint(new Point(0, 0));

        _popup!.VerticalOffset = CalculateVerticalOffset(targetPosition.Y, targetElement.ActualHeight);
        _popup.HorizontalOffset = CalculateHorizontalOffset(targetPosition.X, targetElement.ActualWidth, popupContent);
    }

    private double CalculateVerticalOffset(double targetY, double targetHeight)
    {
        return DropdownAnchorAlignment switch
        {
            DropdownAnchorAlignment.BottomRight => targetY + targetHeight,
            DropdownAnchorAlignment.TopRight => targetY,
            _ => 0
        };
    }

    private double CalculateHorizontalOffset(double targetX, double targetWidth, FrameworkElement popupContent)
    {
        double popupWidth = popupContent.ActualWidth > 0 ? popupContent.ActualWidth : popupContent.DesiredSize.Width;
        return popupWidth > 0 ? targetX + targetWidth - popupWidth : 0;
    }

    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        SelectedItem = InternalComboBox.SelectedItem;
        SelectionChanged?.Invoke(this, e);
    }
}