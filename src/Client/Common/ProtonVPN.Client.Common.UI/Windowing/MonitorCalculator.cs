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
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Vanara.PInvoke;
using Windows.Foundation;
using Windows.Graphics;
using static Vanara.PInvoke.SHCore;
using static Vanara.PInvoke.Shell32;
using static Vanara.PInvoke.User32;

namespace ProtonVPN.Client.Common.UI.Windowing;

public static class MonitorCalculator
{
    private const double DEFAULT_DPI = 96.0;

    public static TaskbarEdge GetTaskbarEdge()
    {
        APPBARDATA appBarData = new()
        { cbSize = (uint)Marshal.SizeOf<APPBARDATA>() };
        if (SHAppBarMessage(ABM.ABM_GETTASKBARPOS, ref appBarData) == IntPtr.Zero)
        {
            return TaskbarEdge.Unknown;
        }

        return (TaskbarEdge)appBarData.uEdge;
    }

    public static RectInt32? GetTrayRect()
    {
        // Find the Shell_TrayWnd (main taskbar window)
        HWND taskbarHwnd = FindWindow("Shell_TrayWnd");
        // Find the system tray area child window
        HWND trayHwnd = FindWindowEx(taskbarHwnd, IntPtr.Zero, "TrayNotifyWnd");

        if (trayHwnd != IntPtr.Zero && GetWindowRect(trayHwnd, out RECT rect))
        {
            return rect.ToRect();
        }

        return null;
    }

    public static PointInt32 GetCursorPosition()
    {
        POINT cursorPosition = GetCursorPos(out POINT pt) ? pt : POINT.Empty;

        return new PointInt32(
            cursorPosition.X,
            cursorPosition.Y);
    }

    public static uint GetDpi(this DisplayArea area)
    {
        nint monitor = Win32Interop.GetMonitorFromDisplayId(area.DisplayId);

        GetDpiForMonitor(
            monitor,
            MONITOR_DPI_TYPE.MDT_EFFECTIVE_DPI,
            out uint dpiX,
            out _);

        // dpiX == dpiY for square pixels, use either
        return dpiX;
    }

    /// <summary>
    /// Convert Device Independent Pixels (DIPs) to Pixels based on the given DPI (Dots per inch)
    /// </summary>
    public static double ToPixels<T>(this T dips, uint dpi)
        where T : struct, IConvertible
    {
        return Convert.ToDouble(dips) * (dpi / DEFAULT_DPI);
    }

    /// <summary>
    /// Convert Pixels to Device Independent Pixels (DIPs) based on the given DPI (Dots per inch)
    /// </summary>
    public static double ToDips<T>(this T pixels, uint dpi)
        where T : struct, IConvertible
    {
        return Convert.ToDouble(pixels) * (DEFAULT_DPI / dpi);
    }
    private static RectInt32 ToRect(this RECT rect)
    {
        return new RectInt32(rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top);
    }
}