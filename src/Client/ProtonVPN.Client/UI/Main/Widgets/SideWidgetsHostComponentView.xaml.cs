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
using Microsoft.UI.Xaml.Input;
using ProtonVPN.Client.Common.UI.Controls.Custom;
using ProtonVPN.Client.Common.UI.Extensions;
using ProtonVPN.Client.Core.Bases;
using Windows.System;

namespace ProtonVPN.Client.UI.Main.Widgets;

public sealed partial class SideWidgetsHostComponentView : IContextAware
{
    public static readonly DependencyProperty IsHomeDisplayedProperty =
        DependencyProperty.Register(nameof(IsHomeDisplayed), typeof(bool), typeof(SideWidgetsHostComponentView), new PropertyMetadata(default, OnIsHomeDisplayedPropertyChanged));

    private Button? _lastKeyboardInvokedButton;
    private bool _shouldRestoreFocus;

    public bool IsHomeDisplayed
    {
        get => (bool)GetValue(IsHomeDisplayedProperty);
        set => SetValue(IsHomeDisplayedProperty, value);
    }

    public SideWidgetsHostComponentViewModel ViewModel { get; }

    public SideWidgetsHostComponentView()
    {
        ViewModel = App.GetService<SideWidgetsHostComponentViewModel>();

        InitializeComponent();
        Loaded += OnLoaded;
    }

    public object GetContext()
    {
        return ViewModel;
    }

    private static void OnIsHomeDisplayedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SideWidgetsHostComponentView view && e.NewValue is bool isHomeDisplayed && isHomeDisplayed)
        {
            view.RestoreFocusIfNeeded();
        }
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        TrackWidgetButtonsKeyboardInput();
    }

    private void RestoreFocusIfNeeded()
    {
        if (_shouldRestoreFocus && _lastKeyboardInvokedButton != null)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                _lastKeyboardInvokedButton?.Focus(FocusState.Keyboard);
                ClearTrackedFocus();
            });
        }
    }

    private void TrackWidgetButtonsKeyboardInput()
    {
        TrackItemsControlWidgetButtons(HeaderWidgetsItemsControl, ViewModel.HeaderWidgets);
        TrackItemsControlWidgetButtons(FooterWidgetsItemsControl, ViewModel.FooterWidgets);
    }

    private void TrackItemsControlWidgetButtons<T>(ItemsControl itemsControl, IEnumerable<T> items)
    {
        foreach (T? item in items)
        {
            DependencyObject container = itemsControl.ContainerFromItem(item);
            if (container is FrameworkElement containerElement)
            {
                IEnumerable<WidgetButton> widgetButtons = containerElement.FindChildrenOfType<WidgetButton>();
                foreach (WidgetButton button in widgetButtons)
                {
                    button.PreviewKeyDown += OnWidgetButtonPreviewKeyDown;
                }
            }
        }
    }

    private void OnWidgetButtonPreviewKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Enter || e.Key == VirtualKey.Space)
        {
            if (sender is Button button)
            {
                _lastKeyboardInvokedButton = button;
                _shouldRestoreFocus = true;
            }
        }
    }

    private void ClearTrackedFocus()
    {
        _lastKeyboardInvokedButton = null;
        _shouldRestoreFocus = false;
    }
}