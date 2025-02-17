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

using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.Common.Core.Geographical;

namespace ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;

public class GatewayLocationIntent : LocationIntentBase
{
    public static GatewayLocationIntent Fastest => new(ConnectionIntentKind.Fastest);
    public static GatewayLocationIntent Random => new(ConnectionIntentKind.Random);

    public override bool IsForPaidUsersOnly => false;

    public string? GatewayName { get; }

    public bool IsSpecificGateway => !string.IsNullOrEmpty(GatewayName);

    public bool IsFastestGateway => !IsSpecificGateway && Kind == ConnectionIntentKind.Fastest;

    public bool IsRandomGateway => !IsSpecificGateway && Kind == ConnectionIntentKind.Random;

    public GatewayLocationIntent(
        string gatewayName,
        ConnectionIntentKind kind = ConnectionIntentKind.Fastest)
        : this(kind)
    {
        GatewayName = gatewayName;
    }

    public GatewayLocationIntent(
        ConnectionIntentKind kind = ConnectionIntentKind.Fastest)
        : base(kind)
    { }

    public override bool IsSameAs(ILocationIntent? intent)
    {
        return base.IsSameAs(intent)
            && intent is GatewayLocationIntent gatewayIntent
            && Kind == gatewayIntent.Kind
            && GatewayName == gatewayIntent.GatewayName;
    }

    public override bool IsSupported(Server server, DeviceLocation? deviceLocation)
    {
        return !IsSpecificGateway || server.GatewayName == GatewayName;
    }

    public override string ToString()
    {
        return IsSpecificGateway
            ? $"Gateway {GatewayName}"
            : $"{Kind} Gateway";
    }

    public override bool IsGenericRandomIntent()
    {
        return false;
    }
}