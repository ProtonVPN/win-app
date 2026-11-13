/*
 * Copyright (c) 2026 Proton AG
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

using System.Net.NetworkInformation;
using static Vanara.PInvoke.IpHlpApi;

namespace ProtonVPN.OperatingSystems.Network.NetworkInterfaces;

public static class VisibleNetworkInterfaces
{
    private const GetAdaptersAddressesFlags ADAPTER_IDS_ONLY_FLAGS =
        GetAdaptersAddressesFlags.GAA_FLAG_SKIP_UNICAST |
        GetAdaptersAddressesFlags.GAA_FLAG_SKIP_ANYCAST |
        GetAdaptersAddressesFlags.GAA_FLAG_SKIP_MULTICAST |
        GetAdaptersAddressesFlags.GAA_FLAG_SKIP_DNS_SERVER |
        GetAdaptersAddressesFlags.GAA_FLAG_SKIP_FRIENDLY_NAME;

    // Since .NET 10, NetworkInterface.GetAllNetworkInterfaces() also returns hidden interfaces,
    // such as the "-WFP Native MAC Layer LightWeight Filter-0000" filters of every adapter.
    // This keeps only the interfaces that Windows lists by default, as .NET 8 did.
    public static NetworkInterface[] Get()
    {
        NetworkInterface[] allInterfaces = NetworkInterface.GetAllNetworkInterfaces();

        HashSet<string>? visibleIds = GetVisibleAdapterIds();
        if (visibleIds is null)
        {
            return allInterfaces;
        }

        return allInterfaces.Where(i => visibleIds.Contains(i.Id)).ToArray();
    }

    private static HashSet<string>? GetVisibleAdapterIds()
    {
        try
        {
            using IP_ADAPTER_ADDRESSES_RESULT adapters = GetAdaptersAddresses(ADAPTER_IDS_ONLY_FLAGS);
            return adapters
                .Select(a => a.AdapterName)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            return null;
        }
    }
}