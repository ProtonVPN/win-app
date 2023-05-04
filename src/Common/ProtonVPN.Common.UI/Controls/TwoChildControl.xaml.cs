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

namespace ProtonVPN.Common.UI.Controls;

public sealed partial class TwoChildControl
{
    private const string NORMAL_VISUAL_STATE = "Normal";
    private const string PRIMARY_CONTENT_POINTER_OVER_VISUAL_STATE = "PrimaryContentPointerOver";
    private const string SECONDARY_CONTENT_POINTER_OVER_VISUAL_STATE = "SecondaryContentPointerOver";

    public TwoChildControl()
    {
        InitializeComponent();

        Loaded += RegisterEventHandlers;
    }

    private void RegisterEventHandlers(object sender, RoutedEventArgs eventArgs)
    {
        PrimaryContentContainer.PointerEntered += (_, _) =>
        {
            VisualStateManager.GoToState(this, PRIMARY_CONTENT_POINTER_OVER_VISUAL_STATE, false);
        };

        PrimaryContentContainer.PointerExited += (_, _) =>
        {
            VisualStateManager.GoToState(this, NORMAL_VISUAL_STATE, false);
        };

        SecondaryContent.PointerEntered += (_, _) =>
        {
            VisualStateManager.GoToState(this, SECONDARY_CONTENT_POINTER_OVER_VISUAL_STATE, false);
        };

        SecondaryContent.PointerExited += (_, _) =>
        {
            VisualStateManager.GoToState(this, NORMAL_VISUAL_STATE, false);
        };
    }

    public UIElement PrimaryContent
    {
        get => (UIElement)GetValue(PrimaryContentProperty);
        set => SetValue(PrimaryContentProperty, value);
    }

    public static readonly DependencyProperty PrimaryContentProperty =
        DependencyProperty.Register(nameof(PrimaryContent), typeof(UIElement), typeof(TwoChildControl),
            new PropertyMetadata(null, OnPrimaryContentChanged));

    private static void OnPrimaryContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        TwoChildControl control = (TwoChildControl)d;
        control.PrimaryContentPresenter.Content = (UIElement)e.NewValue;
    }

    public UIElement SecondaryContent
    {
        get => (UIElement)GetValue(SecondaryContentProperty);
        set => SetValue(SecondaryContentProperty, value);
    }

    public static readonly DependencyProperty SecondaryContentProperty =
        DependencyProperty.Register(nameof(SecondaryContent), typeof(UIElement), typeof(TwoChildControl),
            new PropertyMetadata(null, OnSecondaryContentChanged));

    private static void OnSecondaryContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        TwoChildControl control = (TwoChildControl)d;
        control.SecondaryContentPresenter.Content = (UIElement)e.NewValue;
    }

    public UIElement PrimaryHoverContent
    {
        get => (UIElement)GetValue(PrimaryHoverContentProperty);
        set => SetValue(PrimaryHoverContentProperty, value);
    }

    public static readonly DependencyProperty PrimaryHoverContentProperty =
        DependencyProperty.Register(nameof(PrimaryHoverContent), typeof(UIElement), typeof(TwoChildControl),
            new PropertyMetadata(null, OnPrimaryHoverContentChanged));

    private static void OnPrimaryHoverContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        TwoChildControl control = (TwoChildControl)d;
        control.PrimaryHoverContentPresenter.Content = (UIElement)e.NewValue;
    }
}