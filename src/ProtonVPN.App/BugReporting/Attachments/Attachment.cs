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
using ProtonVPN.Common.Helpers;

namespace ProtonVPN.BugReporting.Attachments
{
    public class Attachment : IEquatable<Attachment>
    {
        public string Name { get; }

        public string Path { get; }

        public long Length { get; }

        public Attachment(string filePath) : this(System.IO.Path.GetFileName(filePath), filePath, 0)
        { }

        private Attachment(string name, string path, long length)
        {
            Ensure.NotEmpty(name, nameof(name));
            Ensure.NotEmpty(path, nameof(path));

            Name = name;
            Path = path;
            Length = length;
        }

        public Attachment WithLength(long length)
        {
            return new Attachment(Name, Path, length);
        }

        public bool Equals(Attachment other)
        {
            return Path == other?.Path;
        }
    }
}
