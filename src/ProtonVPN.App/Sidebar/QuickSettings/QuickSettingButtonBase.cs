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

using System.Windows;
using System.Windows.Controls.Primitives;

namespace ProtonVPN.Sidebar.QuickSettings
{
    internal class QuickSettingButtonBase : ToggleButton
    {
        public static readonly DependencyProperty PopupProperty =
            DependencyProperty.Register("Popup", typeof(Popup),
                typeof(QuickSettingButtonBase),
                new FrameworkPropertyMetadata(null));

        public Popup Popup
        {
            get => (Popup)GetValue(PopupProperty);
            set => SetValue(PopupProperty, value);
        }

        public static readonly DependencyProperty NumberProperty =
            DependencyProperty.Register("Number", typeof(int),
                typeof(QuickSettingButtonBase),
                new FrameworkPropertyMetadata(0));

        public int Number
        {
            get => (int)GetValue(NumberProperty);
            set => SetValue(NumberProperty, value);
        }
    }
}