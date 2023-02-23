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
using System.Linq;
using System.Net;
using System.Reflection;
using DnsClient;
using ProtonVPN.Common.Extensions;

namespace ProtonVPN.Core.OS.Net.Dns
{
    public class DnsClients : IDnsClients
    {
        public IDnsClient DnsClient(IReadOnlyCollection<IPEndPoint> nameServers)
        {
            return nameServers.Any() 
                ? new FixedDnsClient(new LookupClient(nameServers.ToArray()).WithDisabledSocketsReuse()) 
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

    internal static class LookupClientExtensions
    {
        public static LookupClient WithDisabledSocketsReuse(this LookupClient obj)
        {
            obj.UseTcpOnly = true;

            Type lookupClientType = typeof(LookupClient);
            Type udpHandlerType = lookupClientType.Assembly.GetType("DnsClient.DnsUdpMessageHandler");

            FieldInfo field = lookupClientType.GetField("_messageHandler", BindingFlags.Instance | BindingFlags.NonPublic);
            object udpHandler = field.GetValue(obj);

            field = udpHandlerType.GetField("_enableClientQueue", BindingFlags.Instance | BindingFlags.NonPublic);
            field.SetValue(udpHandler, false);

            return obj;
        }
    }
}