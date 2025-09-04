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

namespace ProtonVPN.Common.Core.Helpers;

// The meaning of each Windows SKU can be read here: https://learn.microsoft.com/en-us/windows/win32/api/sysinfoapi/nf-sysinfoapi-getproductinfo
public class WindowsSku
{
    private static readonly Lazy<int> _value = new(GetSku);

    public static int Get()
    {
        return _value.Value;
    }

    public static string GetHexString()
    {
        return $"0x{Get():X}";
    }

    private static int GetSku()
    {
        int productType;
        try
        {
            Version os = OSVersion.Get();
            GetProductInfo(os.Major, os.Minor, 0, 0, out productType);
        }
        catch
        {
            productType = -1;
        }
        return productType;
    }

    [DllImport("Kernel32.dll")]
    private static extern bool GetProductInfo(
        int dwOSMajorVersion,
        int dwOSMinorVersion,
        int dwSpMajorVersion,
        int dwSpMinorVersion,
        out int pdwReturnedProductType);
}