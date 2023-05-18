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
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Markup;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ProtonVPN.Client.Common.UI.Controls;

[ContentProperty(Name = "RowContent")]
public sealed partial class DualCommandsRowControl : UserControl
{
    public static readonly DependencyProperty RowContentProperty =
        DependencyProperty.Register(nameof(RowContent), typeof(UIElement), typeof(DualCommandsRowControl), new PropertyMetadata(default));

    public static readonly DependencyProperty PrimaryCommandProperty =
        DependencyProperty.Register(nameof(PrimaryCommand), typeof(ICommand), typeof(DualCommandsRowControl), new PropertyMetadata(default));

    public static readonly DependencyProperty PrimaryCommandParameterProperty =
        DependencyProperty.Register(nameof(PrimaryCommandParameter), typeof(object), typeof(DualCommandsRowControl), new PropertyMetadata(default));

    public static readonly DependencyProperty PrimaryCommandTextProperty =
        DependencyProperty.Register(nameof(PrimaryCommandText), typeof(string), typeof(DualCommandsRowControl), new PropertyMetadata(default));

    public static readonly DependencyProperty SecondaryCommandProperty =
        DependencyProperty.Register(nameof(SecondaryCommand), typeof(ICommand), typeof(DualCommandsRowControl), new PropertyMetadata(default));

    public static readonly DependencyProperty SecondaryCommandParameterProperty =
        DependencyProperty.Register(nameof(SecondaryCommandParameter), typeof(object), typeof(DualCommandsRowControl), new PropertyMetadata(default));

    public static readonly DependencyProperty SecondaryCommandTextProperty =
        DependencyProperty.Register(nameof(SecondaryCommandText), typeof(string), typeof(DualCommandsRowControl), new PropertyMetadata(default));

    public static readonly DependencyProperty SecondaryCommandIconProperty =
        DependencyProperty.Register(nameof(SecondaryCommandIcon), typeof(UIElement), typeof(DualCommandsRowControl), new PropertyMetadata(default));

    public static readonly DependencyProperty SecondaryCommandFlyoutProperty =
        DependencyProperty.Register(nameof(SecondaryCommandFlyout), typeof(FlyoutBase), typeof(DualCommandsRowControl), new PropertyMetadata(default));

    public UIElement RowContent
    {
        get => (UIElement)GetValue(RowContentProperty);
        set => SetValue(RowContentProperty, value);
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

    public string PrimaryCommandText
    {
        get => (string)GetValue(PrimaryCommandTextProperty);
        set => SetValue(PrimaryCommandTextProperty, value);
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

    public string SecondaryCommandText
    {
        get => (string)GetValue(SecondaryCommandTextProperty);
        set => SetValue(SecondaryCommandTextProperty, value);
    }

    public UIElement SecondaryCommandIcon
    {
        get => (UIElement)GetValue(SecondaryCommandIconProperty);
        set => SetValue(SecondaryCommandIconProperty, value);
    }

    public FlyoutBase SecondaryCommandFlyout
    {
        get => (FlyoutBase)GetValue(SecondaryCommandFlyoutProperty);
        set => SetValue(SecondaryCommandFlyoutProperty, value);
    }

    public DualCommandsRowControl()
    {
        InitializeComponent();
    }
}