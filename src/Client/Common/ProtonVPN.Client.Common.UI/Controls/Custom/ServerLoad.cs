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

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ProtonVPN.Client.Common.UI.Controls.Custom;

public class ServerLoad : Control
{
    public const double LOW_TO_MEDIUM_SERVER_LOAD_THRESHOLD = 0.75;
    public const double MEDIUM_TO_HIGH_SERVER_LOAD_THRESHOLD = 0.90;

    public static readonly DependencyProperty LoadProperty =
        DependencyProperty.Register(nameof(Load), typeof(double), typeof(ServerLoad), new PropertyMetadata(default));

    public static readonly DependencyProperty LowToMediumThresholdProperty =
        DependencyProperty.Register(nameof(LowToMediumThreshold), typeof(double), typeof(ServerLoad), new PropertyMetadata(LOW_TO_MEDIUM_SERVER_LOAD_THRESHOLD));

    public static readonly DependencyProperty MediumToHighTresholdProperty =
        DependencyProperty.Register(nameof(MediumToHighTreshold), typeof(double), typeof(ServerLoad), new PropertyMetadata(MEDIUM_TO_HIGH_SERVER_LOAD_THRESHOLD));

    public static readonly DependencyProperty IsTextVisibleProperty =
        DependencyProperty.Register(nameof(IsTextVisible), typeof(bool), typeof(ServerLoad), new PropertyMetadata(true));

    public double Load
    {
        get => (double)GetValue(LoadProperty);
        set => SetValue(LoadProperty, value);
    }

    public double LowToMediumThreshold
    {
        get => (double)GetValue(LowToMediumThresholdProperty);
        set => SetValue(LowToMediumThresholdProperty, value);
    }

    public double MediumToHighTreshold
    {
        get => (double)GetValue(MediumToHighTresholdProperty);
        set => SetValue(MediumToHighTresholdProperty, value);
    }

    public bool IsTextVisible
    {
        get => (bool)GetValue(IsTextVisibleProperty);
        set => SetValue(IsTextVisibleProperty, value);
    }
}