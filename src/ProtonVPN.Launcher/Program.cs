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
using System.Diagnostics;
using System.IO;

namespace ProtonVPN.Launcher
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string[] folders = Directory.GetDirectories(AppDomain.CurrentDomain.BaseDirectory, "v*", SearchOption.TopDirectoryOnly);
            Version latestVersion = new(0, 0, 0);
            foreach (string path in folders)
            {
                string versionString = new DirectoryInfo(path).Name.Replace("v", string.Empty);
                if (Version.TryParse(versionString, out Version version))
                {
                    if (version > latestVersion)
                    {
                        latestVersion = version;
                    }
                }
            }

            if (latestVersion > new Version(0, 0, 0))
            {
                Process.Start($"v{latestVersion}\\ProtonVPN.Client.exe", args);
            }
        }
    }
}