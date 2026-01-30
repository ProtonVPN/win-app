/*
 * Copyright (c) 2026 Proton AG
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

namespace ProtonVPN.Vpn.WireGuard;

public class PersistedServerRoute : IEquatable<PersistedServerRoute>
{
    public string DestinationIpAddress { get; set; } = string.Empty;
    public bool IsIpv6 { get; set; }

    public bool Equals(PersistedServerRoute other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return IsIpv6 == other.IsIpv6 &&
               StringComparer.OrdinalIgnoreCase.Equals(DestinationIpAddress, other.DestinationIpAddress);
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as PersistedServerRoute);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(
            StringComparer.OrdinalIgnoreCase.GetHashCode(DestinationIpAddress ?? string.Empty),
            IsIpv6);
    }
}