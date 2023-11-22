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

using System.Text.RegularExpressions;
using ProtonVPN.Client.Logic.Servers.Contracts;

namespace ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;

public class ServerLocationIntent : CityStateLocationIntent
{
    public string Id { get; }
    public string Name { get; }
    public int Number { get; }

    public ServerLocationIntent(string id, string name, string countryCode, string cityState)
        : base(countryCode, cityState)
    {
        Id = id;
        Name = name;
        Number = GetNumber();
    }

    public override bool IsSameAs(ILocationIntent? intent)
    {
        return base.IsSameAs(intent)
            && intent is ServerLocationIntent serverIntent
            && Id == serverIntent.Id;
    }

    public override IEnumerable<Server> FilterServers(IEnumerable<Server> servers)
    {
        return servers.Where(s => s.Id == Id);
    }

    private int GetNumber()
    {
        Match match = new Regex(@"#(\d+)").Match(Name);
        if (match.Success)
        {
            string numberString = match.Groups[1].Value;
            return int.Parse(numberString);
        }

        return 0;
    }
}