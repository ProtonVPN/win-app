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

internal sealed class ErrorStringHandle : SafeHandle
{
    public ErrorStringHandle() : base(IntPtr.Zero, true)
    {
    }

    public override bool IsInvalid => handle == IntPtr.Zero;

    protected override bool ReleaseHandle()
    {
        NativeMethods.FreeString(handle);
        return true;
    }

    public override string ToString()
    {
        if (IsInvalid)
        {
            return "Unknown error (null pointer)";
        }

        try
        {
            return Marshal.PtrToStringUTF8(handle) ?? "Unknown error (failed to convert string)";
        }
        catch (Exception ex)
        {
            return $"Error converting native error message: {ex.Message}";
        }
    }
}