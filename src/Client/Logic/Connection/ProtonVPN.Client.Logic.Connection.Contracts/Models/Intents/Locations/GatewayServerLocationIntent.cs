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

using ProtonVPN.Client.Logic.Connection.Contracts.Extensions;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;

namespace ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;

public class GatewayServerLocationIntent : GatewayLocationIntent
{
    public string Id { get; }
    public string Name { get; }
    public int Number { get; }
    public string CountryCode { get; }

    public GatewayServerLocationIntent(string id, string name, string countryCode, string gatewayName)
        : base(gatewayName)
    {
        Id = id;
        Name = name;
        CountryCode = countryCode;
        Number = this.GetServerNumber();
    }

    public override bool IsSameAs(ILocationIntent? intent)
    {
        return base.IsSameAs(intent)
            && intent is GatewayServerLocationIntent serverIntent
            && Id == serverIntent.Id;
    }

    public override bool IsSupported(Server server)
    {
        return server.Id == Id;
    }

    public override string ToString()
    {
        return $"{base.ToString()} - {Name}";
    }
}