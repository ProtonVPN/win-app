/*
 * Copyright (c) 2025 Proton AG
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

namespace ProtonVPN.Common.Core.Vpn;

public readonly struct IpAddressInfo
{
    public string Ipv4Address { get; init; }
    public string Ipv6Address { get; init; }

    public override string ToString()
    {
        return $"IPv4 '{(string.IsNullOrEmpty(Ipv4Address) ? "-" : Ipv4Address)}' " +
               $"IPv6 '{(string.IsNullOrEmpty(Ipv6Address) ? "-" : Ipv6Address)}'";
    }
}