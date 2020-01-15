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

using System;
using System.Collections.Generic;
using System.Linq;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Vpn.Common;

namespace ProtonVPN.Vpn.Connection
{
    internal class VpnEndpointCandidates : IVpnEndpointCandidates
    {
        private readonly Dictionary<VpnProtocol, ICollection<string>> _skippedIps =
            new Dictionary<VpnProtocol, ICollection<string>>();

        private IReadOnlyList<VpnHost> _all = new List<VpnHost>(0);

        public VpnEndpointCandidates()
        {
            Initialize();
        }

        public VpnEndpoint Current { get; private set; }

        public void Set(IReadOnlyList<VpnHost> servers)
        {
            _all = servers;
        }

        public VpnEndpoint Next(VpnProtocol protocol)
        {
            if (!string.IsNullOrEmpty(Current.Server.Ip))
                _skippedIps[protocol].Add(Current.Server.Ip);

            var server = _all.FirstOrDefault(i => !_skippedIps[protocol].Contains(i.Ip));

            if (server.IsEmpty())
            {
                _skippedIps[protocol].Clear();
                server = _all.FirstOrDefault();
            }

            Current = Endpoint(server, protocol);

            return Current;
        }

        public void Reset()
        {
            foreach (var skipped in _skippedIps.Values)
            {
                skipped.Clear();
            }

            Current = VpnEndpoint.EmptyEndpoint;
        }

        public bool Contains(VpnEndpoint endpoint)
        {
            return _all.Contains(endpoint.Server);
        }

        private void Initialize()
        {
            foreach (var protocol in (VpnProtocol[]) Enum.GetValues(typeof(VpnProtocol)))
            {
                _skippedIps[protocol] = new HashSet<string>();
            }
        }

        private static VpnEndpoint Endpoint(VpnHost server, VpnProtocol protocol)
        {
            return server.IsEmpty()
                ? VpnEndpoint.EmptyEndpoint
                : new VpnEndpoint(server, protocol);
        }
    }
}
