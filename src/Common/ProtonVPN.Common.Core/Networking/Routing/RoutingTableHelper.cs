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

using System.Net;
using ProtonVPN.Common.Core.Networking.Extensions;
using Vanara.PInvoke;
using static Vanara.PInvoke.IpHlpApi;
using static Vanara.PInvoke.Ws2_32;

namespace ProtonVPN.Common.Core.Networking.Routing;

public class RoutingTableHelper
{
    public static void CreateRoute(RouteConfiguration route)
    {
        InitializeIpForwardEntry(out MIB_IPFORWARD_ROW2 row);

        row.DestinationPrefix = GetDestinationPrefix(route);
        row.NextHop = GetNextHop(route);
        row.Metric = route.Metric;
        row.InterfaceIndex = route.Gateway is null ? 1 : route.InterfaceIndex;
        row.ValidLifetime = uint.MaxValue;
        row.PreferredLifetime = uint.MaxValue;
        row.Loopback = route.Gateway is null;

        CreateIpForwardEntry2(ref row);
    }

    public static uint? GetLoopbackInterfaceIndex()
    {
        Win32Error result = GetIfTable2(out MIB_IF_TABLE2 table);
        if (result.Succeeded)
        {
            MIB_IF_ROW2? interfaceRow = table.Table?.FirstOrDefault(row => row.Type == IFTYPE.IF_TYPE_SOFTWARE_LOOPBACK);
            return interfaceRow?.InterfaceIndex;
        }

        return null;
    }

    private static IP_ADDRESS_PREFIX GetDestinationPrefix(RouteConfiguration route)
    {
        return new()
        {
            Prefix = CreateSockAddrInet(route.Destination),
            PrefixLength = GetDefaultPrefixLength(route.Destination),
        };
    }

    private static SOCKADDR_INET GetNextHop(RouteConfiguration route)
    {
        return CreateSockAddrInet(route.Gateway ?? new NetworkAddress(route.IsIpv6
                ? IPAddress.IPv6None
                : IPAddress.None));
    }

    private static SOCKADDR_INET CreateSockAddrInet(NetworkAddress address)
    {
        SOCKADDR_INET sockAddr = new()
        {
            si_family = address.GetFamily(),
        };

        if (address.IsIpV6)
        {
            sockAddr.Ipv6 = new SOCKADDR_IN6
            {
                sin6_family = ADDRESS_FAMILY.AF_INET6,
                sin6_addr = new IN6_ADDR(address.Ip.GetAddressBytes()),
            };
        }
        else
        {
            sockAddr.Ipv4 = new SOCKADDR_IN
            {
                sin_family = ADDRESS_FAMILY.AF_INET,
                sin_addr = new IN_ADDR(address.Ip.GetAddressBytes()),
            };
        }

        return sockAddr;
    }

    private static byte GetDefaultPrefixLength(NetworkAddress address)
    {
        if (address.Subnet.HasValue)
        {
            return (byte)address.Subnet.Value;
        }

        if (address.IsIpV6)
        {
            return 128;
        }

        return address.Ip.Equals(IPAddress.Any) ? (byte)0 : (byte)32;
    }

    public static void DeleteRoute(RouteConfiguration route)
    {
        MIB_IPFORWARD_ROW2 routeToDelete = new()
        {
            DestinationPrefix = GetDestinationPrefix(route),
            NextHop = GetNextHop(route),
            InterfaceIndex = route.InterfaceIndex,
        };

        DeleteIpForwardEntry2(ref routeToDelete);
    }

    public static void DeleteRoute(string destinationIpAddress, bool isIpv6)
    {
        IPAddress ipAddress = IPAddress.Parse(destinationIpAddress);
        ADDRESS_FAMILY family = isIpv6
            ? ADDRESS_FAMILY.AF_INET6
            : ADDRESS_FAMILY.AF_INET;

        Win32Error result = GetIpForwardTable2(family, out MIB_IPFORWARD_TABLE2 table);
        if (result.Failed)
        {
            return;
        }

        for (int i = 0; i < table.Table?.Length; i++)
        {
            if (isIpv6 && table.Table[i].DestinationPrefix.Prefix.Ipv6.sin6_addr.Equals(new IN6_ADDR(ipAddress.GetAddressBytes())) ||
                !isIpv6 && table.Table[i].DestinationPrefix.Prefix.Ipv4.sin_addr.Equals(new IN_ADDR(ipAddress.GetAddressBytes())))
            {
                DeleteIpForwardEntry2(ref table.Table[i]);
            }
        }
    }

    public static uint? GetInterfaceMetric(uint interfaceIndex, bool isIpv6)
    {
        MIB_IPINTERFACE_ROW row = new()
        {
            Family = isIpv6 ? ADDRESS_FAMILY.AF_INET6 : ADDRESS_FAMILY.AF_INET,
            InterfaceIndex = interfaceIndex,
        };

        Win32Error result = GetIpInterfaceEntry(ref row);

        return result.Succeeded
            ? row.Metric
            : null;
    }
}