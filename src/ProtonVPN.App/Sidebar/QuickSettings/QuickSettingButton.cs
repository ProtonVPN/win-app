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
using System.Windows.Controls;

namespace ProtonVPN.Sidebar.QuickSettings
{
    internal class QuickSettingButton : QuickSettingButtonBase
    {
        static QuickSettingButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(QuickSettingButton),
                new FrameworkPropertyMetadata(typeof(QuickSettingButton)));
        }

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(UserControl),
                typeof(QuickSettingButton),
                new FrameworkPropertyMetadata(null));

        public UserControl Icon
        {
            get => (UserControl)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public static readonly DependencyProperty ActiveIconProperty =
            DependencyProperty.Register("ActiveIcon", typeof(UserControl),
                typeof(QuickSettingButton),
                new FrameworkPropertyMetadata(null));

        public UserControl ActiveIcon
        {
            get => (UserControl)GetValue(ActiveIconProperty);
            set => SetValue(ActiveIconProperty, value);
        }

        public static readonly DependencyProperty ActiveProperty =
            DependencyProperty.Register("Active", typeof(bool),
                typeof(QuickSettingButton),
                new FrameworkPropertyMetadata(false));

        public bool Active
        {
            get => (bool)GetValue(ActiveProperty);
            set => SetValue(ActiveProperty, value);
        }
    }
}