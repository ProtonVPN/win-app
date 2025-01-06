/*
 * Copyright (c) 2024 Proton AG
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

namespace ProtonVPN.Client.Common.UI.Controls.Custom;

public class DetailRowButton : Button
{
    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register(nameof(Header), typeof(object), typeof(DetailRowButton), new PropertyMetadata(null));

    public object Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public DetailRowButton()
    {
        IsEnabledChanged += OnIsEnabledChanged;
    }

    private void OnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (!IsEnabled)
        {
            CloseFlyout();
        }
    }

    private void CloseFlyout()
    {
        if (Flyout?.IsOpen == true)
        {
            Flyout.Hide();
        }
    }
}

public class DetailRowButtonVisualStateManager : VisualStateManager
{
    protected override bool GoToStateCore(Control control, FrameworkElement templateRoot, string stateName, VisualStateGroup group, VisualState state, bool useTransitions)
    {
        if (control is not DetailRowButton drb)
        {
            return base.GoToStateCore(control, templateRoot, stateName, group, state, useTransitions);
        }

        if (drb.Flyout != null && drb.Flyout.IsOpen)
        {
            return base.GoToStateCore(control, templateRoot, "Active", group, state, useTransitions);
        }
        else
        {
            return base.GoToStateCore(control, templateRoot, stateName, group, state, useTransitions);
        }
    }
}