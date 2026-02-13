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
using System.Text;

namespace ProtonVPN.Common.Legacy.Go;

public static class GoExtensions
{
    public static byte[] ConvertToBytes(this GoBytes goBytes)
    {
        int length = goBytes.Length.ToInt32();
        byte[] bytes = new byte[length];
        for (int i = 0; i < length; i++)
        {
            bytes[i] = Marshal.ReadByte(goBytes.Data, i);
        }

        return bytes;
    }

    public static GoString ToGoString(this string str)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(str);
        IntPtr nativeUtf8 = Marshal.AllocHGlobal(buffer.Length);
        Marshal.Copy(buffer, 0, nativeUtf8, buffer.Length);
        return new GoString { Data = nativeUtf8, Length = (IntPtr)buffer.Length };
    }

    public static string ConvertToString(this GoBytes bytes)
    {
        return Encoding.UTF8.GetString(bytes.ConvertToBytes());
    }
}