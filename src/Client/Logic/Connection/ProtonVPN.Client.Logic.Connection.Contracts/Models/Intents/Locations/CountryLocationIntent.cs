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

using System.Text;
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.Common.Core.Geographical;

namespace ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;

public class CountryLocationIntent : LocationIntentBase
{
    public static CountryLocationIntent Fastest => new();
    public static CountryLocationIntent FastestExcludingMyCountry => new(ConnectionIntentKind.Fastest, true);
    public static CountryLocationIntent Random => new(ConnectionIntentKind.Random);

    public override bool IsForPaidUsersOnly => true;

    public string? CountryCode { get; }

    public bool IsToExcludeMyCountry { get; }

    public bool IsSpecificCountry => !string.IsNullOrEmpty(CountryCode);

    public bool IsFastestCountry => !IsSpecificCountry && Kind == ConnectionIntentKind.Fastest && !IsToExcludeMyCountry;

    public bool IsFastestCountryExcludingMine => !IsSpecificCountry && Kind == ConnectionIntentKind.Fastest && IsToExcludeMyCountry;

    public bool IsRandomCountry => !IsSpecificCountry && Kind == ConnectionIntentKind.Random && !IsToExcludeMyCountry;

    public bool IsRandomCountryExcludingMine => !IsSpecificCountry && Kind == ConnectionIntentKind.Random && IsToExcludeMyCountry;

    public CountryLocationIntent(
        string countryCode,
        ConnectionIntentKind kind = ConnectionIntentKind.Fastest,
        bool isToExcludeMyCountry = false)
        : this(kind, isToExcludeMyCountry)
    {
        if (!string.IsNullOrWhiteSpace(countryCode))
        {
            CountryCode = countryCode.ToUpperInvariant();
        }
    }

    public CountryLocationIntent(
        ConnectionIntentKind kind = ConnectionIntentKind.Fastest,
        bool isToExcludeMyCountry = false)
        : base(kind)
    {
        IsToExcludeMyCountry = isToExcludeMyCountry;
    }

    public override bool IsSameAs(ILocationIntent? intent)
    {
        return base.IsSameAs(intent)
            && intent is CountryLocationIntent countryIntent
            && CountryCode == countryIntent.CountryCode
            && Kind == countryIntent.Kind
            && IsToExcludeMyCountry == countryIntent.IsToExcludeMyCountry;
    }

    public override bool IsSupported(Server server, DeviceLocation? deviceLocation)
    {
        return (CountryCode is null || server.ExitCountry == CountryCode) &&
            (!IsToExcludeMyCountry || string.IsNullOrWhiteSpace(deviceLocation?.CountryCode) || (server.ExitCountry != deviceLocation?.CountryCode && server.EntryCountry != deviceLocation?.CountryCode));
    }

    public override string ToString()
    {
        StringBuilder builder = new();
        builder.Append($"{Kind} ");
        builder.Append(CountryCode is null ? $"country" : $"in {CountryCode}");
        builder.Append(IsToExcludeMyCountry ? " (Exclude my country)" : string.Empty);
        return builder.ToString();
    }

    public override bool IsGenericRandomIntent()
    {
        return IsRandomCountry || IsRandomCountryExcludingMine;
    }
}