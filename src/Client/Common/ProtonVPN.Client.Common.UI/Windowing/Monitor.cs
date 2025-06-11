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

using Vanara.PInvoke;
using static Vanara.PInvoke.User32;

namespace ProtonVPN.Client.Common.UI.Windowing;

public class Monitor
{
    public RECT Area { get; }
    public RECT WorkArea { get; }
    public POINT Dpi { get; }
    public MonitorInfoFlags Flags { get; }

    public int Width => Area.Width;

    public int Height => Area.Height;

    public Monitor(MONITORINFOEX monitorInfo, POINT dpi)
    {
        Area = monitorInfo.rcMonitor;
        WorkArea = monitorInfo.rcWork;
        Dpi = dpi;
        Flags = monitorInfo.dwFlags;
    }
}