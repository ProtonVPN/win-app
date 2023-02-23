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
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ProtonVPN.Windows
{
    public class BalloonNotification
    {
        private static readonly FieldInfo WindowField = typeof(NotifyIcon).GetField("window", BindingFlags.NonPublic | BindingFlags.Instance);

        public void Show(NotifyIcon notifyIcon, Icon icon, string text, int timeout)
        {
            var data = new NotifyIconData
            {
                cbSize = (uint)Marshal.SizeOf(typeof(NotifyIconData)),
                hWnd = GetHandle(notifyIcon),
                uFlags = NotifyFlags.Info,
                uTimeoutOrVersion = timeout,
                szInfo = text,
                hIcon = icon.Handle,
                hBalloonIcon = icon.Handle,
                uID = 1,
                dwInfoFlags = 0x00000004 | 0x00000020
            };

            Shell_NotifyIcon(NotifyCommand.Modify, ref data);
        }

        private static IntPtr GetHandle(NotifyIcon icon)
        {
            if (WindowField == null)
            {
                throw new InvalidOperationException("Unable to find tray icon window");
            }

            if (!(WindowField.GetValue(icon) is NativeWindow window))
            {
                throw new InvalidOperationException("Unable to find tray icon window");
            }

            return window.Handle;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct NotifyIconData
        {
            public uint cbSize;
            public IntPtr hWnd;
            public uint uID;
            public NotifyFlags uFlags;
            private readonly int uCallbackMessage;
            public IntPtr hIcon;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string szTip;
            public int dwState;
            public int dwStateMask;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string szInfo;
            public int uTimeoutOrVersion;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
            public string szInfoTitle;
            public int dwInfoFlags;
            public Guid guid;
            public IntPtr hBalloonIcon;
        }

        private enum NotifyCommand
        {
            Add = 0,
            Modify = 1,
            Delete = 2,
            SetFocus = 3,
            SetVersion = 4
        }

        public enum NotifyFlags
        {
            Message = 1,
            Icon = 2,
            Tip = 4,
            State = 8,
            Info = 16,
            Guid = 32
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern int Shell_NotifyIcon(NotifyCommand cmd, ref NotifyIconData data);
    }
}
