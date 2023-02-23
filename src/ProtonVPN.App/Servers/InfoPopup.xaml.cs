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

namespace ProtonVPN.Servers
{
    public partial class InfoPopup
    {
        public static readonly DependencyProperty ShowPopupProperty = DependencyProperty.Register(
            nameof(ShowPopup), typeof(bool), typeof(InfoPopup), new PropertyMetadata(false));

        public static readonly DependencyProperty PlacementTargetProperty = DependencyProperty.Register(
            nameof(PlacementTarget), typeof(UIElement), typeof(InfoPopup), new PropertyMetadata(null));

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            nameof(Title), typeof(string), typeof(InfoPopup), new PropertyMetadata(string.Empty));

        public InfoPopup()
        {
            InitializeComponent();
            CloseButton.Click += CloseButton_Click;
            Popup.MouseDown += (_, e) => e.Handled = true;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Popup.IsOpen = false;
        }

        public bool ShowPopup
        {
            get => (bool)GetValue(ShowPopupProperty);
            set => SetValue(ShowPopupProperty, value);
        }

        public UIElement PlacementTarget
        {
            get => (UIElement)GetValue(PlacementTargetProperty);
            set => SetValue(PlacementTargetProperty, value);
        }


        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }
    }
}