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

using ProtonVPN.Client.Common.Extensions;

namespace ProtonVPN.Client.Logic.Connection.Contracts.Models;

public struct GatewayServerInfo : IEquatable<GatewayServerInfo>
{
    public static GatewayServerInfo From(string id, string name, string countryCode) => new()
    {
        Id = id ?? throw new ArgumentNullException(nameof(id)),
        Name = name ?? throw new ArgumentNullException(nameof(name)),
        CountryCode = countryCode ?? throw new ArgumentNullException(nameof(countryCode))
    };

    private int? _number;

    public required string Id { get; init; }

    public required string Name { get; init; }

    public required string CountryCode { get; init; }

    public int GetNumber()
    {
        _number ??= Name.GetServerNumber();

        return _number.Value;
    }

    public static bool operator ==(GatewayServerInfo left, GatewayServerInfo right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(GatewayServerInfo left, GatewayServerInfo right)
    {
        return !(left == right);
    }

    public readonly bool Equals(GatewayServerInfo other)
    {
        return Id == other.Id 
            && Name == other.Name 
            && CountryCode == other.CountryCode;
    }

    public override readonly bool Equals(object? obj)
    {
        return obj is GatewayServerInfo other && Equals(other);
    }

    public override readonly int GetHashCode()
    {
        return HashCode.Combine(Id, Name, CountryCode);
    }
}