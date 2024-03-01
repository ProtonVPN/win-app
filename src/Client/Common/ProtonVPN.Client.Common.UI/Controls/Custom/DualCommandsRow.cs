﻿/*
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
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using ProtonVPN.Client.Common.UI.Automation;

namespace ProtonVPN.Client.Common.UI.Controls.Custom;

public class DualCommandsRow : ContentControl
{
    public static readonly DependencyProperty IsContentEnabledProperty =
        DependencyProperty.Register(nameof(IsContentEnabled), typeof(bool), typeof(DualCommandsRow), new PropertyMetadata(true));

    public static readonly DependencyProperty PrimaryCommandContentProperty =
        DependencyProperty.Register(nameof(PrimaryCommandContent), typeof(object), typeof(DualCommandsRow), new PropertyMetadata(default));

    public static readonly DependencyProperty PrimaryCommandContentTemplateProperty =
        DependencyProperty.Register(nameof(PrimaryCommandContentTemplate), typeof(DataTemplate), typeof(DualCommandsRow), new PropertyMetadata(default));

    public static readonly DependencyProperty PrimaryCommandContentTemplateSelectorProperty =
        DependencyProperty.Register(nameof(PrimaryCommandContentTemplateSelector), typeof(DataTemplateSelector), typeof(DualCommandsRow), new PropertyMetadata(default));

    public static readonly DependencyProperty PrimaryCommandProperty =
        DependencyProperty.Register(nameof(PrimaryCommand), typeof(ICommand), typeof(DualCommandsRow), new PropertyMetadata(default));

    public static readonly DependencyProperty PrimaryCommandParameterProperty =
        DependencyProperty.Register(nameof(PrimaryCommandParameter), typeof(object), typeof(DualCommandsRow), new PropertyMetadata(default));

    public static readonly DependencyProperty PrimaryCommandToolTipProperty =
        DependencyProperty.Register(nameof(PrimaryCommandToolTip), typeof(object), typeof(DualCommandsRow), new PropertyMetadata(default));

    public static readonly DependencyProperty PrimaryCommandAutomationIdProperty =
        DependencyProperty.Register(nameof(PrimaryCommandAutomationId), typeof(string), typeof(DualCommandsRow), new PropertyMetadata(default));

    public static readonly DependencyProperty PrimaryCommandAutomationNameProperty =
        DependencyProperty.Register(nameof(PrimaryCommandAutomationName), typeof(string), typeof(DualCommandsRow), new PropertyMetadata(default));

    public static readonly DependencyProperty SecondaryCommandTextProperty =
        DependencyProperty.Register(nameof(SecondaryCommandText), typeof(string), typeof(DualCommandsRow), new PropertyMetadata(default, OnSecondaryCommandTextPropertyChanged));

    public static readonly DependencyProperty SecondaryCommandIconProperty =
        DependencyProperty.Register(nameof(SecondaryCommandIcon), typeof(IconElement), typeof(DualCommandsRow), new PropertyMetadata(default, OnSecondaryCommandIconPropertyChanged));

    public static readonly DependencyProperty SecondaryCommandFlyoutProperty =
        DependencyProperty.Register(nameof(SecondaryCommandFlyout), typeof(FlyoutBase), typeof(DualCommandsRow), new PropertyMetadata(default));

    public static readonly DependencyProperty SecondaryCommandProperty =
        DependencyProperty.Register(nameof(SecondaryCommand), typeof(ICommand), typeof(DualCommandsRow), new PropertyMetadata(default));

    public static readonly DependencyProperty SecondaryCommandParameterProperty =
        DependencyProperty.Register(nameof(SecondaryCommandParameter), typeof(object), typeof(DualCommandsRow), new PropertyMetadata(default));

    public static readonly DependencyProperty SecondaryCommandToolTipProperty =
        DependencyProperty.Register(nameof(SecondaryCommandToolTip), typeof(object), typeof(DualCommandsRow), new PropertyMetadata(default));

    public static readonly DependencyProperty SecondaryCommandAutomationIdProperty =
        DependencyProperty.Register(nameof(SecondaryCommandAutomationId), typeof(string), typeof(DualCommandsRow), new PropertyMetadata(default));

    public static readonly DependencyProperty SecondaryCommandAutomationNameProperty =
        DependencyProperty.Register(nameof(SecondaryCommandAutomationName), typeof(string), typeof(DualCommandsRow), new PropertyMetadata(default));

    public static readonly DependencyProperty IsSecondaryCommandVisibleProperty =
        DependencyProperty.Register(nameof(IsSecondaryCommandVisible), typeof(bool), typeof(DualCommandsRow), new PropertyMetadata(true));

    protected UIElement PART_SecondaryContainer;

    public bool IsContentEnabled
    {
        get => (bool)GetValue(IsContentEnabledProperty);
        set => SetValue(IsContentEnabledProperty, value);
    }

    public object PrimaryCommandContent
    {
        get => GetValue(PrimaryCommandContentProperty);
        set => SetValue(PrimaryCommandContentProperty, value);
    }

    public DataTemplate PrimaryCommandContentTemplate
    {
        get => (DataTemplate)GetValue(PrimaryCommandContentTemplateProperty);
        set => SetValue(PrimaryCommandContentTemplateProperty, value);
    }

    public DataTemplateSelector PrimaryCommandContentTemplateSelector
    {
        get => (DataTemplateSelector)GetValue(PrimaryCommandContentTemplateSelectorProperty);
        set => SetValue(PrimaryCommandContentTemplateSelectorProperty, value);
    }

    public ICommand PrimaryCommand
    {
        get => (ICommand)GetValue(PrimaryCommandProperty);
        set => SetValue(PrimaryCommandProperty, value);
    }

    public object PrimaryCommandParameter
    {
        get => GetValue(PrimaryCommandParameterProperty);
        set => SetValue(PrimaryCommandParameterProperty, value);
    }

    public object PrimaryCommandToolTip
    {
        get => GetValue(PrimaryCommandToolTipProperty);
        set => SetValue(PrimaryCommandToolTipProperty, value);
    }

    public string PrimaryCommandAutomationId
    {
        get => (string)GetValue(PrimaryCommandAutomationIdProperty);
        set => SetValue(PrimaryCommandAutomationIdProperty, value);
    }

    public string PrimaryCommandAutomationName
    {
        get => (string)GetValue(PrimaryCommandAutomationNameProperty);
        set => SetValue(PrimaryCommandAutomationNameProperty, value);
    }

    public string SecondaryCommandText
    {
        get => (string)GetValue(SecondaryCommandTextProperty);
        set => SetValue(SecondaryCommandTextProperty, value);
    }

    public IconElement SecondaryCommandIcon
    {
        get => (IconElement)GetValue(SecondaryCommandIconProperty);
        set => SetValue(SecondaryCommandIconProperty, value);
    }

    public FlyoutBase SecondaryCommandFlyout
    {
        get => (FlyoutBase)GetValue(SecondaryCommandFlyoutProperty);
        set => SetValue(SecondaryCommandFlyoutProperty, value);
    }

    public ICommand SecondaryCommand
    {
        get => (ICommand)GetValue(SecondaryCommandProperty);
        set => SetValue(SecondaryCommandProperty, value);
    }

    public object SecondaryCommandParameter
    {
        get => GetValue(SecondaryCommandParameterProperty);
        set => SetValue(SecondaryCommandParameterProperty, value);
    }

    public object SecondaryCommandToolTip
    {
        get => GetValue(SecondaryCommandToolTipProperty);
        set => SetValue(SecondaryCommandToolTipProperty, value);
    }

    public string SecondaryCommandAutomationId
    {
        get => (string)GetValue(SecondaryCommandAutomationIdProperty);
        set => SetValue(SecondaryCommandAutomationIdProperty, value);
    }

    public string SecondaryCommandAutomationName
    {
        get => (string)GetValue(SecondaryCommandAutomationNameProperty);
        set => SetValue(SecondaryCommandAutomationNameProperty, value);
    }

    public bool IsSecondaryCommandVisible
    {
        get => (bool)GetValue(IsSecondaryCommandVisibleProperty);
        set => SetValue(IsSecondaryCommandVisibleProperty, value);
    }

    public DualCommandsRow()
    {
        DefaultStyleKey = typeof(DualCommandsRow);
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        PART_SecondaryContainer = GetTemplateChild("SecondaryContainer") as UIElement;
        InvalidateSecondaryContainerVisibility();
    }

    protected override AutomationPeer OnCreateAutomationPeer()
    {
        return new CustomControlAutomationPeer(this);
    }

    private static void OnSecondaryCommandTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DualCommandsRow control)
        {
            control.InvalidateSecondaryContainerVisibility();
        }
    }

    private static void OnSecondaryCommandIconPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DualCommandsRow control)
        {
            control.InvalidateSecondaryContainerVisibility();
        }
    }

    private void InvalidateSecondaryContainerVisibility()
    {
        if (PART_SecondaryContainer != null)
        {
            PART_SecondaryContainer.Visibility =
                !string.IsNullOrEmpty(SecondaryCommandText) ||
                (SecondaryCommandIcon is not null && SecondaryCommandIcon.Visibility == Visibility.Visible)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
        }
    }
}