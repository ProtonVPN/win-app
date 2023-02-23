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

using ProtonVPN.Common.Helpers;
using System;

namespace ProtonVPN.Core.Settings.Contracts
{
    public class SplitTunnelingApp: IEquatable<SplitTunnelingApp>
    {
        public string Name { get; set; }

        public string Path { get; set; }

        public string[] AdditionalPaths { get; set; }

        public bool Enabled { get; set; }

        #region IEquatable

        public bool Equals(SplitTunnelingApp other)
        {
            if (other == null)
                return false;

            return string.Equals(Path, other.Path, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals(obj as SplitTunnelingApp);
        }

        public override int GetHashCode()
        {
            return HashCode.Start
                .Hash(Path?.ToUpperInvariant());
        }

        #endregion
    }
}
