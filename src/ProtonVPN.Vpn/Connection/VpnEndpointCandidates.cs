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
using ProtonVPN.Common.Networking;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Vpn.Common;

namespace ProtonVPN.Vpn.Connection
{
    internal class VpnEndpointCandidates : IVpnEndpointCandidates
    {
        private readonly IDictionary<VpnProtocol, ICollection<VpnHost>> _skippedHosts =
            new Dictionary<VpnProtocol, ICollection<VpnHost>>();
        private readonly IList<string> _skippedWireGuardIps = new List<string>();
        private readonly Dictionary<VpnProtocol, ICollection<string>> _skippedOpenVpnIps = new();


        private IReadOnlyList<VpnHost> _all = new List<VpnHost>(0);

        public IVpnEndpoint Current { get; private set; }

        public VpnEndpointCandidates()
        {
            Initialize();
        }

        private void Initialize()
        {
            foreach (VpnProtocol protocol in (VpnProtocol[]) Enum.GetValues(typeof(VpnProtocol)))
            {
                _skippedHosts[protocol] = new HashSet<VpnHost>();
                _skippedOpenVpnIps[protocol] = new HashSet<string>();
            }
        }

        public void Set(IReadOnlyList<VpnHost> servers)
        {
            _all = servers;
        }

        public IVpnEndpoint NextHost(VpnConfig config)
        {
            if (config.VpnProtocol == VpnProtocol.WireGuard)
            {
                return NextWireGuardEndpoint();
            }

            if (!string.IsNullOrEmpty(Current.Server.Ip))
            {
                _skippedHosts[config.VpnProtocol].Add(Current.Server);
            }

            VpnHost server =
                _all.FirstOrDefault(h => _skippedHosts[config.VpnProtocol].All(skippedHost => h != skippedHost));
            Current = CreateOpenVpnEndpoint(server, config.VpnProtocol);

            return Current;
        }

        private IVpnEndpoint NextWireGuardEndpoint()
        {
            if (!string.IsNullOrEmpty(Current.Server.Ip))
            {
                _skippedWireGuardIps.Add(Current.Server.Ip);
            }

            VpnHost server = _all.FirstOrDefault(i => !_skippedWireGuardIps.Contains(i.Ip));
            Current = CreateWireguardEndpoint(server);

            return Current;
        }

        private IVpnEndpoint CreateWireguardEndpoint(VpnHost server)
        {
            return server.IsEmpty() ? new EmptyEndpoint() : new WireGuardEndpoint(server);
        }

        public IVpnEndpoint NextIp(VpnConfig config)
        {
            if (config.VpnProtocol == VpnProtocol.WireGuard)
            {
                return NextWireGuardEndpoint();
            }

            if (!string.IsNullOrEmpty(Current.Server.Ip))
            {
                _skippedOpenVpnIps[config.VpnProtocol].Add(Current.Server.Ip);
            }

            VpnHost server = _all.FirstOrDefault(h => _skippedOpenVpnIps[config.VpnProtocol].All(skippedIp => h.Ip != skippedIp));
            Current = CreateOpenVpnEndpoint(server, config.VpnProtocol);

            return Current;
        }

        private static IVpnEndpoint CreateOpenVpnEndpoint(VpnHost server, VpnProtocol protocol)
        {
            return server.IsEmpty() ? new EmptyEndpoint() : new OpenVpnEndpoint(server, protocol);
        }

        public void Reset()
        {
            foreach (ICollection<VpnHost> skipped in _skippedHosts.Values)
            {
                skipped.Clear();
            }

            foreach (ICollection<string> skipped in _skippedOpenVpnIps.Values)
            {
                skipped.Clear();
            }

            _skippedWireGuardIps.Clear();
            Current = new EmptyEndpoint();
        }

        public bool Contains(IVpnEndpoint endpoint)
        {
            return _all.Contains(endpoint.Server);
        }

        public int CountHosts()
        {
            return _all.Count;
        }

        public int CountIPs()
        {
            return _all.GroupBy(h => h.Ip).Count();
        }
    }
}