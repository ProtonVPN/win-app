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

namespace ProtonVPN.Update.Responses
{
    public class FileResponse : IEquatable<FileResponse>
    {
        public string Url;

        public string Sha512CheckSum;

        public string Arguments;

        #region IEquatable

        public bool Equals(FileResponse other)
        {
            if (other == null)
            {
                return false;
            }

            return string.Equals(Url, other.Url) &&
                   string.Equals(Sha512CheckSum, other.Sha512CheckSum) &&
                   string.Equals(Arguments, other.Arguments);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals(obj as FileResponse);
        }

        public override int GetHashCode()
        {
            throw new InvalidOperationException();
        }

        #endregion
    }
}