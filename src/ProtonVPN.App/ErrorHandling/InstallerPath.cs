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
using System.IO;
using Microsoft.Win32;
using ProtonVPN.Common.Configuration;

namespace ProtonVPN.ErrorHandling
{
    public class InstallerPath
    {
        private readonly IConfiguration _config;

        public InstallerPath(IConfiguration config)
        {
            _config = config;
        }

        public bool Exists(string productCode)
        {
            string regPath = "SOFTWARE\\";
            if (Environment.Is64BitOperatingSystem)
            {
                regPath += "WOW6432Node";
            }

            regPath += "\\Caphyon\\Advanced Installer\\LZMA\\";
            regPath += $"{productCode}\\";
            regPath += _config.AppVersion;

            RegistryKey key = Registry.LocalMachine.OpenSubKey(regPath, false);
            if (key == null)
            {
                return false;
            }

            string path = key.GetValue("AI_ExePath") as string;

            return !string.IsNullOrEmpty(path) && File.Exists(path);
        }
    }
}