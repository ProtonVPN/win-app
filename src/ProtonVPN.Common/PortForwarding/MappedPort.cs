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

namespace ProtonVPN.Common.PortForwarding
{
    public class MappedPort : IEquatable<MappedPort>, IComparable<MappedPort>
    {
        public int InternalPort { get; }
        public int ExternalPort { get; }

        public MappedPort(int internalPort, int externalPort)
        {
            InternalPort = internalPort;
            ExternalPort = externalPort;
        }

        public bool Equals(MappedPort other)
        {
            return InternalPort == other?.InternalPort && ExternalPort == other.ExternalPort;
        }

        public override bool Equals(object other)
        {
            return other is MappedPort port && Equals(port);
        }

        public override int GetHashCode()
        {
            return InternalPort.GetHashCode() ^ ExternalPort.GetHashCode();
        }

        public static bool operator ==(MappedPort left, MappedPort right)
        {
            return IsEqual(left, right);
        }

        private static bool IsEqual(MappedPort left, MappedPort right)
        {
            return (left is null && right is null) || (left is not null && left.Equals(right));
        }

        public static bool operator !=(MappedPort left, MappedPort right)
        {
            return !IsEqual(left, right);
        }

        public int CompareTo(MappedPort other)
        {
            if (ReferenceEquals(this, other))
            {
                return 0;
            }
            if (other is null)
            {
                return 1;
            }
            int internalPortComparison = InternalPort.CompareTo(other.InternalPort);
            return internalPortComparison != 0 ? internalPortComparison : ExternalPort.CompareTo(other.ExternalPort);
        }

        public override string ToString()
        {
            return $"{InternalPort}->{ExternalPort}";
        }
    }
}