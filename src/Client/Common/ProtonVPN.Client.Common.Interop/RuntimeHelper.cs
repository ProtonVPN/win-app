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
using System.Text;
using Microsoft.UI.Xaml;
using ProtonVPN.Client.Common.Interop.Models;
using WinUIEx;

namespace ProtonVPN.Client.Common.Interop;

public class RuntimeHelper
{
    public static bool IsMSIX
    {
        get
        {
            int length = 0;

            return GetCurrentPackageFullName(ref length, null) != 15700L;
        }
    }

    public static string PickSingleFile(Window window, string filterName, string[] filters)
    {
        try
        {
            OpenFileName ofn = new();
            ofn.lStructSize = Marshal.SizeOf(ofn);

            ofn.hwndOwner = window.GetWindowHandle();

            string filterExtensions = string.Join(';', filters.Select(f => $"*{f}"));
            ofn.lpstrFilter = $"{filterName} ({filterExtensions})\0{filterExtensions}";

            ofn.lpstrFile = new string(new char[256]);
            ofn.nMaxFile = ofn.lpstrFile.Length;
            ofn.lpstrFileTitle = new string(new char[64]);
            ofn.nMaxFileTitle = ofn.lpstrFileTitle.Length;

            return GetOpenFileName(ref ofn)
                ? ofn.lpstrFile
                : string.Empty;
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int GetCurrentPackageFullName(ref int packageFullNameLength, StringBuilder? packageFullName);

    [DllImport("comdlg32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern bool GetOpenFileName(ref OpenFileName ofn);
}