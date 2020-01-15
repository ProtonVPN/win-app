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
using System.Collections;
using System.Collections.Generic;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Vpn.Common;

namespace ProtonVPN.Vpn.OpenVpn.Arguments
{
    internal class EndpointArguments : IEnumerable<string>
    {
        private readonly VpnEndpoint _endpoint;

        public EndpointArguments(VpnEndpoint endpoint)
        {
            _endpoint = endpoint;
        }

        public IEnumerator<string> GetEnumerator()
        {
            yield return $"--remote {_endpoint.Server.Ip} {_endpoint.Port} {OpenVpnProtocol(_endpoint.Protocol)}";
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private string OpenVpnProtocol(VpnProtocol protocol)
        {
            switch (protocol)
            {
                case VpnProtocol.OpenVpnUdp:
                    return "udp";
                case VpnProtocol.OpenVpnTcp:
                    return "tcp";
            }

            throw new ArgumentException(nameof(protocol));
        }
    }
}
