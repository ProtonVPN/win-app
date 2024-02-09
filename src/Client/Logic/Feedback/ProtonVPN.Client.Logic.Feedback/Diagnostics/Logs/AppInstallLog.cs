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

using ProtonVPN.Configurations.Contracts;

namespace ProtonVPN.Client.Logic.Feedback.Diagnostics.Logs;

public class AppInstallLog : LogBase
{
    private readonly IStaticConfiguration _config;

    public AppInstallLog(IStaticConfiguration config) : base(config.DiagnosticLogsFolder, "Install.log.txt")
    {
        _config = config;
    }

    public override void Write()
    {
        if (File.Exists(_config.InstallLogsFilePath))
        {
            File.Copy(_config.InstallLogsFilePath, Path, true);
        }
    }
}