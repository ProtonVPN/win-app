/*
 * Copyright (c) 2021 Proton Technologies AG
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

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace ProtonVPN.Sidebar.Announcements
{
    public partial class AnnouncementsView
    {
        public AnnouncementsView()
        {
            InitializeComponent();
            Popup.Opened += Popup_Opened;
        }

        private void Popup_Opened(object sender, EventArgs e)
        {
            Popup.HorizontalOffset = Popup.Child.RenderSize.Width  / -2 + TogglePopupButton.ActualWidth / 2;
        }

        private void Image_OnImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            ((Image)sender).Source = new BitmapImage(new Uri("/ProtonVPN;component/Resources/Assets/Images/bullhorn-solid.png", UriKind.Relative));
        }

        private void OpenPopupButton_MouseEnter(object sender, MouseEventArgs e)
        {
            Popup.IsOpen = true;
            Popup.StaysOpen = true;
        }

        private void OpenPopupButton_MouseLeave(object sender, MouseEventArgs e)
        {
            Popup.IsOpen = false;
            Popup.StaysOpen = false;
        }
    }
}
