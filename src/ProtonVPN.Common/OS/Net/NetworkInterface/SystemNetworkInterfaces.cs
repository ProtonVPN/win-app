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
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using ProtonVPN.Common.Os.Net;

namespace ProtonVPN.Common.OS.Net.NetworkInterface
{
    /// <summary>
    /// Provides access to network interfaces on the system.
    /// </summary>
    public class SystemNetworkInterfaces : INetworkInterfaces
    {
        public SystemNetworkInterfaces()
        {
            NetworkChange.NetworkAddressChanged += (s, e) => NetworkAddressChanged?.Invoke(s, e);
        }

        public event EventHandler NetworkAddressChanged;

        public INetworkInterface[] GetInterfaces()
        {
            System.Net.NetworkInformation.NetworkInterface[] interfaces =
                System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();

            return interfaces
                .Select(i => new SystemNetworkInterface(i))
                .Cast<INetworkInterface>()
                .ToArray();
        }

        public INetworkInterface GetByDescription(string description)
        {
            return GetInterfaces().FirstOrDefault(i => i.Description.Contains(description));
        }

        public INetworkInterface GetByLocalAddress(IPAddress localAddress)
        {
            System.Net.NetworkInformation.NetworkInterface[] interfaces =
                System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();

            foreach (System.Net.NetworkInformation.NetworkInterface i in interfaces)
            {
                if (i.GetIPProperties().UnicastAddresses.FirstOrDefault(a => a.Address.Equals(localAddress)) != null)
                {
                    return new SystemNetworkInterface(i);
                }
            }

            return new NullNetworkInterface();
        }

        public INetworkInterface GetBestInterface(string hardwareIdToExclude)
        {
            return GetByLocalAddress(NetworkUtil.GetBestInterfaceIp(hardwareIdToExclude));
        }

        public INetworkInterface GetByName(string name)
        {
            return GetInterfaces().FirstOrDefault(i => i.Name == name);
        }

        public INetworkInterface GetById(Guid id)
        {
            return GetInterfaces().FirstOrDefault(i => AreIdsEqual(i.Id, id));
        }

        private bool AreIdsEqual(string stringId, Guid id)
        {
            return Guid.TryParse(stringId, out Guid result) && result == id;
        }
    }
}