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
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ProtonVPN.Core.Network
{
    public class WlanClient
    {
        public IReadOnlyCollection<WifiConnection> GetActiveWifiConnections()
        {
            var list = new List<WifiConnection>();
            var result = WlanApi.WlanOpenHandle(2, IntPtr.Zero, out _, out IntPtr clientHandle);

            if (result != WlanApi.ERROR_SUCCESS)
                return list;

            result = WlanApi.WlanEnumInterfaces(clientHandle, IntPtr.Zero, out var ptr);

            if (result != WlanApi.ERROR_SUCCESS)
                return list;

            var structure = (WlanApi.WlanInterfaceInfoListHeader)Marshal.PtrToStructure(ptr, typeof(WlanApi.WlanInterfaceInfoListHeader));
            var num = ptr.ToInt64() + Marshal.SizeOf(structure);

            for (var i = 0; i < structure.numberOfItems; i++)
            {
                var info = (WlanApi.WlanInterfaceInfo)Marshal.PtrToStructure(new IntPtr(num), typeof(WlanApi.WlanInterfaceInfo));
                num += Marshal.SizeOf(info);

                var infoResult = WlanApi.GetInterfaceInfo(clientHandle, info.interfaceGuid);
                if (!infoResult.Success)
                    continue;

                var interfaceInfo = infoResult.Value;
                var connection = new WifiConnection(
                    interfaceInfo.profileName,
                    interfaceInfo.wlanSecurityAttributes.securityEnabled);

                list.Add(connection);
            }

            WlanApi.WlanFreeMemory(ptr);
            WlanApi.WlanCloseHandle(clientHandle, IntPtr.Zero);

            return list;
        }

        public static void Prelink()
        {
            Marshal.PrelinkAll(typeof(WlanApi));
        }
    }
}
