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
using System.Runtime.InteropServices;
using System.Windows.Interop;
using ProtonVPN.Core.Native.Structures;

namespace ProtonVPN.Core.Native
{
    public static class WindowPlacementExtensions
    {
        [DllImport("user32.dll")]
        private static extern bool SetWindowPlacement(IntPtr hWnd, [In] WindowPlacement lpwndpl);

        [DllImport("user32.dll")]
        private static extern bool GetWindowPlacement(IntPtr hWnd, [In, Out] WindowPlacement lpwndpl);

        public static WindowPlacement GetWindowPlacement(this System.Windows.Window window)
        {
            var result = new WindowPlacement();
            GetWindowPlacement(new WindowInteropHelper(window).Handle, result);
            return result;
        }

        public static bool SetWindowPlacement(this System.Windows.Window window, WindowPlacement placement, WindowStates windowState)
        {
            placement.Length = Marshal.SizeOf(typeof(WindowPlacement));
            placement.Flags = 0;

            switch (windowState)
            {
                case WindowStates.Minimized:
                    placement.ShowCommand = (int)WindowStates.Minimized;
                    break;
                case WindowStates.Normal when placement.ShowCommand == (int)WindowStates.Minimized:
                    placement.ShowCommand = (int)WindowStates.Normal;
                    break;
                case WindowStates.Hidden:
                    placement.ShowCommand = (int)WindowStates.Hidden;
                    break;
            }

            return SetWindowPlacement(new WindowInteropHelper(window).Handle, placement);
        }
    }
}
