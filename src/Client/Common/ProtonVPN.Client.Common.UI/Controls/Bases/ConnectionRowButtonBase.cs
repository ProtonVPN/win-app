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

using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ProtonVPN.Client.Common.UI.Controls.Bases;

public abstract class ConnectionRowButtonBase : Button
{
    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(object), typeof(ConnectionRowButtonBase), new PropertyMetadata(default));

    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register(nameof(Header), typeof(string), typeof(ConnectionRowButtonBase), new PropertyMetadata(default));

    public static readonly DependencyProperty IsActiveConnectionProperty =
        DependencyProperty.Register(nameof(IsActiveConnection), typeof(bool), typeof(ConnectionRowButtonBase), new PropertyMetadata(default));

    public static readonly DependencyProperty IsUnderMaintenanceProperty =
        DependencyProperty.Register(nameof(IsUnderMaintenance), typeof(bool), typeof(ConnectionRowButtonBase), new PropertyMetadata(default));

    public static readonly DependencyProperty IsRestrictedProperty =
        DependencyProperty.Register(nameof(IsRestricted), typeof(bool), typeof(ConnectionRowButtonBase), new PropertyMetadata(default));

    public static readonly DependencyProperty SupportsP2PProperty =
        DependencyProperty.Register(nameof(SupportsP2P), typeof(bool), typeof(ConnectionRowButtonBase), new PropertyMetadata(default));

    public static readonly DependencyProperty SupportsTorProperty =
        DependencyProperty.Register(nameof(SupportsTor), typeof(bool), typeof(ConnectionRowButtonBase), new PropertyMetadata(default));

    public static readonly DependencyProperty P2PLabelProperty =
        DependencyProperty.Register(nameof(P2PLabel), typeof(string), typeof(ConnectionRowButtonBase), new PropertyMetadata(default));

    public static readonly DependencyProperty TorLabelProperty =
        DependencyProperty.Register(nameof(TorLabel), typeof(string), typeof(ConnectionRowButtonBase), new PropertyMetadata(default));

    public object Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public string Header
    {
        get => (string)GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public bool IsActiveConnection
    {
        get => (bool)GetValue(IsActiveConnectionProperty);
        set => SetValue(IsActiveConnectionProperty, value);
    }

    public bool IsUnderMaintenance
    {
        get => (bool)GetValue(IsUnderMaintenanceProperty);
        set => SetValue(IsUnderMaintenanceProperty, value);
    }

    public bool IsRestricted
    {
        get => (bool)GetValue(IsRestrictedProperty);
        set => SetValue(IsRestrictedProperty, value);
    }

    public bool SupportsP2P
    {
        get => (bool)GetValue(SupportsP2PProperty);
        set => SetValue(SupportsP2PProperty, value);
    }

    public bool SupportsTor
    {
        get => (bool)GetValue(SupportsTorProperty);
        set => SetValue(SupportsTorProperty, value);
    }

    public string P2PLabel
    {
        get => (string)GetValue(P2PLabelProperty);
        set => SetValue(P2PLabelProperty, value);
    }

    public string TorLabel
    {
        get => (string)GetValue(TorLabelProperty);
        set => SetValue(TorLabelProperty, value);
    }
}