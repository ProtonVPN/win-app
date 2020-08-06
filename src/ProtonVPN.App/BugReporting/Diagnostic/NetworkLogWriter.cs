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
using System.Collections.Generic;
using System.Threading.Tasks;
using ProtonVPN.Common.Logging;

namespace ProtonVPN.BugReporting.Diagnostic
{
    public class NetworkLogWriter
    {
        private readonly IEnumerable<ILog> _networkLogs;
        private readonly ILogger _logger;

        public NetworkLogWriter(IEnumerable<ILog> networkLogs, ILogger logger)
        {
            _logger = logger;
            _networkLogs = networkLogs;
        }

        public async Task WriteAsync()
        {
            await Task.Run(() =>
            {
                foreach (var log in _networkLogs)
                {
                    try
                    {
                        log.Write();
                    }
                    catch (Exception e)
                    {
                        _logger.Error($"Failed to create log file {log.Path}: " + e);
                    }
                }
            });
        }
    }
}
