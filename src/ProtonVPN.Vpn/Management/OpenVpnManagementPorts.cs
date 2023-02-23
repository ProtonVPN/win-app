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

using System.Net;
using System.Net.Sockets;

namespace ProtonVPN.Vpn.Management
{
    /// <summary>
    /// Gets available TCP port from network stack for OpenVPN management interface.
    /// </summary>
    public class OpenVpnManagementPorts
    {
        private readonly IPEndPoint _loopbackEndpoint = new(IPAddress.Loopback, port: 0);

        public int Port()
        {
            using Socket socket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(_loopbackEndpoint);
            return ((IPEndPoint)socket.LocalEndPoint).Port;
        }
    }
}