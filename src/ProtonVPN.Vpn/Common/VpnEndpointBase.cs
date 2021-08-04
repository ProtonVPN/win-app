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

using ProtonVPN.Common.Vpn;

namespace ProtonVPN.Vpn.Common
{
    public abstract class VpnEndpointBase : IVpnEndpoint
    {
        public VpnHost Server { get; }
        public int Port { get; }
        public bool IsEmpty { get; }

        protected VpnEndpointBase(VpnHost server) 
            : this(server, 0)
        {
        }

        protected VpnEndpointBase(VpnHost server, int port, bool isEmpty = false)
        {
            Server = server;
            Port = port;
            IsEmpty = isEmpty;
        }
    }
}