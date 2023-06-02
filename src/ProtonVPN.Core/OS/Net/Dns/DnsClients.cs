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
using System.Linq;
using System.Net;
using DnsClient;
using ProtonVPN.Common.Extensions;

namespace ProtonVPN.Core.OS.Net.Dns
{
    public class DnsClients : IDnsClients
    {
        public IDnsClient DnsClient(IReadOnlyCollection<IPEndPoint> nameServers)
        {
            return nameServers.Any() 
                ? new FixedDnsClient(new LookupClient(nameServers.ToArray())) 
                : NullDnsClient;
        }

        public IReadOnlyCollection<IPEndPoint> NameServers()
        {
            return NameServer.ResolveNameServers(true, false)
                .Select(server => new IPEndPoint(server.Address.ToIPAddressBytes(), server.Port))
                .ToList();
        }

        public static IDnsClient NullDnsClient { get; } = new NullDnsClient();
    }
}