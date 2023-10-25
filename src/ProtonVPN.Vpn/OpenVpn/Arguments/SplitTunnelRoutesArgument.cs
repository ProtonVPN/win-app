﻿/*
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

using System.Collections;
using System.Collections.Generic;
using ProtonVPN.Common.Legacy;
using ProtonVPN.Common.Legacy.OS.Net;

namespace ProtonVPN.Vpn.OpenVpn.Arguments
{
    internal class SplitTunnelRoutesArgument : IEnumerable<string>
    {
        private readonly IReadOnlyCollection<string> _ips;
        private readonly SplitTunnelMode _splitTunnelMode;

        public SplitTunnelRoutesArgument(IReadOnlyCollection<string> ips, SplitTunnelMode splitTunnelMode)
        {
            _ips = ips;
            _splitTunnelMode = splitTunnelMode;
        }

        public IEnumerator<string> GetEnumerator()
        {
            if (_ips == null || _ips.Count == 0)
            {
                yield break;
            }

            foreach (string ip in _ips)
            {
                var address = new NetworkAddress(ip);
                switch (_splitTunnelMode)
                {
                    case SplitTunnelMode.Permit:
                        yield return $"--route {address.Ip} {address.Mask} vpn_gateway 32000";
                        break;
                    case SplitTunnelMode.Block:
                        yield return $"--route {address.Ip} {address.Mask} net_gateway metric";
                        break;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}