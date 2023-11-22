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

using ProtonVPN.Client.Logic.Servers.Contracts;

namespace ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;

public class CityStateLocationIntent : CountryLocationIntent
{
    public string CityState { get; }

    public CityStateLocationIntent(string countryCode, string cityState)
        : base(countryCode)
    {
        CityState = cityState;
    }

    public override bool IsSameAs(ILocationIntent? intent)
    {
        return base.IsSameAs(intent)
            && intent is CityStateLocationIntent cityStateIntent
            && CityState == cityStateIntent.CityState;
    }

    public override IEnumerable<Server> FilterServers(IEnumerable<Server> servers)
    {
        return base.FilterServers(servers).Where(s => s.City == CityState);
    }
}