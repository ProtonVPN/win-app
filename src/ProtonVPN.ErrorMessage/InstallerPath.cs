/*
 * Copyright (c) 2020 Proton Technologies AG
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

namespace ProtonVPN.ErrorMessage
{
    internal class InstallerPath
    {
        private readonly string _productCode;
        private readonly Config _config;

        public InstallerPath(string productCode, Config config)
        {
            _config = config;
            _productCode = productCode;
        }

        public bool Exists()
        {
            var regPath = "SOFTWARE\\";
            if (Environment.Is64BitOperatingSystem)
            {
                regPath += "WOW6432Node";
            }

            regPath += "\\Caphyon\\Advanced Installer\\LZMA\\";
            regPath += $"{_productCode}\\";
            regPath += _config.AppVersion;

            var key = Registry.LocalMachine.OpenSubKey(regPath, false);
            if (key == null)
            {
                return false;
            }

            var path = key.GetValue("AI_ExePath") as string;

            return !string.IsNullOrEmpty(path) && File.Exists(path);
        }
    }
}
