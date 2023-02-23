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

using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ProtonVPN.Vpn.Management
{
    /// <summary>
    /// Interface to OpenVPN management interface.
    /// </summary>
    public interface IManagementChannel
    {
        /// <summary>
        /// Connects to OpenVPN management interface using specified TCP port.
        /// </summary>
        /// <param name="port">TCP port OpenVPN process is listening on.</param>
        /// <exception cref="SocketException">Thrown if failed to connect to OpenVPN management socket.</exception>
        Task Connect(int port);

        /// <summary>
        /// Writes message to OpenVPN management interface.
        /// </summary>
        /// <param name="message">Message to write.</param>
        /// <exception cref="IOException">Thrown if failed to write to OpenVPN management socket.</exception>
        Task WriteLine(string message);

        /// <summary>
        /// Reads current message from the OpenVPN management interface. 
        /// Returns NULL on the end of stream.
        /// </summary>
        /// <returns>Current message. NULL if end of stream reached.</returns>
        /// <exception cref="IOException">Thrown if failed to read from OpenVPN management socket.</exception>
        Task<string> ReadLine();

        /// <summary>
        /// Disconnect from OpenVPN management interface.
        /// </summary>
        void Disconnect();
    }
}
