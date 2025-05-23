﻿/*
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

using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.Common.Core.Geographical;

namespace ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;

public class CityLocationIntent : StateLocationIntent
{
    public string City { get; }

    public CityLocationIntent(string countryCode, string state, string city)
        : base(countryCode, state)
    {
        City = city;
    }

    public CityLocationIntent(string countryCode, string city)
        : base(countryCode)
    {
        City = city;
    }

    public override bool IsSameAs(ILocationIntent? intent)
    {
        return base.IsSameAs(intent)
            && intent is CityLocationIntent cityIntent
            && City == cityIntent.City;
    }

    public override bool IsSupported(Server server, DeviceLocation? deviceLocation)
    {
        return base.IsSupported(server, deviceLocation) && server.City == City;
    }

    public override string ToString()
    {
        return $"{base.ToString()}{(string.IsNullOrEmpty(City) ? string.Empty : $" - {City}")}";
    }
}