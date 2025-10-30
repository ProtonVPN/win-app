/*
 * Copyright (c) 2025 Proton AG
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
using System.Threading;
using System.Windows.Forms;
using System.Drawing;
using FlaUI.Core.AutomationElements;
using ProtonVPN.UI.Tests.UiTools.Interop;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.UiTools;

public static class NativeInput
{
    // Win32 API requires absolute mouse coordinates in a 16-bit (0–65535) range.
    private const double MOUSE_COORDINATE_SCALE = 65535.0;

    // Keep the cursor a few pixels inside the virtual screen bounds to avoid edge issues.
    private const int SCREEN_EDGE_PADDING_PX = 2;

    // When easing the cursor into place, take a small step first to avoid abrupt jumps.
    private const int HOVER_RAMP_STEP_PX = 8;

    // Used to compute the element’s center (width/2, height/2).
    private const double CENTER_DIVISOR = 2.0;


    public static void BringToForeground(IntPtr hWnd)
    {
        if (hWnd != IntPtr.Zero)
        {
            User32.SetForegroundWindow(hWnd);
        }
    }

    private static void SendMouseAbsolute(int screenX, int screenY)
    {
        Rectangle virtualScreen = SystemInformation.VirtualScreen;

        int absX = (int)Math.Round((screenX - virtualScreen.Left) * MOUSE_COORDINATE_SCALE / (virtualScreen.Width - 1));
        int absY = (int)Math.Round((screenY - virtualScreen.Top) * MOUSE_COORDINATE_SCALE / (virtualScreen.Height - 1));

        Input input = new Input
        {
            Type = User32.INPUT_MOUSE,
            MouseInput = new MouseInput
            {
                Dx = absX,
                Dy = absY,
                MouseData = 0,
                DwFlags = User32.MOUSEEVENTF_MOVE |
                          User32.MOUSEEVENTF_ABSOLUTE |
                          User32.MOUSEEVENTF_VIRTUALDESK,
                Time = 0,
                DwExtraInfo = IntPtr.Zero
            }
        };

        Input[] inputs = new Input[] { input };
        int cbSize = Marshal.SizeOf(typeof(Input));
        User32.SendInput(1, inputs, cbSize);
    }

    public static void HoverAbsolute(AutomationElement element, int hoverDurationMs = TestConstants.DEFAULT_HOVER_DURATION_MS)
    {
        Rectangle r = element.BoundingRectangle;
        Rectangle virtualScreen = SystemInformation.VirtualScreen;

        int cx = Math.Max(virtualScreen.Left + SCREEN_EDGE_PADDING_PX,
            Math.Min(virtualScreen.Right - SCREEN_EDGE_PADDING_PX, (int)(r.Left + r.Width / CENTER_DIVISOR)));
        int cy = Math.Max(virtualScreen.Top + SCREEN_EDGE_PADDING_PX,
            Math.Min(virtualScreen.Bottom - SCREEN_EDGE_PADDING_PX, (int)(r.Top + r.Height / CENTER_DIVISOR)));

        SendMouseAbsolute(cx - (int)r.Width, cy - (int)r.Height);
        SendMouseAbsolute(cx - HOVER_RAMP_STEP_PX, cy - HOVER_RAMP_STEP_PX);
        SendMouseAbsolute(cx, cy);
        Thread.Sleep(hoverDurationMs);
    }
}
