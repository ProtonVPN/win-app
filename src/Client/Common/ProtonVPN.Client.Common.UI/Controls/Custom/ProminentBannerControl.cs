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
using Microsoft.UI.Xaml.Media;
using ProtonVPN.Client.Common.Enums;

namespace ProtonVPN.Client.Common.UI.Controls.Custom;

public class ProminentBannerControl : Control
{
    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register(nameof(Header), typeof(string), typeof(ProminentBannerControl), new PropertyMetadata(default));

    public static readonly DependencyProperty DescriptionProperty =
        DependencyProperty.Register(nameof(Description), typeof(string), typeof(ProminentBannerControl), new PropertyMetadata(default));

    public static readonly DependencyProperty FooterProperty =
        DependencyProperty.Register(nameof(Footer), typeof(string), typeof(ProminentBannerControl), new PropertyMetadata(default));

    public static readonly DependencyProperty ActionCommandProperty =
        DependencyProperty.Register(nameof(ActionCommand), typeof(ICommand), typeof(ProminentBannerControl), new PropertyMetadata(default));

    public static readonly DependencyProperty ActionButtonTextProperty =
        DependencyProperty.Register(nameof(ActionButtonText), typeof(string), typeof(ProminentBannerControl), new PropertyMetadata(default));

    public static readonly DependencyProperty IsActionButtonVisibleProperty =
        DependencyProperty.Register(nameof(IsActionButtonVisible), typeof(bool), typeof(ProminentBannerControl), new PropertyMetadata(default));

    public static readonly DependencyProperty DismissCommandProperty =
        DependencyProperty.Register(nameof(DismissCommand), typeof(ICommand), typeof(ProminentBannerControl), new PropertyMetadata(default));

    public static readonly DependencyProperty DismissButtonTextProperty =
        DependencyProperty.Register(nameof(DismissButtonText), typeof(string), typeof(ProminentBannerControl), new PropertyMetadata(default));

    public static readonly DependencyProperty IsDismissButtonVisibleProperty =
        DependencyProperty.Register(nameof(IsDismissButtonVisible), typeof(bool), typeof(ProminentBannerControl), new PropertyMetadata(default));

    public static readonly DependencyProperty IconSourceProperty =
        DependencyProperty.Register(nameof(IconSource), typeof(ImageSource), typeof(ProminentBannerControl), new PropertyMetadata(default));

    public static readonly DependencyProperty SmallIllustrationSourceProperty =
        DependencyProperty.Register(nameof(SmallIllustrationSource), typeof(ImageSource), typeof(ProminentBannerControl), new PropertyMetadata(default));

    public static readonly DependencyProperty LargeIllustrationSourceProperty =
        DependencyProperty.Register(nameof(LargeIllustrationSource), typeof(ImageSource), typeof(ProminentBannerControl), new PropertyMetadata(default));

    public static readonly DependencyProperty IsExpandedProperty =
        DependencyProperty.Register(nameof(IsExpanded), typeof(bool), typeof(ProminentBannerControl), new PropertyMetadata(true));

    public static readonly DependencyProperty BannerStyleProperty =
        DependencyProperty.Register(nameof(BannerStyle), typeof(ProminentBannerStyle), typeof(ProminentBannerControl), new PropertyMetadata(default));

    public static readonly DependencyProperty TooltipTextProperty =
        DependencyProperty.Register(nameof(TooltipText), typeof(string), typeof(ProminentBannerControl), new PropertyMetadata(default));

    public string Header
    {
        get => (string)GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public string Description
    {
        get => (string)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public string Footer
    {
        get => (string)GetValue(FooterProperty);
        set => SetValue(FooterProperty, value);
    }

    public ICommand ActionCommand
    {
        get => (ICommand)GetValue(ActionCommandProperty);
        set => SetValue(ActionCommandProperty, value);
    }

    public string ActionButtonText
    {
        get => (string)GetValue(ActionButtonTextProperty);
        set => SetValue(ActionButtonTextProperty, value);
    }

    public bool IsActionButtonVisible
    {
        get => (bool)GetValue(IsActionButtonVisibleProperty);
        set => SetValue(IsActionButtonVisibleProperty, value);
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

    public ImageSource IconSource
    {
        get => (ImageSource)GetValue(IconSourceProperty);
        set => SetValue(IconSourceProperty, value);
    }

    public ImageSource SmallIllustrationSource
    {
        get => (ImageSource)GetValue(SmallIllustrationSourceProperty);
        set => SetValue(SmallIllustrationSourceProperty, value);
    }

    public ImageSource LargeIllustrationSource
    {
        get => (ImageSource)GetValue(LargeIllustrationSourceProperty);
        set => SetValue(LargeIllustrationSourceProperty, value);
    }

    public bool IsExpanded
    {
        get => (bool)GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    public ProminentBannerStyle BannerStyle
    {
        get => (ProminentBannerStyle)GetValue(BannerStyleProperty);
        set => SetValue(BannerStyleProperty, value);
    }

    public string TooltipText
    {
        get => (string)GetValue(TooltipTextProperty);
        set
        {
            if (string.IsNullOrEmpty(value))
            {
                SetValue(TooltipTextProperty, null);
            }
            else
            {
                SetValue(TooltipTextProperty, value);
            }
        }
    }
}