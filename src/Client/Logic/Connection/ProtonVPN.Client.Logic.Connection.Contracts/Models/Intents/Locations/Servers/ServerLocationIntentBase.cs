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

using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.Cities;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.Countries;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.States;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;

namespace ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.Servers;

public abstract class ServerLocationIntentBase : LocationIntentBase
{
    public SingleCountryLocationIntent Country { get; }
    public SingleStateLocationIntent? State { get; }
    public SingleCityLocationIntent City { get; }

    public override bool IsForPaidUsersOnly => true;

    protected ServerLocationIntentBase(SingleCityLocationIntent city)
    {
        City = city ?? throw new ArgumentNullException(nameof(city));
        State = city.State;
        Country = city.Country;
    }

    public override bool IsSameAs(ILocationIntent? intent)
    {
        return base.IsSameAs(intent)
            && intent is ServerLocationIntentBase serverIntent
            && City.IsSameAs(serverIntent.City);
    }

    public override bool IsSupported(Server server)
    {
        return City.IsSupported(server);
    }

    public override string ToString()
    {
        return City.ToString();
    }
}