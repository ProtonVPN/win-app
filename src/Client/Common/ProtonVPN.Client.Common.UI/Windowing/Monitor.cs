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

using ProtonVPN.Client.Common.UI.Windowing.System;
using Windows.Foundation;

namespace ProtonVPN.Client.Common.UI.Windowing;

public class Monitor
{
    public int Size;
    public Rect Area;
    /// <summary>The WorkArea is the Area of the monitor minus the Windows taskbar</summary>
    public Rect WorkArea;
    public uint Flags;
    public Point Dpi;

    public Monitor()
    {
    }

    public Monitor(W32MonitorInfo w32MonitorInfo, Point dpi)
    {
        Size = w32MonitorInfo.Size;
        Area = W32RectToRect(w32MonitorInfo.Monitor);
        WorkArea = W32RectToRect(w32MonitorInfo.WorkArea);
        Flags = w32MonitorInfo.Flags;
        Dpi = dpi;
    }

    private static Rect W32RectToRect(W32Rect w32Rect)
    {
        return new(new Point(w32Rect.Left, w32Rect.Top),
                   new Point(w32Rect.Right, w32Rect.Bottom));
    }
}