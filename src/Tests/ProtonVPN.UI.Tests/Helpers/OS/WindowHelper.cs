/*
 * Copyright (c) 2026 Proton AG
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
using System.Threading;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace ProtonVPN.UI.Tests.TestsHelper;

public static class WindowHelper
{
    [DllImport("user32.dll")]
    private static extern bool SetProcessDPIAware();

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr FindWindow(string? lpClassName, string? lpWindowName);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool GetWindowRect(IntPtr windowHandle, out RECT lpRect);

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr windowHandle);

    [DllImport("user32.dll")]
    private static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

    private const int MOUSEEVENTF_LEFTDOWN = 0x02;
    private const int MOUSEEVENTF_LEFTUP = 0x04;

    static WindowHelper()
    {
        EnableDpiAwareness();
    }

    public static void EnableDpiAwareness()
    {
        SetProcessDPIAware();
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    public static IntPtr WaitForAndFocusWindow(string title, TimeSpan? timeout = null)
    {
        TimeSpan effectiveTimeout = timeout ?? TestConstants.ThirtySecondsTimeout;
        DateTime timeoutDate = DateTime.UtcNow + effectiveTimeout;

        while (DateTime.UtcNow < timeoutDate)
        {
            IntPtr windowHandle = FindWindow(null, title);

            if (windowHandle != IntPtr.Zero)
            {
                if (!TryGetWindowBounds(windowHandle, out Rectangle bounds))
                {
                    throw new InvalidOperationException($"Could not retrieve the window position: {title}");
                }

                SetForegroundWindow(windowHandle);
                Thread.Sleep(500);

                int x = bounds.Left + (bounds.Width / 2);
                int y = bounds.Top + 12;

                Cursor.Position = new Point(x, y);

                Thread.Sleep(200);

                mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);

                Thread.Sleep(TestConstants.OneSecondTimeout);

                return windowHandle;
            }

            Thread.Sleep(TestConstants.RetryInterval);
        }

        throw new TimeoutException($"Window was not found within {effectiveTimeout.TotalSeconds} seconds: {title}");
    }

    public static bool TryGetWindowBounds(IntPtr windowHandle, out Rectangle bounds)
    {
        if (!GetWindowRect(windowHandle, out RECT rect))
        {
            bounds = Rectangle.Empty;
            return false;
        }

        bounds = new Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
        return true;
    }
}
