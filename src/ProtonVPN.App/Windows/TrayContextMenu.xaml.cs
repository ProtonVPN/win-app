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

using System;
using System.Windows.Input;

namespace ProtonVPN.Windows
{
    public partial class TrayContextMenu
    {
        private readonly IWindowPositionSetter _windowPositionSetter;

        public TrayContextMenu(IWindowPositionSetter windowPositionSetter)
        {
            _windowPositionSetter = windowPositionSetter;

            InitializeComponent();

            Deactivated += TrayContextMenu_Deactivated;
            Activated += TrayContextMenu_Activated;
            PreviewMouseLeftButtonUp += TrayContextMenu_MouseLeftButtonUp;
        }

        private void TrayContextMenu_Activated(object sender, EventArgs e)
        {
            _windowPositionSetter.SetPositionToMouse(this);
        }

        private void TrayContextMenu_Deactivated(object sender, EventArgs e)
        {
            Hide();
        }

        private void TrayContextMenu_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Hide();
        }
    }
}
