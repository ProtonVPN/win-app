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

using System.Runtime.InteropServices;

namespace ProtonVPN.Client.Logic.Servers.Loads.Native;

internal static partial class NativeMethods
{
    private const string BINARY_NAME = "proton_vpn_binary_status";

    [LibraryImport(BINARY_NAME, EntryPoint = "compute_loads_cffi")]
    internal static unsafe partial int ComputeLoads(
        ReadOnlySpan<FfiLogical> logicals,
        nuint logicalsLength,
        ReadOnlySpan<byte> statusFile,
        nuint statusFileLength,
        in FfiLocation userLocation,
        byte* userCountry,
        Span<FfiLoad> loads,
        out ErrorStringHandle error);

    [LibraryImport(BINARY_NAME, EntryPoint = "free_c_string")]
    internal static partial void FreeString(IntPtr ptr);
}