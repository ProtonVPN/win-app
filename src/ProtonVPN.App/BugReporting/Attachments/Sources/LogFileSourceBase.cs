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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.AppLogs;

namespace ProtonVPN.BugReporting.Attachments.Sources
{
    public abstract class LogFileSourceBase : ILogFileSource
    {
        private readonly ILogger _logger;
        private readonly string _path;
        private readonly int _maxNumOfFiles;
        private readonly long _maxFileSize;

        protected LogFileSourceBase(ILogger logger, long maxFileSize, string path, int maxNumOfFiles)
        {
            _logger = logger;
            _maxFileSize = maxFileSize;
            _path = path;
            _maxNumOfFiles = maxNumOfFiles;
        }

        public IEnumerable<string> Get()
        {
            try
            {
                return FileNames();
            }
            catch (Exception e) when (e.IsFileAccessException() || e is SecurityException)
            {
                _logger.Warn<AppFileAccessFailedLog>("Failed to add attachment(s).", e);
            }

            return Enumerable.Empty<string>();
        }

        private IEnumerable<string> FileNames()
        {
            DirectoryInfo directory = new(_path);
            IEnumerable<string> fileNames = directory
                .GetFiles()
                .Where(f => f.Length <= _maxFileSize)
                .OrderByDescending(p => p.LastWriteTimeUtc)
                .Take(_maxNumOfFiles)
                .Select(f => f.FullName);

            return fileNames;
        }
    }
}