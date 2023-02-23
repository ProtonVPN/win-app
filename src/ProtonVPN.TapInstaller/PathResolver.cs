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

namespace TapInstaller
{
    public class PathResolver
    {
        private readonly string _baseDir;

        public PathResolver(string baseDir)
        {
            _baseDir = baseDir;
        }

        public string GetTapinstallPath()
        {
            return Path.Combine(_baseDir, "installer", Bitness() ,"tapinstall.exe");
        }

        public string GetDriversDir()
        {
            return Path.Combine(_baseDir, WindowsVersion(), Bitness());
        }

        private static string Bitness()
        {
            return Environment.Is64BitOperatingSystem ? "x64" : "x86";
        }

        private static string WindowsVersion()
        {
            if (Environment.OSVersion.Version.Major == 10)
                return "windows10";

            return "windows7";
        }
    }
}
