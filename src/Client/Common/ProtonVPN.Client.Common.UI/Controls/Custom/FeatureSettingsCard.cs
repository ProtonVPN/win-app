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

using CommunityToolkit.WinUI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ProtonVPN.Client.Common.UI.Controls.Custom;

public class FeatureSettingsCard : SettingsCard
{
    public object FeatureIcon
    {
        get => (object)GetValue(FeatureIconProperty);
        set => SetValue(FeatureIconProperty, value);
    }

    public static readonly DependencyProperty FeatureIconProperty =
        DependencyProperty.Register(nameof(FeatureIcon), typeof(object), typeof(FeatureSettingsCard), new PropertyMetadata(default));

    public FeatureSettingsCard()
    {
        DefaultStyleKey = typeof(FeatureSettingsCard);

        // Set the header icon just so the container is not made invisible
        HeaderIcon = new FontIcon();
    }
}