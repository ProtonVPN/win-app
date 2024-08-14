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
using ProtonVPN.Client.Common.Enums;

namespace ProtonVPN.Client.Common.UI.Assets.Icons.Base;

public class ProfileIconUserControl : IconUserControl
{
    public static readonly DependencyProperty AccentColorProperty =
        DependencyProperty.Register(nameof(AccentColor), typeof(ProfileColor), typeof(ProfileIconUserControl), new PropertyMetadata(ProfileColor.Purple));

    public ProfileColor AccentColor
    {
        get => (ProfileColor)GetValue(AccentColorProperty);
        set => SetValue(AccentColorProperty, value);
    }
}