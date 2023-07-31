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
using File = ProtonVPN.Api.Contracts.File;

namespace ProtonVPN.Client.Logic.Feedback.Attachments;

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

    private File ToFile(Attachment attachment)
    {
        return new(attachment.Name, FileContent(attachment.Path));
    }

    private byte[] FileContent(string filename)
    {
        using (FileStream fs = new(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        {
            int length = (int)fs.Length;
            byte[] buffer = new byte[length];
            fs.Read(buffer, 0, length);
            return buffer;
        }
    }
}