/*
 * Copyright (c) 2020 Proton Technologies AG
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
using System.Reflection;
using DnsClient;

namespace ProtonVPN.Core.OS.Net.Dns
{
    public class DnsClients : IDnsClients
    {
        public IDnsClient DnsClient()
        {
            return new FixedDnsClient(new LookupClient().WithDisabledSocketsReuse());
        }

        public IDnsClient DnsClient(IReadOnlyCollection<IPEndPoint> nameServers)
        {
            return nameServers.Any() 
                ? new FixedDnsClient(new LookupClient(nameServers.ToArray()).WithDisabledSocketsReuse()) 
                : NullDnsClient;
        }

        public IReadOnlyCollection<IPEndPoint> NameServers()
        {
            return NameServer.ResolveNameServers(true, false).ToArray();
        }

        public static IDnsClient NullDnsClient { get; } = new NullDnsClient();

    }

    internal static class LookupClientExtensions
    {
        public static LookupClient WithDisabledSocketsReuse(this LookupClient obj)
        {
            obj.UseTcpOnly = true;

            var lookupClientType = typeof(LookupClient);
            var udpHandlerType = lookupClientType.Assembly.GetType("DnsClient.DnsUdpMessageHandler");

            var field = lookupClientType.GetField("_messageHandler", BindingFlags.Instance | BindingFlags.NonPublic);
            var udpHandler = field.GetValue(obj);

            field = udpHandlerType.GetField("_enableClientQueue", BindingFlags.Instance | BindingFlags.NonPublic);
            field.SetValue(udpHandler, false);

            return obj;
        }
    }
}
