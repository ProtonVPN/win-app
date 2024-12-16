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
using ProtonVPN.Client.Common.UI.Controls.Bases;

namespace ProtonVPN.Client.Common.UI.Controls.Custom;

public class ServerConnectionRowButton : ConnectionRowButtonBase
{
    public static readonly DependencyProperty SupportsSmartRoutingProperty =
        DependencyProperty.Register(nameof(SupportsSmartRouting), typeof(bool), typeof(ServerConnectionRowButton), new PropertyMetadata(default));

    public static readonly DependencyProperty SmartRoutingLabelProperty =
        DependencyProperty.Register(nameof(SmartRoutingLabel), typeof(string), typeof(ServerConnectionRowButton), new PropertyMetadata(default));

    public static readonly DependencyProperty ServerLoadProperty =
        DependencyProperty.Register(nameof(ServerLoad), typeof(double), typeof(ServerConnectionRowButton), new PropertyMetadata(default));

    public static readonly DependencyProperty BaseLocationProperty =
        DependencyProperty.Register(nameof(BaseLocation), typeof(string), typeof(ServerConnectionRowButton), new PropertyMetadata(default));

    public bool SupportsSmartRouting
    {
        get => (bool)GetValue(SupportsSmartRoutingProperty);
        set => SetValue(SupportsSmartRoutingProperty, value);
    }

    public string SmartRoutingLabel
    {
        get => (string)GetValue(SmartRoutingLabelProperty);
        set => SetValue(SmartRoutingLabelProperty, value);
    }

    public double ServerLoad
    {
        get => (double)GetValue(ServerLoadProperty);
        set => SetValue(ServerLoadProperty, value);
    }

    public string BaseLocation
    {
        get => (string)GetValue(BaseLocationProperty);
        set => SetValue(BaseLocationProperty, value);
    }
}