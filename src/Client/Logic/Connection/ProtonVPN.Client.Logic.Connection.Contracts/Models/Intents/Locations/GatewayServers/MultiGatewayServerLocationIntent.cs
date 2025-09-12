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
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.Gateways;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;

namespace ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.GatewayServers;

public class MultiGatewayServerLocationIntent : GatewayServerLocationIntentBase, IMultiLocationIntent
{
    public static MultiGatewayServerLocationIntent From(string gatewayName, IEnumerable<GatewayServerInfo> servers, SelectionStrategy strategy)
        => new(SingleGatewayLocationIntent.From(gatewayName), servers, strategy);

    public IReadOnlyList<GatewayServerInfo> Servers { get; }

    public SelectionStrategy Strategy { get; }

    public bool IsSelectionEmpty => Servers.Count == 0;

    public MultiGatewayServerLocationIntent(
        SingleGatewayLocationIntent gateway,
        IEnumerable<GatewayServerInfo> servers,
        SelectionStrategy strategy = SelectionStrategy.Fastest)
        : base(gateway)
    {
        Servers = servers.Distinct().OrderBy(s => s.Id).ToList();
        Strategy = strategy;
    }

    public MultiGatewayServerLocationIntent(
        SingleGatewayLocationIntent gateway,
        SelectionStrategy strategy = SelectionStrategy.Fastest)
        : this(gateway, [], strategy)
    { }

    public override bool IsSameAs(ILocationIntent? intent)
    {
        return base.IsSameAs(intent)
            && intent is MultiGatewayServerLocationIntent multiGatewayServerIntent
            && Strategy == multiGatewayServerIntent.Strategy
            && Servers.SequenceEqual(multiGatewayServerIntent.Servers);
    }

    public override bool IsSupported(Server server)
    {
        return base.IsSupported(server)
            && (IsSelectionEmpty || Servers.Any(s => s.Id == server.Id));
    }

    public override string ToString()
    {
        return $"{base.ToString()} - {Strategy} server{(IsSelectionEmpty ? string.Empty : $" in {string.Join(", ", Servers.Select(s => s.Name))}")}";
    }
}