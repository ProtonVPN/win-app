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
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;

namespace ProtonVPN.Client.Logic.Connection.Contracts.Models;

public class ConnectionDetails
{
    public IConnectionIntent OriginalConnectionIntent { get; }

    public DateTime EstablishedConnectionTime { get; }

    public string? CountryCode { get; set; }

    public string? CityState { get; set; }

    public string? ServerName { get; set; }

    public double? ServerLoad { get; set; }

    public TimeSpan? ServerLatency { get; set; }

    public string? Protocol { get; set; }

    public IEnumerable<string> Features { get; }

    public ConnectionDetails(IConnectionIntent connectionIntent)
    {
        EstablishedConnectionTime = DateTime.UtcNow;

        OriginalConnectionIntent = connectionIntent;

        Features = new List<string>();    

        MockConnectionDetails();
    }

    private void MockConnectionDetails()
    {
        CountryCode = (OriginalConnectionIntent.Location as CountryLocationIntent)?.CountryCode;
        CityState = (OriginalConnectionIntent.Location as CityStateLocationIntent)?.CityState;

        int? serverNumber = (OriginalConnectionIntent.Location as ServerLocationIntent)?.ServerNumber ?? (OriginalConnectionIntent.Location as FreeServerLocationIntent)?.ServerNumber;
        ServerName = !string.IsNullOrEmpty(CountryCode) && serverNumber != null
            ? $"{CountryCode}#{serverNumber}" : null;
        
        ServerLoad = 0.4;
        ServerLatency = TimeSpan.FromMilliseconds(46);
        Protocol = "WireGuard";
    }
}