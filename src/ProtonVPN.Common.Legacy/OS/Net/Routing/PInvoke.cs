﻿/*
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

namespace ProtonVPN.Common.Legacy.OS.Net.Routing;

public static class PInvoke
{
    private const string DllName = "iphlpapi.dll";

    [DllImport(DllName, CharSet = CharSet.Auto)]
    public static extern int CreateIpForwardEntry(IntPtr route);

    [DllImport(DllName, CharSet = CharSet.Auto)]
    public static extern int DeleteIpForwardEntry(IntPtr route);

    [DllImport(DllName, CharSet = CharSet.Auto)]
    public static extern int GetIpForwardTable(IntPtr pIpForwardTable, ref int pdwSize, bool bOrder);

    [DllImport(DllName)]
    public static extern int GetIpInterfaceEntry(ref MibIPInterfaceRow row);

    public static IPForwardTable ReadIPForwardTable(IntPtr tablePtr)
    {
        IPForwardTable result = (IPForwardTable)Marshal.PtrToStructure(tablePtr, typeof(IPForwardTable));
        MibIPForwardRow[] table = new MibIPForwardRow[result.Size];
        IntPtr p = new IntPtr(tablePtr.ToInt64() + Marshal.SizeOf(result.Size));
        for (int i = 0; i < result.Size; ++i)
        {
            table[i] = (MibIPForwardRow)Marshal.PtrToStructure(p, typeof(MibIPForwardRow));
            p = new IntPtr(p.ToInt64() + Marshal.SizeOf(typeof(MibIPForwardRow)));
        }
        result.Table = table;

        return result;
    }
}