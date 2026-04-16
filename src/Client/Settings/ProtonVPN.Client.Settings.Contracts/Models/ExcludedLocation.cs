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

using System.Diagnostics.CodeAnalysis;
using ProtonVPN.Client.Settings.Contracts.Enums;

namespace ProtonVPN.Client.Settings.Contracts.Models;

public struct ExcludedLocation : IEquatable<ExcludedLocation>
{
    public ExcludedLocationType Type { get; set; }
    public string CountryCode { get; set; }
    public string? StateName { get; set; }
    public string? CityName { get; set; }

    public ExcludedLocation(string countryCode)
    {
        Type = ExcludedLocationType.Country;
        CountryCode = countryCode;
        StateName = null;
        CityName = null;
    }

    public ExcludedLocation(string countryCode, string stateName)
    {
        Type = ExcludedLocationType.State;
        CountryCode = countryCode;
        StateName = stateName;
        CityName = null;
    }

    public ExcludedLocation(string countryCode, string? stateName, string cityName)
    {
        Type = ExcludedLocationType.City;
        CountryCode = countryCode;
        StateName = stateName;
        CityName = cityName;
    }

    public bool Equals(ExcludedLocation other)
    {
        return Type == other.Type
            && string.Equals(CountryCode, other.CountryCode, StringComparison.OrdinalIgnoreCase)
            && string.Equals(StateName, other.StateName, StringComparison.OrdinalIgnoreCase)
            && string.Equals(CityName, other.CityName, StringComparison.OrdinalIgnoreCase);
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj?.GetType() != GetType())
        {
            return false;
        }
        return Equals((ExcludedLocation)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(
            Type,
            CountryCode?.ToUpperInvariant(),
            StateName?.ToUpperInvariant(),
            CityName?.ToUpperInvariant());
    }
}
