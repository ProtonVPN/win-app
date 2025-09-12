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

using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.Countries;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.Common.Core.Extensions;

namespace ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.States;

public class SingleStateLocationIntent : StateLocationIntentBase, ISingleLocationIntent
{
    public static SingleStateLocationIntent From(string countryCode, string stateName)
        => new(SingleCountryLocationIntent.From(countryCode), stateName);
    
    public string StateName { get; }

    public SingleStateLocationIntent(
        SingleCountryLocationIntent country,
        string stateName)
        : base(country)
    {
        StateName = stateName ?? throw new ArgumentNullException(nameof(stateName));
    }

    public override bool IsSameAs(ILocationIntent? intent)
    {
        return base.IsSameAs(intent)
            && intent is SingleStateLocationIntent stateIntent
            && StateName.EqualsIgnoringCase(stateIntent.StateName);
    }

    public override bool IsSupported(Server server)
    {
        return base.IsSupported(server)
            && StateName.EqualsIgnoringCase(server.State);
    }

    public override string ToString()
    {
        return $"{base.ToString()} - State {StateName}";
    }
}