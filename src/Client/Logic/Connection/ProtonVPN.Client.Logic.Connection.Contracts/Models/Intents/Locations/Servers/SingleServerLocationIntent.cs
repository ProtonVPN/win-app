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
using ProtonVPN.Client.Logic.Servers.Contracts.Models;

namespace ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.Servers;

public class SingleServerLocationIntent : ServerLocationIntentBase, ISingleLocationIntent
{
    public static SingleServerLocationIntent From(string countryCode, string stateName, string cityName, ServerInfo server)
        => new(SingleCityLocationIntent.From(countryCode, stateName, cityName), server);

    public static SingleServerLocationIntent From(string countryCode, string cityName, ServerInfo server)
        => new(SingleCityLocationIntent.From(countryCode, cityName), server);

    public ServerInfo Server { get; }

    public SingleServerLocationIntent(
        SingleCityLocationIntent city,
        ServerInfo server)
        : base(city)
    {
        Server = server;
    }

    public override bool IsSameAs(ILocationIntent? intent)
    {
        return base.IsSameAs(intent)
            && intent is SingleServerLocationIntent serverIntent
            && Server == serverIntent.Server;
    }

    public override bool IsSupported(Server server)
    {
        return base.IsSupported(server)
            && server.Id == Server.Id;
    }

    public override string ToString()
    {
        return $"{base.ToString()} - Server {Server.Name}";
    }
}