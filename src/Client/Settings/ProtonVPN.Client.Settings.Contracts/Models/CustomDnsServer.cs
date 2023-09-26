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

using System.Diagnostics.CodeAnalysis;

namespace ProtonVPN.Client.Settings.Contracts.Models;

public struct CustomDnsServer : IEquatable<CustomDnsServer>
{
    public string IpAddress { get; set; }

    public bool IsActive { get; set; }

    public CustomDnsServer(string ipAddress, bool isActive)
    {
        IpAddress = ipAddress;
        IsActive = isActive;
    }

    public bool Equals(CustomDnsServer other)
    {
        return string.Equals(IpAddress, other.IpAddress, StringComparison.OrdinalIgnoreCase)
            && IsActive == other.IsActive;
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj?.GetType() != GetType())
        {
            return false;
        }
        return Equals((CustomDnsServer)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(IpAddress?.ToUpperInvariant(), IsActive);
    }
}