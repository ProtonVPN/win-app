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
using System.IO;
using System.Linq;
using ProtonVPN.Common.Extensions;

namespace ProtonVPN.Common.Logging
{
    public class LogCleaner
    {
        private readonly ILogger _logger;

        public LogCleaner(ILogger logger)
        {
            _logger = logger;
        }

        public void Clean(string logPath, int maxFiles)
        {
            var filesToDelete = GetFiles(logPath)
                .OrderByDescending(CreationTime)
                .Skip(maxFiles);

            DeleteFiles(filesToDelete);
        }

        private IEnumerable<FileInfo> GetFiles(string path)
        {
            try
            {
                var directoryInfo = new DirectoryInfo(path);
                return directoryInfo.GetFiles();
            }
            catch (Exception e) when (e.IsFileAccessException())
            {
                return new FileInfo[0];
            }
        }

        private DateTime CreationTime(FileInfo file)
        {
            try
            {
                return file.CreationTimeUtc;
            }
            catch (Exception e) when (e.IsFileAccessException() || e is ArgumentOutOfRangeException)
            {
                _logger.Error(e.Message);
                return DateTime.MinValue;
            }
        }

        private void DeleteFiles(IEnumerable<FileInfo> files)
        {
            foreach (var file in files)
            {
                DeleteFile(file.FullName);
            }
        }

        private void DeleteFile(string filename)
        {
            try
            {
                File.Delete(filename);
            }
            catch (Exception e) when (e.IsFileAccessException())
            {
                _logger.Error(e.Message);
            }
        }
    }
}
