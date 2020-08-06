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
using System.IO.Compression;

namespace ProtonVPN.BugReporting.Diagnostic
{
    internal class DriverInstallLog : BaseLog
    {
        private const string Filename = "setupapi.dev.log";

        public DriverInstallLog(string path) : base(path, Filename)
        {
        }

        public override void Write()
        {
            var windowsPath = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            var copyFrom = System.IO.Path.Combine(windowsPath, "inf", Filename);

            using var fs = new FileStream(Path.Replace(".log", ".zip"), FileMode.Create);
            using var arch = new ZipArchive(fs, ZipArchiveMode.Create);
            arch.CreateEntryFromFile(copyFrom, Filename);
        }
    }
}
