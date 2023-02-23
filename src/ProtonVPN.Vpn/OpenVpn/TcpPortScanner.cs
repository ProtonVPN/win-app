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
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Vpn.Common;

namespace ProtonVPN.Vpn.OpenVpn
{
    public class TcpPortScanner
    {
        private byte[] _staticKey;

        public void Config(byte[] staticKey)
        {
            _staticKey = staticKey;
        }

        public async Task<bool> Alive(VpnEndpoint vpnEndpoint, Task timeoutTask)
        {
            OpenVpnHandshake packet = new(_staticKey);
            IPEndPoint endpoint = new(IPAddress.Parse(vpnEndpoint.Server.Ip), vpnEndpoint.Port);
            using Socket socket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                await SafeSocketAction(socket.ConnectAsync(endpoint)).WithTimeout(timeoutTask);

                byte[] bytes = packet.Bytes(true);
                await SafeSocketAction(socket.SendAsync(new ArraySegment<byte>(bytes), SocketFlags.None)).WithTimeout(timeoutTask);

                byte[] answer = new byte[1024];
                int received = await SafeSocketFunc(socket.ReceiveAsync(new ArraySegment<byte>(answer), SocketFlags.None)).WithTimeout(timeoutTask);

                return received > 0;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                socket.Close();
            }
        }

        private static Task SafeSocketAction(Task task)
        {
            task.ContinueWith(t =>
                {
                    switch (t.Exception?.InnerException)
                    {
                        case null:
                        case SocketException _:
                        case ObjectDisposedException _:
                            return;
                        default:
                            throw t.Exception;
                    }
                },
                TaskContinuationOptions.OnlyOnFaulted);

            return task;
        }

        private static Task<TResult> SafeSocketFunc<TResult>(Task<TResult> task)
        {
            task.ContinueWith(t =>
                {
                    switch (t.Exception?.InnerException)
                    {
                        case null:
                        case SocketException _:
                        case ObjectDisposedException _:
                            return;
                        default:
                            throw t.Exception;
                    }
                },
                TaskContinuationOptions.OnlyOnFaulted);

            return task;
        }
    }
}