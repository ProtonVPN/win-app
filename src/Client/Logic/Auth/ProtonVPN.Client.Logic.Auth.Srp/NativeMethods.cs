/*
 * Copyright (c) 2026 Proton AG
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

namespace ProtonVPN.Client.Logic.Auth.Srp;

public static partial class NativeMethods
{
    private const string BINARY_NAME = "proton_srp_cffi";

    [LibraryImport(BINARY_NAME, EntryPoint = "generate_proof", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial int GenerateProof(
        IntPtr passwordPtr,
        nuint passwordLength,
        string salt,
        string modulus,
        string serverChallenge,
        out FfiSrpProof proof,
        out IntPtr errorPtr
    );

    [LibraryImport(BINARY_NAME, EntryPoint = "free_c_string")]
    internal static partial void FreeString(IntPtr stringPtr);

    [LibraryImport(BINARY_NAME, EntryPoint = "free_proof")]
    internal static partial void FreeProof(ref FfiSrpProof proof);

    [LibraryImport("kernel32.dll", EntryPoint = "RtlZeroMemory", SetLastError = false)]
    internal static partial void ZeroMemory(IntPtr dest, int size);
}