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

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ProtonVPN.BugReporting.Attachments.Source
{
    internal class LogFileSource : IEnumerable<string>
    {
        private readonly string _path;
        private readonly int _count;

        public LogFileSource(string path, int count)
        {
            _path = path;
            _count = count;
        }

        public IEnumerator<string> GetEnumerator()
        {
            return FileNames().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<string> FileNames()
        {
            var directory = new DirectoryInfo(_path);
            var fileNames = directory
                .GetFiles()
                .OrderByDescending(p => p.LastWriteTimeUtc)
                .Take(_count)
                .Select(f => f.FullName);

            return fileNames;
        }
    }
}
