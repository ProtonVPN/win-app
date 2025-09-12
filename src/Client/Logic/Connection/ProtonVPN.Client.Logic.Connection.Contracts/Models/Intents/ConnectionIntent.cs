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

using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.Countries;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.FreeServers;

namespace ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents;

public class ConnectionIntent : ConnectionIntentBase, IConnectionIntent
{
    public static IConnectionIntent Default => new ConnectionIntent(MultiCountryLocationIntent.Default);
    public static IConnectionIntent FreeDefault => new ConnectionIntent(FreeServerLocationIntent.Default);

    public ConnectionIntent(ILocationIntent location, IFeatureIntent? feature = null)
        : base(location, feature)
    { }

    public override bool IsSameAs(IConnectionIntent? intent)
    {
        if (intent == null)
        {
            return false;
        }

        return intent is ConnectionIntent
            // Check whether both location are identical and both feature are null or identical
            && Location.IsSameAs(intent.Location)
            && ((Feature == null && intent.Feature == null) || Feature?.IsSameAs(intent.Feature) == true);
    }
}