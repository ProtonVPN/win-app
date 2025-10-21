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
using Microsoft.Win32;

namespace ProtonVPN.Service.Update;

public class CurrentAppVersionProvider : ICurrentAppVersionProvider
{
    private const string PATH = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Proton VPN_is1";

    public Version GetVersion()
    {
        using (RegistryKey subkey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey(PATH))
        {
            object value = subkey?.GetValue("DisplayVersion");
            string versionString = value?.ToString() ?? string.Empty;
            return new Version(versionString);
        }
    }
}