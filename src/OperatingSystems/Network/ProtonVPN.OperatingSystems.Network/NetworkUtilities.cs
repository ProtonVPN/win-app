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
using System.Runtime.InteropServices;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.NetworkLogs;
using ProtonVPN.OperatingSystems.Network.Contracts;
using Vanara.PInvoke;
using static Vanara.PInvoke.IpHlpApi;
using static Vanara.PInvoke.Ws2_32;

namespace ProtonVPN.OperatingSystems.Network;

public class NetworkUtilities : INetworkUtilities
{
    public const uint ERROR_SUCCESS = 0;

    private readonly ILogger _logger;

    public NetworkUtilities(
        ILogger logger)
    {
        _logger = logger;
    }

    public void EnableIPv6OnAllAdapters(string appName, string excludeId)
    {
        AssertSuccess(() => PInvoke.EnableIPv6OnAllAdapters(appName, excludeId));
    }

    public void DisableIPv6OnAllAdapters(string appName, string excludeId)
    {
        AssertSuccess(() => PInvoke.DisableIPv6OnAllAdapters(appName, excludeId));
    }

    public void EnableIPv6(string appName, string interfaceId)
    {
        AssertSuccess(() => PInvoke.EnableIPv6(appName, interfaceId));
    }

    public IPAddress GetBestInterfaceIPv4Address(string excludedIfaceHwid)
    {
        byte[] bytes = new byte[4];
        GCHandle pinnedBytes = GCHandle.Alloc(bytes, GCHandleType.Pinned);

        AssertSuccess(() => PInvoke.GetBestInterfaceIp(pinnedBytes.AddrOfPinnedObject(), excludedIfaceHwid));

        pinnedBytes.Free();

        return new IPAddress(bytes);
    }

    public void SetLowestTapMetric(uint index)
    {
        AssertSuccess(() => PInvoke.SetLowestTapMetric(index));
    }

    public void RestoreDefaultTapMetric(uint index)
    {
        AssertSuccess(() => PInvoke.RestoreDefaultTapMetric(index));
    }

    private void AssertSuccess(Func<uint> function)
    {
        uint status;
        try
        {
            status = function();
        }
        catch (SEHException ex)
        {
            throw new NetworkUtilException(ex.ErrorCode, ex);
        }

        switch (status)
        {
            case ERROR_SUCCESS:
                return;
            default:
                throw new NetworkUtilException(status);
        }
    }

    public NetworkAddress? GetDefaultIpv6Gateway(INetworkInterface tunnelInterface, INetworkInterface[] networkInterfaces)
    {
        List<uint> interfacesWithGlobalUnicastAddresses = GetInterfaceIndexesWithGlobalUnicastAddress(tunnelInterface, networkInterfaces);
        if (interfacesWithGlobalUnicastAddresses.Count == 0)
        {
            _logger.Warn<NetworkLog>("No interface found with global unicast address.");
            return null;
        }

        List<MIB_IPFORWARD_ROW2> ipForwardRows = GetIpv6DefaultRoutes(interfacesWithGlobalUnicastAddresses);
        if (ipForwardRows.Count == 0)
        {
            _logger.Error<NetworkLog>("No IPv6 route found.");
            return null;
        }

        Dictionary<uint, uint> interfaceMetrics = GetIpv6InterfaceMetrics();
        byte[]? nextHop = GetNextHopWithBestEffectiveMetric(ipForwardRows, interfaceMetrics);

        return nextHop is not null && NetworkAddress.TryParse(new IPAddress(nextHop).ToString(), out NetworkAddress ipv6DefaultRoute)
            ? ipv6DefaultRoute
            : null;
    }

    private static byte[]? GetNextHopWithBestEffectiveMetric(List<MIB_IPFORWARD_ROW2> ipForwardRows, Dictionary<uint, uint> interfaceMetrics)
    {
        byte[]? nextHop = null;
        uint bestEffectiveMetric = uint.MaxValue;

        foreach (MIB_IPFORWARD_ROW2 row in ipForwardRows)
        {
            if (!interfaceMetrics.TryGetValue(row.InterfaceIndex, out uint interfaceMetric))
            {
                continue;
            }

            uint effectiveMetric = row.Metric + interfaceMetric;
            if (effectiveMetric < bestEffectiveMetric)
            {
                bestEffectiveMetric = effectiveMetric;
                nextHop = row.NextHop.Ipv6.sin6_addr.bytes;
            }
        }

        return nextHop;
    }

    private List<uint> GetInterfaceIndexesWithGlobalUnicastAddress(INetworkInterface tunnelInterface, INetworkInterface[] networkInterfaces)
    {
        return networkInterfaces
            .Where(i => !i.Equals(tunnelInterface))
            .Where(i => i.GetUnicastAddresses().Any(a => a.IsGlobalUnicastAddress()))
            .Where(i => i.Index != 0)
            .Select(i => i.Index)
            .ToList();
    }

    private List<MIB_IPFORWARD_ROW2> GetIpv6DefaultRoutes(List<uint> interfacesWithGlobalUnicastAddresses)
    {
        Win32Error result = GetIpForwardTable2(ADDRESS_FAMILY.AF_INET6, out MIB_IPFORWARD_TABLE2 interfaces);
        if (result.Failed)
        {
            _logger.Error<NetworkLog>("Failed to retrieve IP forward table.", result.GetException());
            return [];
        }

        return interfaces?.Table?.Where(row => IsDefaultIpv6Route(row, interfacesWithGlobalUnicastAddresses)).ToList() ?? [];
    }

    private static bool IsDefaultIpv6Route(MIB_IPFORWARD_ROW2 row, List<uint> interfacesWithGlobalUnicastAddresses)
    {
        return row.DestinationPrefix.PrefixLength == 0 &&
            new IPAddress(row.DestinationPrefix.Prefix.Ipv6.sin6_addr.bytes).Equals(IPAddress.IPv6None) &&
            interfacesWithGlobalUnicastAddresses.Contains(row.InterfaceIndex);
    }

    private Dictionary<uint, uint> GetIpv6InterfaceMetrics()
    {
        Win32Error result = GetIpInterfaceTable(ADDRESS_FAMILY.AF_INET6, out MIB_IPINTERFACE_TABLE interfaces);
        if (result.Failed || interfaces?.Table is null)
        {
            _logger.Error<NetworkLog>("Failed to retrieve IP interface table.", result.GetException());
            return [];
        }

        Dictionary<uint, uint> metrics = [];

        foreach (MIB_IPINTERFACE_ROW interfaceRow in interfaces.Table)
        {
            metrics.Add(interfaceRow.InterfaceIndex, interfaceRow.Metric);
        }

        return metrics;
    }
}