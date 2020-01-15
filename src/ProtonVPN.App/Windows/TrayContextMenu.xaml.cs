/*
 * Copyright (c) 2020 Proton Technologies AG
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
using System.Windows.Forms;
using System.Windows.Input;
using ProtonVPN.Core;

namespace ProtonVPN.Windows
{
    public partial class TrayContextMenu
    {
        public TrayContextMenu()
        {
            InitializeComponent();

            Deactivated += TrayContextMenu_Deactivated;
            Activated += TrayContextMenu_Activated;
            PreviewMouseLeftButtonUp += TrayContextMenu_MouseLeftButtonUp;
        }

        private void TrayContextMenu_Deactivated(object sender, EventArgs e)
        {
            Hide();
        }

        private void TrayContextMenu_Activated(object sender, EventArgs e)
        {
            var mouseX = Control.MousePosition.X * 96 / SystemParams.GetDpiX();
            var mouseY = Control.MousePosition.Y * 96 / SystemParams.GetDpiY();

            Left = mouseX - ActualWidth < 0 ? mouseX : mouseX - ActualWidth;
            Top = mouseY - ActualHeight < 0 ? mouseY : mouseY - ActualHeight;
        }

        private void TrayContextMenu_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Hide();
        }
    }
}
