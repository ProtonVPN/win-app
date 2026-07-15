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
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Common.Legacy.Vpn;
using ProtonVPN.Vpn.Common;

namespace ProtonVPN.Vpn.Connection;

public class VpnEndpointCandidates : IVpnEndpointCandidates
{
    private readonly Dictionary<VpnProtocol, ICollection<string>> _skippedIps = [];

    private IReadOnlyList<VpnHost> _all = [];

    public VpnEndpoint? Current { get; private set; }

    public VpnEndpointCandidates()
    {
        Initialize();
    }

    private void Initialize()
    {
        foreach (VpnProtocol protocol in (VpnProtocol[])Enum.GetValues(typeof(VpnProtocol)))
        {
            _skippedIps[protocol] = new HashSet<string>();
        }
    }

    public void Set(IReadOnlyList<VpnHost> servers)
    {
        _all = servers;
    }

    private VpnEndpoint NextEndpoint(VpnConfig config)
    {
        VpnHost server = _all.FirstOrDefault(h => HasUntriedIp(h, config));

        Current = CreateVpnEndpoint(server, config.VpnProtocol);

        return Current;
    }

    private bool HasUntriedIp(VpnHost host, VpnConfig config)
    {
        foreach (string ip in GetCandidateIps(host, config))
        {
            if (!_skippedIps[config.VpnProtocol].Contains(ip))
            {
                return true;
            }
        }

        return false;
    }

    private static IEnumerable<string> GetCandidateIps(VpnHost host, VpnConfig config)
    {
        foreach (VpnProtocol preferredProtocol in config.PreferredProtocols)
        {
            string ip = host.GetIp(preferredProtocol);
            if (!string.IsNullOrEmpty(ip))
            {
                yield return ip;
            }
        }
    }

    public VpnEndpoint NextIp(VpnConfig config)
    {
        if (Current is not null && !Current.IsEmpty)
        {
            foreach (string ip in GetCandidateIps(Current.Server, config))
            {
                _skippedIps[config.VpnProtocol].Add(ip);
            }
        }

        return NextEndpoint(config);
    }

    private static VpnEndpoint CreateVpnEndpoint(VpnHost server, VpnProtocol protocol)
    {
        return server.IsEmpty() ? new() : new VpnEndpoint(server, protocol);
    }

    public void Reset()
    {
        foreach (ICollection<string> skipped in _skippedIps.Values)
        {
            skipped.Clear();
        }

        Current = new();
    }

    public bool Contains(VpnEndpoint endpoint)
    {
        return _all.Any(s => s == endpoint.Server);
    }

    public int CountIPs()
    {
        return _all.GroupBy(h => h.Ip).Count();
    }
}