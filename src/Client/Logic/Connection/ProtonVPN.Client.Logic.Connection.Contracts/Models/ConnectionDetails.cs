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
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Common.Core.Networking;

namespace ProtonVPN.Client.Logic.Connection.Contracts.Models;

public class ConnectionDetails
{
    public IConnectionIntent OriginalConnectionIntent { get; }

    public DateTime EstablishedConnectionTime { get; }
    public string? CountryCode { get; set; }
    public string? CityState { get; set; }
    public string? ServerId { get; set; }
    public string? ServerName { get; set; }
    public double? ServerLoad { get; set; }
    public TimeSpan? ServerLatency { get; set; }
    public VpnProtocol Protocol { get; set; }
    public bool IsGateway { get; }

    public ConnectionDetails(IConnectionIntent connectionIntent, Server? server = null, VpnProtocol vpnProtocol = VpnProtocol.Smart)
    {
        EstablishedConnectionTime = DateTime.UtcNow;
        OriginalConnectionIntent = connectionIntent;
        CountryCode = server?.ExitCountry;
        CityState = server?.City;
        ServerId = server?.Id;
        ServerName = server?.Name;
        ServerLoad = server?.Load / 100D;
        ServerLatency = TimeSpan.FromMilliseconds(46); // TODO: implement real value
        Protocol = vpnProtocol;
        IsGateway = server?.Features.IsSupported(ServerFeatures.B2B) ?? false;
    }
}