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
using ProtonVPN.Client.Common.UI.Extensions;
using ProtonVPN.Client.Common.UI.Windowing.System;
using Windows.Foundation;

namespace ProtonVPN.Client.Common.UI.Windowing;

public class CursorWindowCalculator
{
    /// <summary>Return the nearest monitor if the point is not contained by any monitor</summary>
    private const uint MONITOR_DEFAULTTONEAREST = 0x00000002;
    /// <summary>The effective DPI. This value should be used when determining the correct scale factor for scaling UI elements.
    /// This incorporates the scale factor set by the user for this specific display.</summary>
    private const uint MDT_EFFECTIVE_DPI = 0;

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetCursorPos(ref W32Point pt);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetMonitorInfo(IntPtr hMonitor, ref W32MonitorInfo lpmi);

    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromPoint(W32Point pt, uint dwFlags);

    [DllImport("Shcore.dll")]
    private static extern uint GetDpiForMonitor(IntPtr hMonitor, uint dpiType, ref uint dpiX, ref uint dpiY);

    public static W32Point? CalculateWindowCenteredInCursorMonitor(double windowWidth, double windowHeight)
    {
        try
        {
            W32Point? nullableCursorPosition = GetCursorPosition();
            if (nullableCursorPosition is null)
            {
                return null; // Error when obtaining mouse cursor position
            }
            W32Point cursorPosition = nullableCursorPosition.Value;

            Monitor monitor = GetCursorMonitor(cursorPosition);
            if (monitor is null)
            {
                return null; // Error when obtaining monitor information
            }

            W32Point screenCenterPoint = CalculateScreenCenterPoint(monitor);

            return CalculateWindowCenteredInPoint(screenCenterPoint, monitor, windowWidth, windowHeight);
        }
        catch
        {
            return null;
        }
    }

    private static W32Point CalculateScreenCenterPoint(Monitor monitor)
    {
        Rect screen = monitor.WorkArea;
        return new()
        {
            X = (int)Math.Round(screen.Left + ((screen.Right - screen.Left) / 2.0)),
            Y = (int)Math.Round(screen.Top + ((screen.Bottom - screen.Top) / 2.0))
        };
    }

    private static W32Point? GetCursorPosition()
    {
        W32Point point = new();
        return GetCursorPos(ref point) ? point : null;
    }

    private static Monitor GetCursorMonitor(W32Point cursorPosition)
    {
        IntPtr monitorHandle = MonitorFromPoint(cursorPosition, MONITOR_DEFAULTTONEAREST);
        W32MonitorInfo monitorInfo = new()
        {
            Size = Marshal.SizeOf(typeof(W32MonitorInfo))
        };
        bool hasMonitorInfo = GetMonitorInfo(monitorHandle, ref monitorInfo);

        uint dpiX = 0;
        uint dpiY = 0;
        uint result = GetDpiForMonitor(monitorHandle, MDT_EFFECTIVE_DPI, ref dpiX, ref dpiY);

        if (hasMonitorInfo && result == 0)
        {
            return new Monitor(monitorInfo, new Point(x: dpiX, y: dpiY));
        }
        return null;
    }

    private static W32Point CalculateWindowCenteredInPoint(W32Point centerPoint,
        Monitor monitor, double windowWidth, double windowHeight)
    {
        Rect monitorRect = monitor.WorkArea;
        double offsetX = Math.Round((windowWidth / 2.0) * (monitor.Dpi.X / 96.0));
        double offsetY = Math.Round((windowHeight / 2.0) * (monitor.Dpi.Y / 96.0));

        double top = centerPoint.Y - offsetY;
        double left = centerPoint.X - offsetX;

        Rect screen = new(new Point(monitorRect.Left, monitorRect.Top), new Point(monitorRect.Right, monitorRect.Bottom));
        Rect window = new(new Point(left, top), new Point(left + windowWidth, top + windowHeight));

        top = window.Top;
        left = window.Left;

        if (!screen.Contains(window))
        {
            if (window.Top < screen.Top)
            {
                double diff = Math.Abs(screen.Top - window.Top);
                top = window.Top + diff;
            }

            if (window.Bottom > screen.Bottom)
            {
                double diff = window.Bottom - screen.Bottom;
                top = window.Top - diff;
            }

            if (window.Left < screen.Left)
            {
                double diff = Math.Abs(screen.Left - window.Left);
                left = window.Left + diff;
            }

            if (window.Right > screen.Right)
            {
                double diff = window.Right - screen.Right;
                left = window.Left - diff;
            }
        }

        return new W32Point() { X = (int)left, Y = (int)top };
    }

    public static W32Point? CalculateWindowCenteredInCursor(double windowWidth, double windowHeight)
    {
        try
        {
            W32Point? nullableCursorPosition = GetCursorPosition();
            if (nullableCursorPosition is null)
            {
                return null; // Error when obtaining mouse cursor position
            }
            W32Point cursorPosition = nullableCursorPosition.Value;

            Monitor monitor = GetCursorMonitor(cursorPosition);
            if (monitor is null)
            {
                return null; // Error when obtaining monitor information
            }

            return CalculateWindowCenteredInPoint(cursorPosition, monitor, windowWidth, windowHeight);
        }
        catch
        {
            return null;
        }
    }
}
