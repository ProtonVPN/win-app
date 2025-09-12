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

using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.Common.Core.Extensions;

namespace ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.Gateways;

public class SingleGatewayLocationIntent : GatewayLocationIntentBase, ISingleLocationIntent
{
    public static SingleGatewayLocationIntent From(string gatewayName)
        => new(gatewayName);

    public string GatewayName { get; }

    public SingleGatewayLocationIntent(
        string gatewayName)
    {
        GatewayName = gatewayName ?? throw new ArgumentNullException(nameof(gatewayName));
    }

    public override bool IsSameAs(ILocationIntent? intent)
    {
        return base.IsSameAs(intent)
            && intent is SingleGatewayLocationIntent gatewayIntent
            && GatewayName.EqualsIgnoringCase(gatewayIntent.GatewayName);
    }

    public override bool IsSupported(Server server)
    {
        return GatewayName.EqualsIgnoringCase(server.GatewayName);
    }

    public override string ToString()
    {
        return $"Gateway {GatewayName}";
    }
}