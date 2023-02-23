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

using System.Collections.Generic;
using System.Management;

namespace ProtonVPN.Vpn.Networks.Adapters
{
    public class NetworkAdaptersLoader : INetworkAdaptersLoader
    {
        public IList<INetworkAdapter> GetAll()
        {
            SelectQuery query = new("Win32_NetworkAdapter");
            ManagementObjectSearcher search = new(query);
            IList<INetworkAdapter> networkAdapters = new List<INetworkAdapter>();
            foreach (ManagementObject result in search.Get())
            {
                networkAdapters.Add(new NetworkAdapter(result));
            }

            return networkAdapters;
        }
    }
}