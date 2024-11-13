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
using Windows.UI.Text;

namespace ProtonVPN.Client.Common.UI.Controls.Custom;

[TemplatePart(Name = "PART_HeaderContainer", Type = typeof(Panel))]
[TemplatePart(Name = "PART_ScrollViewer", Type = typeof(ScrollViewer))]
public class PageContentControl : ContentControl
{
    public static readonly DependencyProperty PageHeaderProperty =
        DependencyProperty.Register(nameof(PageHeader), typeof(object), typeof(PageContentControl), new PropertyMetadata(default, OnPageHeaderPropertyChanged));

    public static readonly DependencyProperty PageHeaderTemplateProperty =
        DependencyProperty.Register(nameof(PageHeaderTemplate), typeof(DataTemplate), typeof(PageContentControl), new PropertyMetadata(default));

    public static readonly DependencyProperty BackCommandProperty =
        DependencyProperty.Register(nameof(BackCommand), typeof(ICommand), typeof(PageContentControl), new PropertyMetadata(default, OnBackCommandPropertyChanged));

    public static readonly DependencyProperty BackCommandParameterProperty =
        DependencyProperty.Register(nameof(BackCommandParameter), typeof(object), typeof(PageContentControl), new PropertyMetadata(default, OnBackCommandPropertyChanged));

    public static readonly DependencyProperty IsBackButtonVisibleProperty =
        DependencyProperty.Register(nameof(IsBackButtonVisible), typeof(bool), typeof(PageContentControl), new PropertyMetadata(default));

    public static readonly DependencyProperty CanContentScrollProperty =
        DependencyProperty.Register(nameof(CanContentScroll), typeof(bool), typeof(PageContentControl), new PropertyMetadata(true));

    public static readonly DependencyProperty IsCompactProperty =
        DependencyProperty.Register(nameof(IsCompact), typeof(bool), typeof(PageContentControl), new PropertyMetadata(default));

    public static readonly DependencyProperty MaxContentWidthProperty =
        DependencyProperty.Register(nameof(MaxContentWidth), typeof(double), typeof(PageContentControl), new PropertyMetadata(double.MaxValue));

    public static readonly DependencyProperty PageHeaderFontSizeProperty =
        DependencyProperty.Register(nameof(PageHeaderFontSize), typeof(double), typeof(PageContentControl), new PropertyMetadata(default));

    public static readonly DependencyProperty PageHeaderFontWeightProperty =
        DependencyProperty.Register(nameof(PageHeaderFontWeight), typeof(FontWeight), typeof(PageContentControl), new PropertyMetadata(default));

    public static readonly DependencyProperty PageHeaderLineHeightProperty =
        DependencyProperty.Register(nameof(PageHeaderLineHeight), typeof(double), typeof(PageContentControl), new PropertyMetadata(default));

    protected Panel? PART_HeaderContainer;
    protected ScrollViewer? PART_ScrollViewer;

    public double PageHeaderFontSize
    {
        get => (int)GetValue(PageHeaderFontSizeProperty);
        set => SetValue(PageHeaderFontSizeProperty, value);
    }

    public FontWeight PageHeaderFontWeight
    {
        get => (FontWeight)GetValue(PageHeaderFontWeightProperty);
        set => SetValue(PageHeaderFontWeightProperty, value);
    }

    public double PageHeaderLineHeight
    {
        get => (double)GetValue(PageHeaderLineHeightProperty);
        set => SetValue(PageHeaderLineHeightProperty, value);
    }

    public double MaxContentWidth
    {
        get => (double)GetValue(MaxContentWidthProperty);
        set => SetValue(MaxContentWidthProperty, value);
    }

    public bool IsCompact
    {
        get => (bool)GetValue(IsCompactProperty);
        set => SetValue(IsCompactProperty, value);
    }

    public bool CanContentScroll
    {
        get => (bool)GetValue(CanContentScrollProperty);
        set => SetValue(CanContentScrollProperty, value);
    }

    public object BackCommandParameter
    {
        get => GetValue(BackCommandParameterProperty);
        set => SetValue(BackCommandParameterProperty, value);
    }

    public ICommand BackCommand
    {
        get => (ICommand)GetValue(BackCommandProperty);
        set => SetValue(BackCommandProperty, value);
    }

    public bool IsBackButtonVisible
    {
        get => (bool)GetValue(IsBackButtonVisibleProperty);
        set => SetValue(IsBackButtonVisibleProperty, value);
    }

    public DataTemplate PageHeaderTemplate
    {
        get => (DataTemplate)GetValue(PageHeaderTemplateProperty);
        set => SetValue(PageHeaderTemplateProperty, value);
    }

    public object PageHeader
    {
        get => GetValue(PageHeaderProperty);
        set => SetValue(PageHeaderProperty, value);
    }

    protected bool IsPageHeaderEmpty => PageHeader == null || string.IsNullOrEmpty(PageHeader.ToString());

    protected bool IsBackCommandEmpty => BackCommand == null || !BackCommand.CanExecute(BackCommandParameter);

    public PageContentControl()
    {
        DefaultStyleKey = typeof(PageContentControl);
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        PART_HeaderContainer = GetTemplateChild("PART_HeaderContainer") as Panel;
        InvalidateHeaderVisibility();

        PART_ScrollViewer = GetTemplateChild("PART_ScrollViewer") as ScrollViewer;

        // Auto focus when page is loaded
        Focus(FocusState.Programmatic);
    }

    protected virtual void InvalidateHeaderVisibility()
    {
        if (PART_HeaderContainer == null)
        {
            return;
        }

        PART_HeaderContainer.Visibility = IsPageHeaderEmpty && IsBackCommandEmpty
            ? Visibility.Collapsed
            : Visibility.Visible;
    }

    private static void OnPageHeaderPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PageContentControl control)
        {
            control.InvalidateHeaderVisibility();
        }
    }

    private static void OnBackCommandPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PageContentControl control)
        {
            control.InvalidateHeaderVisibility();
        }
    }

    public void ResetContentScroll()
    {
        if (PART_ScrollViewer != null && CanContentScroll)
        {
            PART_ScrollViewer.ScrollToVerticalOffset(0);
            PART_ScrollViewer.ScrollToHorizontalOffset(0);
        }
    }
}