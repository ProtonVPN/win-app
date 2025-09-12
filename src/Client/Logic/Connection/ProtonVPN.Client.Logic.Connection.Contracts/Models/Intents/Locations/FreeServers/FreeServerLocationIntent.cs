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

using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Servers.Contracts.Enums;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;

namespace ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.FreeServers;

public class FreeServerLocationIntent : LocationIntentBase, IMultiLocationIntent
{
    public static FreeServerLocationIntent Default => Fastest;
    public static FreeServerLocationIntent Fastest => new();
    public static FreeServerLocationIntent Random(ServerInfo? serverToExclude) => new(serverToExclude);

    public SelectionStrategy Strategy { get; }

    public ServerInfo? ServerToExclude { get; }

    public override bool IsForPaidUsersOnly => false;

    private FreeServerLocationIntent(SelectionStrategy strategy)
    {
        Strategy = strategy;
    }

    protected FreeServerLocationIntent()
        : this(SelectionStrategy.Fastest)
    { }

    protected FreeServerLocationIntent(ServerInfo? serverToExclude)
        : this(SelectionStrategy.Random)
    {
        ServerToExclude = serverToExclude;
    }

    public override bool IsSameAs(ILocationIntent? intent)
    {
        return base.IsSameAs(intent)
            && intent is FreeServerLocationIntent freeServerIntent
            && Strategy == freeServerIntent.Strategy
            && ServerToExclude == freeServerIntent.ServerToExclude;
    }

    public override bool IsSupported(Server server)
    {
        return server.Tier == ServerTiers.Free
            && ServerToExclude?.Id != server.Id;
    }

    public override string ToString()
    {
        string intent = $"{Strategy} free server";

        if (ServerToExclude.HasValue)
        {
            intent += $" (excluding {ServerToExclude.Value.Name})";
        }

        return intent;
    }
}