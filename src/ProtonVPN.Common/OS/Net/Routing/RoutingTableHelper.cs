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
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using ProtonVPN.Common.Extensions;

namespace ProtonVPN.Common.OS.Net.Routing
{
    public class RoutingTableHelper
    {
        // A local route where the next hop is the final destination (a local interface).
        private const int MibIPRouteTypeDirect = 3;
        private const int MibIPProtoNetmgmt = 3;

        public static void CreateRoute(string destination, string mask, string gateway, uint interfaceIndex, int metric)
        {
            MibIPForwardRow route = new MibIPForwardRow
            {
                DwForwardDest = BitConverter.ToUInt32(IPAddress.Parse(destination).GetAddressBytes(), 0),
                DwForwardMask = BitConverter.ToUInt32(IPAddress.Parse(mask).GetAddressBytes(), 0),
                DwForwardNextHop = BitConverter.ToUInt32(IPAddress.Parse(gateway).GetAddressBytes(), 0),
                DwForwardMetric1 = Convert.ToUInt32(metric),
                DwForwardType = MibIPRouteTypeDirect,
                DwForwardProto = MibIPProtoNetmgmt,
                DwForwardAge = 0,
                DwForwardPolicy = 0,
                DwForwardNextHopAS = 0,
                DwForwardIfIndex = Convert.ToUInt32(interfaceIndex)
            };

            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(MibIPForwardRow)));
            try
            {
                Marshal.StructureToPtr(route, ptr, false);
                PInvoke.CreateIpForwardEntry(ptr);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }

        public static void DeleteRoute(string destination, string mask, string gateway)
        {
            uint uDestination = destination.ToIPAddressBytes();
            uint uMask = mask.ToIPAddressBytes();
            uint uGateway = gateway.ToIPAddressBytes();

            DeleteRouteInternal((row, _) => row.DwForwardDest.Equals(uDestination) &&
                                            row.DwForwardMask.Equals(uMask) &&
                                            row.DwForwardNextHop.Equals(uGateway));
        }

        public static void DeleteRoute(string destination)
        {
            uint uDestination = destination.ToIPAddressBytes();
            DeleteRouteInternal((row, _) => row.DwForwardDest.Equals(uDestination));
        }

        private static void DeleteRouteInternal(Func<MibIPForwardRow, int, bool> func)
        {
            IPForwardTable routingTable = GetRoutingTable();
            List<MibIPForwardRow> filtered = routingTable.Table.Where(func).ToList();
            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(MibIPForwardRow)));

            try
            {
                foreach (MibIPForwardRow routeEntry in filtered)
                {
                    Marshal.StructureToPtr(routeEntry, ptr, false);
                    PInvoke.DeleteIpForwardEntry(ptr);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }

        public static int GetIpInterfaceEntry(uint interfaceIndex, out MibIPInterfaceRow row)
        {
            row = new MibIPInterfaceRow {
                Family = 2,
                InterfaceIndex = interfaceIndex,
            };
            return PInvoke.GetIpInterfaceEntry(ref row);
        }

        public static IPForwardTable GetRoutingTable()
        {
            IntPtr fwdTable = IntPtr.Zero;
            int size = 0;
            PInvoke.GetIpForwardTable(fwdTable, ref size, true);
            fwdTable = Marshal.AllocHGlobal(size);
            PInvoke.GetIpForwardTable(fwdTable, ref size, true);
            IPForwardTable forwardTable = PInvoke.ReadIPForwardTable(fwdTable);
            Marshal.FreeHGlobal(fwdTable);

            return forwardTable;
        }
    }
}