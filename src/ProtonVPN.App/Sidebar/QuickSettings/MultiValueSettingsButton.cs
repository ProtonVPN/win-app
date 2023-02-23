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

namespace ProtonVPN.Sidebar.QuickSettings
{
    internal class MultiValueSettingsButton : QuickSettingButtonBase
    {
        static MultiValueSettingsButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MultiValueSettingsButton),
                new FrameworkPropertyMetadata(typeof(MultiValueSettingsButton)));
        }

        public static readonly DependencyProperty ModeProperty =
            DependencyProperty.Register("Mode", typeof(int),
                typeof(MultiValueSettingsButton),
                new FrameworkPropertyMetadata(0));

        public int Mode
        {
            get => (int)GetValue(ModeProperty);
            set => SetValue(ModeProperty, value);
        }

        public static readonly DependencyProperty EnabledProperty =
            DependencyProperty.Register("Enabled", typeof(bool),
                typeof(MultiValueSettingsButton),
                new FrameworkPropertyMetadata(false));

        public bool Enabled
        {
            get => (bool)GetValue(EnabledProperty);
            set => SetValue(EnabledProperty, value);
        }
    }
}