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
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.DnsLogs;
using AddressFamily = System.Net.Sockets.AddressFamily;
using SystemDns = System.Net.Dns;

namespace ProtonVPN.Dns.Resolvers.System
{
    public class SystemDnsResolver : ISystemDnsResolver
    {
        private readonly ILogger _logger;

        public SystemDnsResolver(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<IList<IPAddress>> ResolveWithSystemAsync(string host, CancellationToken cancellationToken)
        {
            try
            {
                _logger.Info<DnsResolveLog>($"Attempting to resolve host '{host}' through system.");
                return await SystemDns.GetHostAddressesAsync(host, AddressFamily.InterNetwork, cancellationToken);
            }
            catch (Exception e)
            {
                _logger.Error<DnsErrorLog>($"Failed to resolve host '{host}' through system.", e);
                return new List<IPAddress>();
            }
        }
    }
}