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

using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ProtonVPN.BugReporting.Attachments.Filters
{
    internal class FileLengthAttachmentFilter: IEnumerable<Attachment>
    {
        private readonly ILogger _logger;
        private readonly IEnumerable<Attachment> _source;

        public FileLengthAttachmentFilter(ILogger logger, IEnumerable<Attachment> source)
        {
            _logger = logger;
            _source = source;
        }

        public IEnumerator<Attachment> GetEnumerator() => _source.Select(Filtered).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private Attachment Filtered(Attachment item)
        {
            return item.HasError() ? item : WithLength(item);
        }

        private Attachment WithLength(Attachment item) => 
            WithError(item.WithLength(SafeFileLength(item.Path)));

        private Attachment WithError(Attachment item) => 
            item.Length < 0 ? item.WithError(AttachmentErrorType.FileReadError) : item;

        private long SafeFileLength(string filename)
        {
            try
            {
                return FileLength(filename);
            }
            catch (Exception e) when (e.IsFileAccessException())
            {
                _logger.Warn("Failed to get file length: " + e.CombinedMessage());
                return -1;
            }
        }

        private long FileLength(string filename) => new FileInfo(filename).Length;
    }
}
