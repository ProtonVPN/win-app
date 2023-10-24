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

using System.IO.Compression;
using ProtonVPN.Configurations.Contracts;

namespace ProtonVPN.Client.Logic.Feedback.Diagnostics.Logs;

public class DriverInstallLog : LogBase
{
    private const string SETUP_API_LOG_FILE = "setupapi.dev.log";

    public DriverInstallLog(IStaticConfiguration config)
        : base(config.DiagnosticLogsFolder, "setupapi.dev.zip")
    { }

    public override void Write()
    {
        string windowsPath = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        string copyFrom = System.IO.Path.Combine(windowsPath, "inf", SETUP_API_LOG_FILE);

        using FileStream fs = new(Path, FileMode.Create);
        using ZipArchive arch = new(fs, ZipArchiveMode.Create);
        arch.CreateEntryFromFile(copyFrom, SETUP_API_LOG_FILE);
    }
}