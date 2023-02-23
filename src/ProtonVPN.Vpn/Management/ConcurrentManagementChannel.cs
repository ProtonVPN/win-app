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

using System.Threading.Tasks;
using ProtonVPN.Common.Threading;

namespace ProtonVPN.Vpn.Management
{
    /// <summary>
    /// Adds multi-threading support to <see cref="IManagementChannel.WriteLine"/> method.
    /// </summary>
    internal class ConcurrentManagementChannel : IManagementChannel
    {
        private readonly IManagementChannel _managementChannel;
        private readonly SerialTaskQueue _writeQueue;

        public ConcurrentManagementChannel(IManagementChannel managementChannel)
        {
            _managementChannel = managementChannel;

            _writeQueue = new SerialTaskQueue();
        }

        public Task Connect(int port)
        {
            return _managementChannel.Connect(port);
        }

        public void Disconnect()
        {
            _managementChannel.Disconnect();
        }

        public Task<string> ReadLine()
        {
            return _managementChannel.ReadLine();
        }

        /// <summary>
        /// Writes message to OpenVPN management interface. Safe to call from multiple threads simultaneously.
        /// </summary>
        /// <param name="message">Message to write.</param>
        /// <exception cref="System.IO.IOException">Thrown if failed to write to OpenVPN management socket.</exception>
        public Task WriteLine(string message)
        {
            return _writeQueue.Enqueue(() => _managementChannel.WriteLine(message));
        }
    }
}
