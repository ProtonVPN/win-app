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

namespace ProtonVPN.IPv6.Interop;

internal static partial class NativeMethods
{
    private const string BINARY_NAME = "proton_vpn_ipv6chaos";

    [LibraryImport(BINARY_NAME, EntryPoint = "ipv6_chaos_algorithm_new")]
    internal static unsafe partial int CreateInstance(
        [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr)]
        string[] prefixes,
        nuint prefixesLength,
        ReadOnlySpan<byte> prefixTree,
        nuint prefixTreeLength,
        out Ipv6ChaosAlgoHandle handle);

    [LibraryImport(BINARY_NAME, EntryPoint = "ipv6_chaos_algorithm_generate")]
    internal static partial int Generate(
        Ipv6ChaosAlgoHandle handle,
        [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr)]
        string[] realIpv6Addresses,
        nuint realIpv6AddressesLength,
        ushort addressesToGenerate,
        out FFISliceHandle sliceHandle);

    [LibraryImport(BINARY_NAME, EntryPoint = "ipv6_chaos_algorithm_persist_load")]
    internal static partial int PersistLoad(
        Ipv6ChaosAlgoHandle handle,
        [MarshalAs(UnmanagedType.LPStr)]
        string persistedData);

    [LibraryImport(BINARY_NAME, EntryPoint = "ipv6_chaos_algorithm_persist_dump")]
    internal static partial int PersistDump(Ipv6ChaosAlgoHandle handle, out Utf8StringHandle stringHandle);

    [LibraryImport(BINARY_NAME, EntryPoint = "ipv6_chaos_algorithm_free")]
    internal static partial int CloseHandle(IntPtr handle);

    [LibraryImport(BINARY_NAME, EntryPoint = "free_ipv6_slice")]
    internal static partial int FreeIpv6Slice(IntPtr handle);

    [LibraryImport(BINARY_NAME, EntryPoint = "free_string")]
    internal static partial int FreeString(IntPtr ptr);
}