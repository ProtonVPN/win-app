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

using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents;
using ProtonVPN.Client.Logic.Servers.Contracts.Enums;
using ProtonVPN.Client.Logic.Servers.Contracts.Extensions;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.Client.Common.Extensions;
using ProtonVPN.Common.Core.Networking;

namespace ProtonVPN.Client.Logic.Connection.Contracts.Models;

public class ConnectionDetails
{
    public IConnectionIntent OriginalConnectionIntent { get; }

    public Server Server { get; }
    public PhysicalServer PhysicalServer { get; }

    public VpnProtocol Protocol { get;  }

    public DateTime EstablishedConnectionTimeUtc { get; }

    public string ExitCountryCode => Server.ExitCountry;
    public string EntryCountryCode => Server.EntryCountry;
    public string CityState => Server.City;
    public string ServerId => Server.Id;
    public string PhysicalServerId => PhysicalServer.Id;
    public int ServerNumber => Server.Name.GetServerNumber();
    public ServerTiers? ServerTier => (ServerTiers)Server.Tier;
    public string ServerName => Server.Name;
    public double ServerLoad => Server.Load / 100D;
    public TimeSpan? ServerLatency { get; } // TODO: Implement real value
    public bool IsGateway => Server.Features.IsSupported(ServerFeatures.B2B);
    public string GatewayName => Server.GatewayName;

    public ConnectionDetails(IConnectionIntent connectionIntent, Server server, PhysicalServer physicalServer, VpnProtocol protocol)
    {
        OriginalConnectionIntent = connectionIntent;
        EstablishedConnectionTimeUtc = DateTime.UtcNow;
        Server = server;
        PhysicalServer = physicalServer;
        Protocol = protocol;
    }
}