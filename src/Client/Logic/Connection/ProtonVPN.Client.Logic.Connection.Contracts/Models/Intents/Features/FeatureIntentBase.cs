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

namespace ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;

public abstract class FeatureIntentBase : IntentBase, IFeatureIntent
{
    public virtual bool IsSameAs(IFeatureIntent? intent)
    {
        if (intent == null)
        {
            return false;
        }

        return GetType() == intent.GetType();
    }

    public IEnumerable<Server> FilterServers(IEnumerable<Server> servers)
    {
        return servers.Where(IsSupported);
    }

    public abstract bool IsSupported(Server server);

    public IFeatureIntent Copy()
    {
        return (IFeatureIntent)MemberwiseClone();
    }
}