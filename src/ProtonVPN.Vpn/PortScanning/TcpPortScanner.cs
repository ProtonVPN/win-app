/*
 * Copyright (c) 2025 Proton AG
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
using ProtonVPN.Common.Core.Extensions;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Vpn.OpenVpn;

namespace ProtonVPN.Vpn.PortScanning;

public class TcpPortScanner : ITcpPortScanner
{
    private readonly byte[] _staticKey;

    public TcpPortScanner(IStaticConfiguration config)
    {
        _staticKey = config.OpenVpn.StaticKey;
    }

    public async Task<bool> IsAliveAsync(string ip, int port, Task timeoutTask)
    {
        OpenVpnHandshake packet = new(_staticKey);
        IPEndPoint endpoint = new(IPAddress.Parse(ip), port);
        using Socket socket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        try
        {
            await SafeSocketActionAsync(socket.ConnectAsync(endpoint)).WithTimeout(timeoutTask);

            byte[] bytes = packet.Bytes(true);
            await SafeSocketActionAsync(socket.SendAsync(new ArraySegment<byte>(bytes), SocketFlags.None)).WithTimeout(timeoutTask);

            byte[] answer = new byte[1024];
            int received = await SafeSocketFuncAsync(socket.ReceiveAsync(new ArraySegment<byte>(answer), SocketFlags.None)).WithTimeout(timeoutTask);

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

    private static async Task SafeSocketActionAsync(Task task)
    {
        try
        {
            await task.ConfigureAwait(false);
        }
        catch (SocketException) { }           // swallowed
        catch (ObjectDisposedException) { }   // swallowed
        // Any other exception propagates naturally to the caller
    }

    private static async Task<TResult> SafeSocketFuncAsync<TResult>(Task<TResult> task)
    {
        try
        {
            return await task.ConfigureAwait(false);
        }
        catch (SocketException) { }           // swallowed
        catch (ObjectDisposedException) { }   // swallowed
        // Any other exception propagates naturally to the caller

        return default!;
    }
}