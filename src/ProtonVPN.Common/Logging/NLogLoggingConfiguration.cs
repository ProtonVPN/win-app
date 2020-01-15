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

using NLog;
using NLog.Config;
using NLog.Targets;
using NLog.Targets.Wrappers;
using ProtonVPN.Common.Helpers;
using System.Text;

namespace ProtonVPN.Common.Logging
{
    public class NLogLoggingConfiguration
    {
        private FileTarget _fileTarget;
        private readonly string _logFolderPath;
        private readonly string _prefix;

        public NLogLoggingConfiguration(string logFolderPath, string prefix)
        {
            _logFolderPath = logFolderPath;
            _prefix = prefix;
        }

        public NLogLoggingConfiguration Setup()
        {
            Ensure.NotEmpty(_logFolderPath, nameof(_logFolderPath));
            Ensure.NotEmpty(_prefix, nameof(_prefix));

            var config = new LoggingConfiguration();

            var fileTarget = new AsyncTargetWrapper("file", _fileTarget = new FileTarget
            {
                FileName = $"{_logFolderPath}/{_prefix}.txt",
                Layout = "${longdate} ${level:uppercase=true} ${message} ${exception:format=tostring}",
                Encoding = Encoding.UTF8,
                KeepFileOpen = true,
                ConcurrentWrites = false,
                OptimizeBufferReuse = true,
                OpenFileCacheTimeout = 30,
                ArchiveEvery = FileArchivePeriod.Day,
                ArchiveAboveSize = 490000,
                ArchiveFileName = $"{_logFolderPath}/{_prefix}.{{#}}.txt",
                ArchiveNumbering = ArchiveNumberingMode.DateAndSequence,
                MaxArchiveFiles = 30,
                ArchiveDateFormat = "yyyy-MM-dd"
            });
            config.AddTarget(fileTarget);

            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, fileTarget));

            LogManager.Configuration = config;

            return this;
        }

        public NLogFile LogFile(ILogger logger)
        {
            return new NLogFile(logger, _fileTarget);
        }
    }
}
