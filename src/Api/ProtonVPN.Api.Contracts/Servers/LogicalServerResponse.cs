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

using System.Collections.Generic;
using Newtonsoft.Json;
using ProtonVPN.Api.Contracts.Geographical;

namespace ProtonVPN.Api.Contracts.Servers
{
    public class LogicalServerResponse
    {
        [JsonProperty("ID")]
        public string Id { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
        public string EntryCountry { get; set; }
        public string ExitCountry { get; set; }
        public string Domain { get; set; }
        public sbyte Tier { get; set; }
        public sbyte Features { get; set; }
        public LocationResponse LocationResponse { get; set; }
        public sbyte Status { get; set; }
        public sbyte Load { get; set; }
        public float Score { get; set; }
        public string HostCountry { get; set; }
        public List<PhysicalServerResponse> Servers { get; set; }

        public static LogicalServerResponse Empty => new()
        {
            Id = string.Empty,
            Name = "Server removed",
            City = string.Empty,
            EntryCountry = "ZZ",
            ExitCountry = "ZZ",
            Domain = string.Empty,
            Tier = 0,
            Features = 0,
            LocationResponse = new LocationResponse
            {
                Lat = 0,
                Long = 0
            },
            Status = 0,
            Load = 0,
            Score = 0F,
            Servers = new List<PhysicalServerResponse>(0)
        };
    }
}