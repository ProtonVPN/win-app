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
using Microsoft.UI.Xaml.Markup;

namespace ProtonVPN.Client.Common.UI.Controls;

[TemplatePart(Name = "PART_HeaderContainer", Type = typeof(FrameworkElement))]
[ContentProperty(Name = "PageContent")]
public sealed partial class PageContentControl : UserControl
{
    public static readonly DependencyProperty PageContentProperty =
        DependencyProperty.Register(nameof(PageContent), typeof(object), typeof(PageContentControl), new PropertyMetadata(default));

    public static readonly DependencyProperty PageContentTemplateProperty =
        DependencyProperty.Register(nameof(PageContentTemplate), typeof(DataTemplate), typeof(PageContentControl), new PropertyMetadata(default));

    public static readonly DependencyProperty PageHeaderProperty =
        DependencyProperty.Register(nameof(PageHeader), typeof(object), typeof(PageContentControl), new PropertyMetadata(default, OnPageHeaderPropertyChanged));

    public static readonly DependencyProperty PageHeaderTemplateProperty =
        DependencyProperty.Register(nameof(PageHeaderTemplate), typeof(DataTemplate), typeof(PageContentControl), new PropertyMetadata(default));

    public static readonly DependencyProperty BackCommandProperty =
        DependencyProperty.Register(nameof(BackCommand), typeof(ICommand), typeof(PageContentControl), new PropertyMetadata(default, OnBackCommandPropertyChanged));

    public static readonly DependencyProperty BackCommandParameterProperty =
        DependencyProperty.Register(nameof(BackCommandParameter), typeof(object), typeof(PageContentControl), new PropertyMetadata(default));

    public static readonly DependencyProperty CanContentScrollProperty =
        DependencyProperty.Register(nameof(CanContentScroll), typeof(bool), typeof(PageContentControl), new PropertyMetadata(true));

    public static readonly DependencyProperty IsCompactProperty =
        DependencyProperty.Register(nameof(IsCompact), typeof(bool), typeof(PageContentControl), new PropertyMetadata(default));

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

    public object PageContent
    {
        get => GetValue(PageContentProperty);
        set => SetValue(PageContentProperty, value);
    }

    public DataTemplate PageContentTemplate
    {
        get => (DataTemplate)GetValue(PageContentTemplateProperty);
        set => SetValue(PageContentTemplateProperty, value);
    }

    public ICommand BackCommand
    {
        get => (ICommand)GetValue(BackCommandProperty);
        set => SetValue(BackCommandProperty, value);
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

    public PageContentControl()
    {
        InitializeComponent();

        Loaded += OnPageContentControlLoaded;
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

    private void OnPageContentControlLoaded(object sender, RoutedEventArgs e)
    {
        // Auto focus when page is loaded
        Focus(FocusState.Programmatic);
    }

    private void InvalidateHeaderVisibility()
    {
        bool isPageHeaderEmpty = PageHeader == null || string.IsNullOrEmpty(PageHeader.ToString());
        bool isBackCommandEmpty = BackCommand == null || !BackCommand.CanExecute(BackCommandParameter);

        PART_HeaderContainer.Visibility = isPageHeaderEmpty && isBackCommandEmpty
            ? Visibility.Collapsed
            : Visibility.Visible;
    }
}