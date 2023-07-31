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
using ProtonVPN.Common.Configuration;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;

namespace ProtonVPN.Client.Logic.Feedback.Diagnostics;

public class NetworkLogWriter : INetworkLogWriter
{
    private readonly IEnumerable<ILog> _networkLogs;
    private readonly ILogger _logger;
    private readonly IConfiguration _config;

    public NetworkLogWriter(IConfiguration config, IEnumerable<ILog> networkLogs, ILogger logger)
    {
        _config = config;
        _logger = logger;
        _networkLogs = networkLogs;
    }

    public async Task WriteAsync()
    {
        await Task.Run(() =>
        {
            using FileStream fs = new(_config.DiagnosticsZipPath, FileMode.Create);
            using ZipArchive arch = new(fs, ZipArchiveMode.Create);

            foreach (ILog log in _networkLogs)
            {
                try
                {
                    log.Write();
                    arch.CreateEntryFromFile(log.Path, log.Filename);
                    if (File.Exists(log.Path))
                    {
                        File.Delete(log.Path);
                    }
                }
                catch (Exception e)
                {
                    _logger.Error<AppFileAccessFailedLog>($"Failed to create log file '{log.Path}'.", e);
                }
            }
        });
    }
}