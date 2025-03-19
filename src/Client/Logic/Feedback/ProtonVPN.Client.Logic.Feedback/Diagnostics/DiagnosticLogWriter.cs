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
using ProtonVPN.Client.Logic.Feedback.Diagnostics.Logs;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;

namespace ProtonVPN.Client.Logic.Feedback.Diagnostics;

public class DiagnosticLogWriter : IDiagnosticLogWriter
{
    private readonly IEnumerable<ILog> _diagnosticLogFiles;
    private readonly ILogger _logger;
    private readonly IStaticConfiguration _config;

    public DiagnosticLogWriter(IStaticConfiguration config, IEnumerable<ILog> diagnosticLogFiles, ILogger logger)
    {
        _config = config;
        _logger = logger;
        _diagnosticLogFiles = diagnosticLogFiles;
    }

    public async Task WriteAsync()
    {
        await Task.Run(() =>
        {
            Directory.CreateDirectory(_config.DiagnosticLogsFolder);

            using FileStream fs = new(_config.DiagnosticLogsZipFilePath, FileMode.Create);
            using ZipArchive arch = new(fs, ZipArchiveMode.Create);

            foreach (ILog logFile in _diagnosticLogFiles)
            {
                try
                {
                    logFile.Write();
                    arch.CreateEntryFromFile(logFile.Path, logFile.Filename);
                    if (File.Exists(logFile.Path))
                    {
                        File.Delete(logFile.Path);
                    }
                }
                catch (Exception e)
                {
                    _logger.Error<AppFileAccessFailedLog>($"Failed to create log file '{logFile.Path}'.", e);
                }
            }
        });
    }
}