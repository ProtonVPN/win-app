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
using ProtonVPN.Client.Logic.Servers.Contracts.Models;

namespace ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.Gateways;

public class MultiGatewayLocationIntent : GatewayLocationIntentBase, IMultiLocationIntent
{
    public static MultiGatewayLocationIntent Default => Fastest;
    public static MultiGatewayLocationIntent Fastest => From(SelectionStrategy.Fastest);
    public static MultiGatewayLocationIntent Random => From(SelectionStrategy.Random);
    public static MultiGatewayLocationIntent From(SelectionStrategy strategy) => new(strategy);
    public static MultiGatewayLocationIntent From(IEnumerable<string> gatewayNames, SelectionStrategy strategy) => new(gatewayNames, strategy);

    public IReadOnlyList<string> GatewayNames { get; }

    public SelectionStrategy Strategy { get; }

    public bool IsSelectionEmpty => GatewayNames.Count == 0;

    public MultiGatewayLocationIntent(
        IEnumerable<string> gatewayNames,
        SelectionStrategy strategy = SelectionStrategy.Fastest)
    {
        GatewayNames = gatewayNames.Distinct().OrderBy(c => c).ToList();
        Strategy = strategy;
    }

    public MultiGatewayLocationIntent(
        SelectionStrategy strategy = SelectionStrategy.Fastest)
        : this([], strategy)
    { }

    public override bool IsSameAs(ILocationIntent? intent)
    {
        return base.IsSameAs(intent)
            && intent is MultiGatewayLocationIntent multiGatewayIntent
            && Strategy == multiGatewayIntent.Strategy
            && GatewayNames.SequenceEqual(multiGatewayIntent.GatewayNames, StringComparer.OrdinalIgnoreCase);
    }

    public override bool IsSupported(Server server)
    {
        return IsSelectionEmpty || GatewayNames.Contains(server.GatewayName, StringComparer.OrdinalIgnoreCase);
    }

    public override string ToString()
    {
        return $"{Strategy} Gateway{(IsSelectionEmpty ? string.Empty : $" in {string.Join(", ", GatewayNames)}")}";
    }
}