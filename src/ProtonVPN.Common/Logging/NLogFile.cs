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
using NLog.Layouts;
using NLog.Targets;
using NLog.Targets.Wrappers;
using System;
using System.IO;
using System.IO.Compression;

namespace ProtonVPN.Common.Logging
{
    public class NLogFile
    {
        private readonly ILogger _logger;
        private readonly FileTarget _target;

        public NLogFile(ILogger logger, string targetName): this(logger, Target<FileTarget>(targetName))
        {
        }

        public NLogFile(ILogger logger, FileTarget target)
        {
            _logger = logger;
            _target = target;
        }

        public string PlainContent()
        {
            return SafeContent((f) => PlainFileContent(f));
        }

        public byte[] ZippedContent(string targetName)
        {
            return SafeContent((f) => ZippedFileContent(f));
        }

        private T SafeContent<T>(Func<string, T> func)
        {
            try
            {
                return LogContent(func);
            }
            catch (IOException ex)
            {
                _logger.Error(ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.Error(ex);
            }
            catch (NotSupportedException ex)
            {
                _logger.Error(ex);
            }

            return default;
        }

        private T LogContent<T>(Func<string, T> func)
        {
            var fileName = LogFileName(_target);
            if (string.IsNullOrEmpty(fileName))
                throw new NotSupportedException($"Unable to get latest log file name");

            T content = default;
            if (fileName != null && File.Exists(fileName))
                content = func(fileName);

            return content;
        }

        private byte[] ZippedFileContent(string fileName)
        {
            using (var outStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(outStream, ZipArchiveMode.Create, true))
                {
                    var fileInArchive = archive.CreateEntry(fileName, CompressionLevel.Optimal);
                    using (var entryStream = fileInArchive.Open())
                    using (var fileToCompress = File.OpenRead(fileName))
                    {
                        fileToCompress.CopyTo(entryStream);
                    }
                }
                return outStream.ToArray();
            }
        }

        private string PlainFileContent(string fileName)
        {
            return File.ReadAllText(fileName);
        }

        private string LogFileName(string targetName)
        {
            var target = Target<FileTarget>(targetName);
            return LogFileName(target);
        }

        private string LogFileName(FileTarget target)
        {
            if (target == null) return null;

            var layout = target.FileName as SimpleLayout;
            if (layout == null) return null;

            var logEvent = new LogEventInfo { TimeStamp = DateTime.Now };
            return target.FileName.Render(logEvent);
        }

        private static T Target<T>(string targetName) where T : Target
        {
            var target = LogManager.Configuration?.FindTargetByName(targetName);
            while ((target != null) && (target is WrapperTargetBase wrapper))
            {
                target = wrapper.WrappedTarget;
            }
            return target as T;
        }
    }
}
