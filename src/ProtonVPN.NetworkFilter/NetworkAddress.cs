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

namespace ProtonVPN.NetworkFilter;

[StructLayout(LayoutKind.Sequential)]
public struct NetworkAddress
{
    private NetworkAddress(string address, string mask, int? prefix, bool isIpv6)
    {
        Address = address;
        Mask = mask;
        Prefix = prefix ?? 128;
        IsIpv6 = isIpv6;
    }

    public static NetworkAddress FromIpv4(string address, string mask)
    {
        return new(address, mask, null, false);
    }

    public static NetworkAddress FromIpv6(string address, int? prefix)
    {
        return new(address, string.Empty, prefix, true);
    }

    [MarshalAs(UnmanagedType.LPStr)]
    public string Address;

    [MarshalAs(UnmanagedType.LPStr)]
    public string Mask;

    public int Prefix;

    public bool IsIpv6;
}