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
    internal class SettingButton : Button
    {
        static SettingButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SettingButton),
                new FrameworkPropertyMetadata(typeof(SettingButton)));
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string),
                typeof(SettingButton),
                new FrameworkPropertyMetadata(string.Empty));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static readonly DependencyProperty ActiveProperty =
            DependencyProperty.Register("Active", typeof(bool),
                typeof(SettingButton),
                new FrameworkPropertyMetadata(false));

        public bool Active
        {
            get => (bool)GetValue(ActiveProperty);
            set => SetValue(ActiveProperty, value);
        }

        public static readonly DependencyProperty IsTurnOnButtonProperty =
            DependencyProperty.Register("IsTurnOnButton", typeof(bool),
                typeof(SettingButton),
                new FrameworkPropertyMetadata(false));

        public bool IsTurnOnButton
        {
            get => (bool)GetValue(IsTurnOnButtonProperty);
            set => SetValue(IsTurnOnButtonProperty, value);
        }

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(UserControl),
                typeof(SettingButton),
                new FrameworkPropertyMetadata(null));

        public UserControl Icon
        {
            get => (UserControl)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public static readonly DependencyProperty DisabledProperty =
            DependencyProperty.Register("Disabled", typeof(bool),
                typeof(SettingButton),
                new FrameworkPropertyMetadata(false));

        public bool Disabled
        {
            get => (bool)GetValue(DisabledProperty);
            set => SetValue(DisabledProperty, value);
        }
    }
}