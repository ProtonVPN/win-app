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

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ProtonVPN.Api.Contracts.Geographical;
using ProtonVPN.Core.Servers.Name;

namespace ProtonVPN.Core.Servers.Models
{
    public class Server : IEquatable<Server>
    {
        public string Id { get; }
        public string Name { get; }
        public string City { get; }
        public string EntryCountry { get; }
        public string ExitCountry { get; }
        public string Domain { get; }
        public sbyte Status { get; }
        public int Tier { get; }
        public sbyte Features { get; }
        public int Load { get; }
        public float Score { get; }
        public LocationResponse LocationResponse { get; }
        public string ExitIp { get; set; }
        public IReadOnlyList<PhysicalServer> Servers { get; }

        public Server(
            string id,
            string name,
            string city,
            string entryCountry,
            string exitCountry,
            string domain,
            sbyte status,
            int tier,
            sbyte features,
            sbyte load,
            float score,
            LocationResponse locationResponse,
            IReadOnlyList<PhysicalServer> physicalServers,
            string exitIp)
        {
            Id = id;
            Name = name;
            City = city;
            EntryCountry = entryCountry;
            ExitCountry = exitCountry;
            Domain = domain;
            Status = status;
            Tier = tier;
            Features = features;
            Load = load;
            Score = score;
            LocationResponse = locationResponse;
            Servers = physicalServers;
            ExitIp = exitIp;
        }

        public IName GetServerName()
        {
            if (this.IsSecureCore())
            {
                return new SecureCoreName
                {
                    EntryCountryCode = EntryCountry,
                    ExitCountryCode = ExitCountry
                };
            }

            return new StandardServerName { Name = Name };
        }

        public IName GetNameWithCountry()
        {
            return new StandardServerName
            {
                Name = Name,
                EntryCountryCode = EntryCountry
            };
        }

        public bool IsFree()
        {
            return Tier == ServerTiers.Free;
        }

        public bool IsPhysicalFree()
        {
            string number = Name.Substring(Name.IndexOf('#') + 1);
            number = Regex.Match(number, @"\d+").Value;

            return Tier == ServerTiers.Free || !string.IsNullOrEmpty(number) && int.Parse(number) >= 100;
        }

        public bool MatchesSearchCriteria(string query)
        {
            query = query.ToLower();
            return Name.ToLower().Contains(query) || (!string.IsNullOrEmpty(City) && City.ToLower().Contains(query));
        }

        public Server Clone()
        {
            return (Server) MemberwiseClone();
        }

        public bool IsEmpty()
        {
            return Equals(Empty());
        }
        
        public bool IsFeatureSupported(Features feature)
        {
            switch (feature)
            {
                case Core.Servers.Features.SecureCore:
                    return ServerFeatures.IsSecureCore(Features);
                case Core.Servers.Features.Tor:
                    return ServerFeatures.SupportsTor(Features);
                case Core.Servers.Features.P2P:
                    return ServerFeatures.SupportsP2P(Features);
                default:
                    return true;
            }
        }

        public static Server Empty() =>
            new Server(
                "",
                "ZZ#0",
                "",
                "ZZ",
                "ZZ",
                "",
                0,
                0,
                0,
                0,
                0,
                new LocationResponse { Lat = 0f, Long = 0f },
                new List<PhysicalServer>(0),
                null);

        #region Equatable

        public bool Equals(Server other)
        {
            return Id == other?.Id;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Server);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        #endregion
    }
}
