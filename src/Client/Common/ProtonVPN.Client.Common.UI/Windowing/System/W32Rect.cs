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

using System.Runtime.InteropServices;

namespace ProtonVPN.Client.Common.UI.Windowing.System;

[StructLayout(LayoutKind.Sequential)]
public struct W32Rect
{
    public int Left = 0;
    public int Top = 0;
    public int Right = 0;
    public int Bottom = 0;

    public W32Rect()
    {
    }

    public W32Rect(W32Point topLeftCorner, int width, int height)
    {
        Left = topLeftCorner.X;
        Top = topLeftCorner.Y;
        Right = topLeftCorner.X + width;
        Bottom = topLeftCorner.Y + height;
    }

    public readonly int GetWidth()
    {
        return Right - Left;
    }

    public readonly int GetHeight()
    {
        return Bottom - Top;
    }
}