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
using ProtonVPN.Core.Native.Structures;

namespace ProtonVPN.Core.Native
{
    public static class WindowPositionExtensions
    {
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOZORDER = 0x0004;
        private const uint SWP_NOREDRAW = 0x0008;
        private const uint SWP_NOACTIVATE = 0x0010;
        private const uint SWP_FRAMECHANGED = 0x0020;
        private const uint SWP_SHOWWINDOW = 0x0040;
        private const uint SWP_HIDEWINDOW = 0x0080;
        private const uint SWP_NOCOPYBITS = 0x0100;
        private const uint SWP_NOOWNERZORDER = 0x0200;
        private const uint SWP_NOSENDCHANGING = 0x0400;

        private const uint TOPMOST_FLAGS =
            SWP_NOACTIVATE | SWP_NOOWNERZORDER | SWP_NOSIZE | SWP_NOMOVE | SWP_NOREDRAW | SWP_NOSENDCHANGING;

        private static readonly IntPtr HWND_TOPMOST = new(-1);
        private static readonly IntPtr HWND_NOTOPMOST = new(-2);
        private static readonly IntPtr HWND_TOP = new(0);
        private static readonly IntPtr HWND_BOTTOM = new(1);

        public static Rectangle? GetWindowRectangle(IntPtr hwnd)
        {
            bool isSuccessful = GetWindowRect(hwnd, out Rectangle rect);
            return isSuccessful ? rect : null;
        }

        /// <summary>https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getwindowrect</summary>
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowRect(IntPtr hWnd, out Rectangle lpRect);

        public static void SetWindowPosition(IntPtr hwnd, WindowPosition windowPosition)
        {
            if (windowPosition.IsForegroundWindow)
            {
                SetWindowPosition(hwnd, HWND_TOPMOST, windowPosition);
            }
            else
            {
                SetWindowPosition(hwnd, HWND_TOP, windowPosition);
                SetWindowPosition(hwnd, HWND_NOTOPMOST, windowPosition);
            }
        }

        private static void SetWindowPosition(IntPtr hwnd, IntPtr hWndInsertAfter, WindowPosition windowPosition)
        {
            SetWindowPos(hwnd, hWndInsertAfter, windowPosition.X, windowPosition.Y, windowPosition.Width, windowPosition.Height, TOPMOST_FLAGS);
        }

        /// <summary>https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowpos</summary>
        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
    }
}
