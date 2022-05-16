/*
 * Copyright (c) 2022 Proton Technologies AG
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
using System.Drawing;
using System.Windows.Forms;
using ProtonVPN.Core;
using Window = System.Windows.Window;

namespace ProtonVPN.Windows
{
    public class WindowPositionSetter : IWindowPositionSetter
    {
        public void SetPositionToMouse(Window window)
        {
            Point mousePosition = Control.MousePosition;
            Screen screen = Screen.FromPoint(mousePosition);

            double mouseX = mousePosition.X * 96 / SystemParams.GetDpiX();
            double mouseY = mousePosition.Y * 96 / SystemParams.GetDpiY();

            int screenVerticalCenter = screen.Bounds.Top + (int)Math.Floor(screen.Bounds.Height / 2.0);
            int screenHorizontalCenter = screen.Bounds.Left + (int)Math.Floor(screen.Bounds.Width / 2.0);

            window.Left = mouseX < screenHorizontalCenter ? mouseX : mouseX - window.ActualWidth;
            window.Top = mouseY < screenVerticalCenter ? mouseY : mouseY - window.ActualHeight;
        }
    }
}
