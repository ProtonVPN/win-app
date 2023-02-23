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

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProtonVPN.Api.Contracts;

namespace ProtonVPN.BugReporting.Attachments
{
    public class AttachmentsToApiFiles : IEnumerable<File>
    {
        private readonly IEnumerable<Attachment> _source;

        public AttachmentsToApiFiles(IEnumerable<Attachment> source)
        {
            _source = source;
        }

        public IEnumerator<File> GetEnumerator()
        {
            return _source.Select(ToFile).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private File ToFile(Attachment attachment) => new(attachment.Name, FileContent(attachment.Path));

        private byte[] FileContent(string filename) => new Core.OS.FileSystem.File(filename).Content();
    }
}
