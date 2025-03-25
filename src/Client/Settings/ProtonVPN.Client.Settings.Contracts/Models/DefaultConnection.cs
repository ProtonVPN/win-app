/*
 * Copyright (c) 2024 Proton AG
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

using ProtonVPN.Client.Settings.Contracts.Enums;

namespace ProtonVPN.Client.Settings.Contracts.Models;

public struct DefaultConnection
{
    public static DefaultConnection Fastest => new(DefaultConnectionType.Fastest);
    public static DefaultConnection Random => new(DefaultConnectionType.Random);
    public static DefaultConnection Last => new(DefaultConnectionType.Last);

    public DefaultConnectionType Type { get; init; }
    public Guid RecentId { get; init; } = Guid.Empty;

    public DefaultConnection(Guid recentId)
        : this(DefaultConnectionType.Recent)
    {
        RecentId = recentId;
    }

    private DefaultConnection(DefaultConnectionType type)
    {
        Type = type;
    }

    public override bool Equals(object? obj) => obj is DefaultConnection other && Equals(other);
    public bool Equals(DefaultConnection dc) => Type == dc.Type && RecentId == dc.RecentId;
    public override int GetHashCode() => (Type, RecentId).GetHashCode();
    public static bool operator ==(DefaultConnection left, DefaultConnection right) => left.Equals(right);
    public static bool operator !=(DefaultConnection left, DefaultConnection right) => !(left == right);

    public override string ToString()
    {
        string value = Type.ToString();

        if (Type == DefaultConnectionType.Recent)
        {
            value += $" {RecentId}";
        }

        return value;
    }
}