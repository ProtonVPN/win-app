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

namespace ProtonVPN.Vpn.PortMapping.UdpClients
{
    public class UdpClientWrapper : IUdpClientWrapper
    {
        private UdpClient _udpClient;
        private IPEndPoint _endpoint;

        public void Start(IPEndPoint endpoint)
        {
            _udpClient = new();
            _endpoint = endpoint;
        }

        public void Send(byte[] data)
        {
            _udpClient.Send(data, data.Length, _endpoint);
        }

        public byte[] Receive()
        {
            IPEndPoint remoteEndpoint = _endpoint;
            return _udpClient.Receive(ref remoteEndpoint);
        }

        public void Stop()
        {
            try
            {
                _udpClient?.Close();
            }
            finally
            {
                _udpClient?.Dispose();
                _udpClient = null;
            }
        }

        public void Reset()
        {
            Stop();
            _udpClient = new();
        }

        public void Dispose()
        {
            Stop();
        }
    }
}