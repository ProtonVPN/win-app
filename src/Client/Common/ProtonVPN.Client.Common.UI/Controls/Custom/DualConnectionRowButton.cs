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
using Microsoft.UI.Xaml.Controls.Primitives;
using ProtonVPN.Client.Common.UI.Controls.Bases;

namespace ProtonVPN.Client.Common.UI.Controls.Custom;

public class DualConnectionRowButton : DualConnectionRowButtonBase
{
    public static readonly DependencyProperty SecondaryCommandProperty =
        DependencyProperty.Register(nameof(SecondaryCommand), typeof(ICommand), typeof(DualConnectionRowButton), new PropertyMetadata(default));

    public static readonly DependencyProperty SecondaryCommandParameterProperty =
        DependencyProperty.Register(nameof(SecondaryCommandParameter), typeof(object), typeof(DualConnectionRowButton), new PropertyMetadata(default));

    public static readonly DependencyProperty SecondaryCommandFlyoutProperty =
        DependencyProperty.Register(nameof(SecondaryCommandFlyout), typeof(FlyoutBase), typeof(DualConnectionRowButton), new PropertyMetadata(default, OnSecondaryCommandFlyoutPropertyChanged));

    private const string FLYOUT_OPENED_VISUAL_STATE = "FlyoutOpened";
    private const string FLYOUT_CLOSED_VISUAL_STATE = "FlyoutClosed";

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

    public FlyoutBase SecondaryCommandFlyout
    {
        get => (FlyoutBase)GetValue(SecondaryCommandFlyoutProperty);
        set => SetValue(SecondaryCommandFlyoutProperty, value);
    }

    private static void OnSecondaryCommandFlyoutPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DualConnectionRowButton control)
        {
            if (e.OldValue is FlyoutBase oldFlyout)
            {
                oldFlyout.Opened -= control.OnFlyoutOpened;
                oldFlyout.Closed -= control.OnFlyoutClosed;
            }

            if (e.NewValue is FlyoutBase newFlyout)
            {
                newFlyout.Opened += control.OnFlyoutOpened;
                newFlyout.Closed += control.OnFlyoutClosed;
            }
        }
    }

    private void OnFlyoutOpened(object? sender, object e)
    {
        VisualStateManager.GoToState(this, FLYOUT_OPENED_VISUAL_STATE, false);
    }

    private void OnFlyoutClosed(object? sender, object e)
    {
        VisualStateManager.GoToState(this, FLYOUT_CLOSED_VISUAL_STATE, false);
    }
}
