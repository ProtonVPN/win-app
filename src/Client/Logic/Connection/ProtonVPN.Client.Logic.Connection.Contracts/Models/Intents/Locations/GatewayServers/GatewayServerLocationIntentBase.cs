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

using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.Gateways;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;

namespace ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.GatewayServers;

public abstract class GatewayServerLocationIntentBase : LocationIntentBase
{
    public SingleGatewayLocationIntent Gateway { get; }

    public override bool IsForPaidUsersOnly => false;

    protected GatewayServerLocationIntentBase(SingleGatewayLocationIntent gateway)
    {
        Gateway = gateway ?? throw new ArgumentNullException(nameof(gateway));
    }

    public override bool IsSameAs(ILocationIntent? intent)
    {
        return base.IsSameAs(intent)
            && intent is GatewayServerLocationIntentBase gatewayServerIntent
            && Gateway.IsSameAs(gatewayServerIntent.Gateway);
    }

    public override bool IsSupported(Server server)
    {
        return Gateway.IsSupported(server);
    }

    public override string ToString()
    {
        return Gateway.ToString();
    }
}