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

using ProtonVPN.Common.OS.Net.NetworkInterface;
using ProtonVPN.Common.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ProtonVPN.Core.OS.Net.Dns
{
    /// <summary>
    /// A volatile DNS client.
    /// Refreshes DNS servers on network address change.
    /// </summary>
    public class DnsClient : IDnsClient
    {
        private readonly IDnsClients _dnsClients;
        private readonly SingleAction _refreshDnsClient;

        private IDnsClient _dnsClient = DnsClients.NullDnsClient;
        private volatile bool _addressChanged = true;

        public DnsClient(IDnsClients dnsClients, INetworkInterfaces networkInterfaces)
        {
            _dnsClients = dnsClients;

            _refreshDnsClient = new SingleAction(RefreshDnsClient);
            networkInterfaces.NetworkAddressChanged += NetworkAddressChanged;
        }

        public async Task<string> Resolve(string host, CancellationToken token)
        {
            var dnsClient = await GetDnsClient();
            return await dnsClient.Resolve(host, token);
        }

        public IReadOnlyCollection<IPEndPoint> NameServers => _dnsClient.NameServers;

        private void NetworkAddressChanged(object sender, EventArgs e)
        {
            _addressChanged = true;
        }

        private async Task<IDnsClient> GetDnsClient()
        {
            if (_addressChanged)
            {
                await _refreshDnsClient.Run();
            }

            return _dnsClient;
        }

        private void RefreshDnsClient()
        {
            if (!_addressChanged)
            {
                return;
            }

            _addressChanged = false;

            var dnsServers = _dnsClients.NameServers();
            if (_dnsClient.NameServers.ToHashSet().SetEquals(dnsServers))
            {
                return;
            }

            _dnsClient = _dnsClients.DnsClient(dnsServers);
        }
    }
}
