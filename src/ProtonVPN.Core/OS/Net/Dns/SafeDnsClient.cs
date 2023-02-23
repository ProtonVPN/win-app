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
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using DnsClient;
using ProtonVPN.Common.Helpers;

namespace ProtonVPN.Core.OS.Net.Dns
{
    /// <summary>
    /// Suppresses DNS client exceptions.
    /// </summary>
    public class SafeDnsClient : IDnsClient
    {
        private readonly IDnsClient _origin;

        public SafeDnsClient(IDnsClient origin)
        {
            Ensure.NotNull(origin, nameof(origin));

            _origin = origin;
        }

        public async Task<string> Resolve(string host, CancellationToken token)
        {
            try
            {
                return await _origin.Resolve(host, token);
            }
            catch (Exception e) when (e is SocketException || e is DnsResponseException || e is OperationCanceledException)
            {
                return null;
            }
        }

        public IReadOnlyCollection<IPEndPoint> NameServers => _origin.NameServers;
    }
}
