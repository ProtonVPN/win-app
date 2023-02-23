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

namespace ProtonVPN.WireGuardDriver
{
    internal class Win32
    {
        private const string WireGuardDll = "wireguard.dll";

        [DllImport(WireGuardDll, EntryPoint = "WireGuardOpenAdapter", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern IntPtr OpenAdapter([MarshalAs(UnmanagedType.LPWStr)] string name);

        [DllImport(WireGuardDll, EntryPoint = "WireGuardCloseAdapter", CallingConvention = CallingConvention.StdCall)]
        public static extern void CloseAdapter(IntPtr adapter);

        [DllImport(WireGuardDll, EntryPoint = "WireGuardGetConfiguration", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern bool GetConfiguration(IntPtr adapter, byte[] iface, ref uint bytes);
    }
}