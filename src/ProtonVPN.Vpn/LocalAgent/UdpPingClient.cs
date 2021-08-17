/*
 * Copyright (c) 2021 Proton Technologies AG
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

using System.Threading;
using System.Threading.Tasks;
using ProtonVPN.Common.Go;

namespace ProtonVPN.Vpn.LocalAgent
{
    public class UdpPingClient
    {
        private static readonly int TimeoutInMilliseconds = 3000;

        public async Task<bool> Ping(string ip, int port, string serverKeyBase64, CancellationToken cancellationToken)
        {
            try
            {
                return await Task.Run(() =>
                {
                    using GoString ipGoString = ip.ToGoString();
                    using GoString serverKeyBase64GoString = serverKeyBase64.ToGoString();
                    return PInvoke.Ping(ipGoString, port, serverKeyBase64GoString, TimeoutInMilliseconds);
                }, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                return false;
            }
        }
    }
}