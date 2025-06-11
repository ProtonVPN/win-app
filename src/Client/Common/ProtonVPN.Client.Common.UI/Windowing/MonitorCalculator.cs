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
using Vanara.PInvoke;
using Windows.Foundation;
using static Vanara.PInvoke.SHCore;
using static Vanara.PInvoke.Shell32;
using static Vanara.PInvoke.User32;

namespace ProtonVPN.Client.Common.UI.Windowing;

public static class MonitorCalculator
{
    private const int DEFAULT_WINDOW_WIDTH = 636;
    private const int DEFAULT_WINDOW_HEIGHT = 589;

    public static Point? CalculateWindowCenteredInCursor(double windowWidth, double windowHeight)
    {
        POINT? cursorPosition = GetCursorPosition();
        if (cursorPosition is null)
        {
            return null;
        }

        Monitor? monitor = GetCursorMonitor(cursorPosition.Value);
        if (monitor is null)
        {
            return null;
        }

        return CalculateWindowCenteredInPoint(cursorPosition.Value.ToPoint(), monitor, windowWidth, windowHeight);
    }

    public static Point? CalculateWindowCenteredInCursorMonitor(double windowWidth, double windowHeight)
    {
        POINT? cursorPosition = GetCursorPosition();
        if (cursorPosition is null)
        {
            return null;
        }

        Monitor? monitor = GetCursorMonitor(cursorPosition.Value);
        if (monitor is null)
        {
            return null;
        }

        Point screenCenter = CalculateScreenCenterPoint(monitor);
        return CalculateWindowCenteredInPoint(screenCenter, monitor, windowWidth, windowHeight);
    }

    public static Rect? GetValidWindowSizeAndPosition(Rect windowRect)
    {
        Monitor? monitor = GetWindowMonitor(windowRect.ToRECT());
        if (monitor is null)
        {
            return null;
        }

        double windowWidth = windowRect.Width;
        double windowHeight = windowRect.Height;

        RECT workArea = monitor.WorkArea;

        bool needsCentering =
            windowRect.Top < workArea.top ||
            windowRect.Bottom > workArea.bottom ||
            windowRect.Left < workArea.left ||
            windowRect.Right > workArea.right;

        if (windowWidth > workArea.Width || windowHeight > workArea.Height)
        {
            windowWidth = Math.Min(DEFAULT_WINDOW_WIDTH, workArea.Width);
            windowHeight = Math.Min(DEFAULT_WINDOW_HEIGHT, workArea.Height);
            needsCentering = true;
        }

        if (needsCentering)
        {
            Point screenCenter = CalculateScreenCenterPoint(monitor);
            Point topLeft = CalculateWindowCenteredInPoint(screenCenter, monitor, windowWidth, windowHeight);
            return new Rect(topLeft.X, topLeft.Y, topLeft.X + windowWidth, topLeft.Y + windowHeight);
        }

        return windowRect;
    }

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

    private static Point ToPoint(this POINT point)
    {
        return new Point(point.X, point.Y);
    }

    private static Rect ToRect(this RECT rect)
    {
        return new Rect(rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top);
    }

    private static POINT ToPOINT(this Point point)
    {
        return new POINT((int)point.X, (int)point.Y);
    }

    private static RECT ToRECT(this Rect rect)
    {
        return new RECT((int)rect.X, (int)rect.Y, (int)(rect.X + rect.Width), (int)(rect.Y + rect.Height));
    }

    private static POINT? GetCursorPosition()
    {
        return GetCursorPos(out POINT pt) ? pt : null;
    }

    private static Monitor? GetCursorMonitor(POINT point)
    {
        HMONITOR hMonitor = MonitorFromPoint(point, MonitorFlags.MONITOR_DEFAULTTONEAREST);
        return GetMonitorByHandle(hMonitor);
    }

    private static Monitor? GetWindowMonitor(RECT rect)
    {
        HMONITOR hMonitor = MonitorFromRect(rect, MonitorFlags.MONITOR_DEFAULTTONEAREST);
        return GetMonitorByHandle(hMonitor);
    }

    private static Monitor? GetMonitorByHandle(HMONITOR hMonitor)
    {
        // Declare the MONITORINFO structure before using it
        MONITORINFOEX monitorInfo = new()
        {
            cbSize = (uint)Marshal.SizeOf(typeof(MONITORINFOEX)) // Initialize the structure size
        };

        // Fix the syntax and ensure the structure is passed correctly
        if (!GetMonitorInfo(hMonitor, ref monitorInfo))
        {
            return null;
        }

        if (GetDpiForMonitor(hMonitor, MONITOR_DPI_TYPE.MDT_EFFECTIVE_DPI, out uint dpiX, out uint dpiY) != 0)
        {
            return null;
        }

        return new Monitor(monitorInfo, new POINT((int)dpiX, (int)dpiY));
    }

    private static Point CalculateScreenCenterPoint(Monitor monitor)
    {
        RECT work = monitor.WorkArea;
        return new Point(
            work.left + (work.Width / 2),
            work.top + (work.Height / 2));
    }

    private static Point CalculateWindowCenteredInPoint(Point centerPoint, Monitor monitor, double windowWidth, double windowHeight)
    {
        double scaleX = monitor.Dpi.X / 96.0;
        double scaleY = monitor.Dpi.Y / 96.0;

        int offsetX = (int)Math.Round(windowWidth / 2.0 * scaleX);
        int offsetY = (int)Math.Round(windowHeight / 2.0 * scaleY);

        double left = centerPoint.X - offsetX;
        double top = centerPoint.Y - offsetY;

        RECT screen = monitor.WorkArea;

        // Clamp to screen bounds
        if (left < screen.left)
        {
            left = screen.left;
        }

        if (top < screen.top)
        {
            top = screen.top;
        }

        if (left + windowWidth > screen.right)
        {
            left = screen.right - (int)(windowWidth * scaleX);
        }

        if (top + windowHeight > screen.bottom)
        {
            top = screen.bottom - (int)(windowHeight * scaleY);
        }

        return new Point(left, top);
    }
}