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

using System.IO;
using System.Text;
using Microsoft.Win32;

namespace ProtonVPN.BugReporting.Diagnostic
{
    internal class InstalledAppsLog : BaseLog
    {
        private const string RegKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";

        public InstalledAppsLog(string path) : base(path, "Apps.txt")
        {
        }

        public override void Write()
        {
            File.WriteAllText(Path, Content);
        }

        private string Content
        {
            get
            {
                using var key = Registry.LocalMachine.OpenSubKey(RegKey);
                if (key == null)
                {
                    return string.Empty;
                }

                var result = new StringBuilder();
                foreach (var item in key.GetSubKeyNames())
                {
                    using var subKey = key.OpenSubKey(item);
                    var app = subKey?.GetValue("DisplayName");
                    var version = subKey?.GetValue("DisplayVersion");
                    if (app == null)
                    {
                        continue;
                    }

                    result.AppendLine($"{app} {version}");
                }

                return result.ToString();
            }
        }
    }
}
