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
using System.Net.Sockets;
using DnsClient;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.DnsLogs;
using ProtonVPN.Dns.Contracts.NameServers;

namespace ProtonVPN.Dns.NameServers
{
    public class NameServersLoader : INameServersLoader
    {
        private readonly INameServersResolver _nameServersResolver;
        private readonly ILogger _logger;

        public NameServersLoader(INameServersResolver nameServersResolver, ILogger logger)
        {
            _nameServersResolver = nameServersResolver;
            _logger = logger;
        }

        public IList<IPEndPoint> Get()
        {
            _logger.Info<DnsNameServerLog>("Getting name servers from available network interfaces.");
            IList<IPEndPoint> nameServers;
            try
            {
                nameServers = _nameServersResolver.Resolve()
                    .Select(NameServerToIPEndPoint)
                    .Where(e => e is not null &&
                                e.AddressFamily == AddressFamily.InterNetwork &&
                                !Equals(e.Address, IPAddress.Loopback) &&
                                !Equals(e.Address, IPAddress.Any) &&
                                !Equals(e.Address, IPAddress.Broadcast) &&
                                !Equals(e.Address, IPAddress.None))
                    .ToList();

                if (nameServers.Any())
                {
                    _logger.Info<DnsNameServerLog>($"Found {nameServers.Count} name servers.");
                }
                else
                {
                    _logger.Error<DnsNameServerLog>("Failed to find any name servers.");
                }
            }
            catch (Exception e)
            {
                nameServers = new List<IPEndPoint>();
                _logger.Error<DnsNameServerLog>("An error occurred when attempting to find name servers.", e);
            }

            return nameServers;
        }

        private IPEndPoint NameServerToIPEndPoint(NameServer nameServer)
        {
            if (nameServer?.Address == null)
            {
                return null;
            }

            IPAddress ipAddress = null;
            if (IPAddress.TryParse(nameServer.Address, out IPAddress parsedAddress))
            {
                ipAddress = parsedAddress;
                _logger.Debug<DnsNameServerLog>($"Found name server {ipAddress}:{nameServer.Port}.");
            }

            return ipAddress == null ? null : new IPEndPoint(ipAddress, nameServer.Port);
        }
    }
}