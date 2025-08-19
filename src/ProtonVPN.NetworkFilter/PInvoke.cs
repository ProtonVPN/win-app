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
using System.Runtime.InteropServices;
using ProtonVPN.Common.Core.Helpers;

namespace ProtonVPN.NetworkFilter;

internal class PInvoke
{
    private const string BINARY_NAME = "ProtonVPN.IpFilter.dll";

    private static string BinaryPath => PathProvider.GetResourcesPath(BINARY_NAME);

    static PInvoke()
    {
        nint handle = LoadLibrary(BinaryPath);
        if (handle == IntPtr.Zero)
        {
            throw new DllNotFoundException($"Failed to load binary: {BinaryPath}.");
        }
    }

    [DllImport("kernel32.dll")]
    private static extern IntPtr LoadLibrary(string path);

    [DllImport(
        BINARY_NAME,
        EntryPoint = "IPFilterCreateDynamicSession",
        CallingConvention = CallingConvention.Cdecl)]
    public static extern uint CreateDynamicSession(ref IntPtr handle);

    [DllImport(
        BINARY_NAME,
        EntryPoint = "IPFilterCreateSession",
        CallingConvention = CallingConvention.Cdecl)]
    public static extern uint CreateSession(ref IntPtr handle);

    [DllImport(
        BINARY_NAME,
        EntryPoint = "IPFilterDestroySession",
        CallingConvention = CallingConvention.Cdecl)]
    public static extern uint DestroySession(IntPtr handle);

    [DllImport(
        BINARY_NAME,
        EntryPoint = "IPFilterStartTransaction",
        CallingConvention = CallingConvention.Cdecl)]
    public static extern uint StartTransaction(IntPtr sessionHandle);

    [DllImport(
        BINARY_NAME,
        EntryPoint = "IPFilterAbortTransaction",
        CallingConvention = CallingConvention.Cdecl)]
    public static extern uint AbortTransaction(IntPtr sessionHandle);

    [DllImport(
        BINARY_NAME,
        EntryPoint = "IPFilterCommitTransaction",
        CallingConvention = CallingConvention.Cdecl)]
    public static extern uint CommitTransaction(IntPtr sessionHandle);

    [DllImport(
        BINARY_NAME,
        EntryPoint = "IPFilterCreateProvider",
        CallingConvention = CallingConvention.Cdecl)]
    public static extern uint CreateProvider(
        IntPtr sessionHandle,
        ref DisplayData displayData,
        uint persistent,
        [In, Out] ref Guid key);

    [DllImport(
        BINARY_NAME,
        EntryPoint = "IPFilterIsProviderRegistered",
        CallingConvention = CallingConvention.Cdecl)]
    public static extern uint IsProviderRegistered(
        IntPtr sessionHandle,
        [In] ref Guid key,
        [In, Out] ref uint result);

    [DllImport(
        BINARY_NAME,
        EntryPoint = "IPFilterDestroyProvider",
        CallingConvention = CallingConvention.Cdecl)]
    public static extern uint DestroyProvider(
        IntPtr sessionHandle,
        [In, Out] ref Guid key);

    [DllImport(
        BINARY_NAME,
        EntryPoint = "IPFilterCreateProviderContext",
        CallingConvention = CallingConvention.Cdecl)]
    public static extern uint CreateProviderContext(
        IntPtr sessionHandle,
        ref DisplayData displayData,
        [In] ref Guid providerKey,
        uint size,
        IntPtr data,
        uint persistent,
        [In, Out] ref Guid key);

    [DllImport(
        BINARY_NAME,
        EntryPoint = "IPFilterDestroyProviderContext",
        CallingConvention = CallingConvention.Cdecl)]
    public static extern uint DestroyProviderContext(
        IntPtr sessionHandle,
        [In] ref Guid key);

    [DllImport(
        BINARY_NAME,
        EntryPoint = "IPFilterCreateCallout",
        CallingConvention = CallingConvention.Cdecl)]
    public static extern uint CreateCallout(
        IntPtr sessionHandle,
        ref DisplayData displayData,
        [In] ref Guid providerKey,
        uint layer,
        uint persistent,
        [In, Out] ref Guid key);

    [DllImport(
        BINARY_NAME,
        EntryPoint = "IPFilterDestroyCallout",
        CallingConvention = CallingConvention.Cdecl)]
    public static extern uint DestroyCallout(
        IntPtr sessionHandle,
        [In] ref Guid key);

    [DllImport(
        BINARY_NAME,
        EntryPoint = "IPFilterCreateSublayer",
        CallingConvention = CallingConvention.Cdecl)]
    public static extern uint CreateSublayer(
        IntPtr sessionHandle,
        [In] ref Guid providerKey,
        ref DisplayData displayData,
        uint weight,
        uint persistent,
        [In, Out] ref Guid key);

    [DllImport(
        BINARY_NAME,
        EntryPoint = "IPFilterDestroySublayer",
        CallingConvention = CallingConvention.Cdecl)]
    public static extern uint DestroySublayer(
        IntPtr sessionHandle,
        [In] ref Guid key);

    [DllImport(
        BINARY_NAME,
        EntryPoint = "IPFilterDoesSublayerExist",
        CallingConvention = CallingConvention.Cdecl)]
    public static extern uint DoesSublayerExist(
        IntPtr sessionHandle,
        [In] ref Guid key,
        [In, Out] ref uint result);

    [DllImport(
        BINARY_NAME,
        EntryPoint = "IPFilterDoesFilterExist",
        CallingConvention = CallingConvention.Cdecl)]
    public static extern uint DoesFilterExist(
        IntPtr sessionHandle,
        [In] ref Guid key,
        [In, Out] ref uint result);

    [DllImport(
        BINARY_NAME,
        EntryPoint = "IPFilterDoesProviderContextExist",
        CallingConvention = CallingConvention.Cdecl)]
    public static extern uint DoesProviderContextExist(
        IntPtr sessionHandle,
        [In] ref Guid key,
        [In, Out] ref uint result);

    [DllImport(
        BINARY_NAME,
        EntryPoint = "IPFilterDoesCalloutExist",
        CallingConvention = CallingConvention.Cdecl)]
    public static extern uint DoesCalloutExist(
        IntPtr sessionHandle,
        [In] ref Guid key,
        [In, Out] ref uint result);

    [DllImport(
        BINARY_NAME,
        EntryPoint = "IPFilterDestroySublayerFilters",
        CallingConvention = CallingConvention.Cdecl)]
    public static extern uint DestroySublayerFilters(
        IntPtr sessionHandle,
        [In] ref Guid providerId,
        [In] ref Guid sublayerId);

    [DllImport(
        BINARY_NAME,
        EntryPoint = "IPFilterDestroySublayerFiltersByName",
        CallingConvention = CallingConvention.Cdecl)]
    public static extern uint DestroySublayerFiltersByName(
        IntPtr sessionHandle,
        [In] ref Guid providerId,
        [In] ref Guid sublayerId,
        [MarshalAs(UnmanagedType.LPWStr)]  string name);

    [DllImport(
        BINARY_NAME,
        EntryPoint = "IPFilterGetSublayerFilterCount",
        CallingConvention = CallingConvention.Cdecl)]
    public static extern uint GetSublayerFilterCount(
        IntPtr sessionHandle,
        [In] ref Guid providerId,
        [In] ref Guid sublayerId,
        [In, Out] ref uint result);

    [DllImport(
        BINARY_NAME,
        EntryPoint = "IPFilterDestroyCallouts",
        CallingConvention = CallingConvention.Cdecl)]
    public static extern uint DestroyCallouts(IntPtr sessionHandle, [In] ref Guid providerId);

    [DllImport(
        BINARY_NAME,
        EntryPoint = "IPFilterDestroyFilter",
        CallingConvention = CallingConvention.Cdecl)]
    public static extern uint DestroyFilter(
        IntPtr sessionHandle,
        [In] ref Guid key);

    [DllImport(
        BINARY_NAME,
        EntryPoint = "IPFilterCreateLayerFilter",
        CallingConvention = CallingConvention.Cdecl)]
    public static extern uint CreateLayerFilter(
        IntPtr sessionHandle,
        [In] ref Guid providerKey,
        [In] ref Guid sublayerKey,
        ref DisplayData displayData,
        uint layer,
        uint action,
        uint weight,
        [In] ref Guid calloutKey,
        [In] ref Guid providerContextKey,
        uint persistent,
        [In, Out] ref Guid filterKey);

    [DllImport(
        BINARY_NAME,
        EntryPoint = "IPFilterCreateRemoteIPv4Filter",
        CallingConvention = CallingConvention.Cdecl)]
    public static extern uint CreateRemoteIPv4Filter(
        IntPtr sessionHandle,
        [In] ref Guid providerKey,
        [In] ref Guid sublayerKey,
        ref DisplayData displayData,
        uint layer,
        uint action,
        uint weight,
        [In] ref Guid calloutKey,
        [In] ref Guid providerContextKey,
        [MarshalAs(UnmanagedType.LPStr)] string addr,
        uint persistent,
        [In, Out] ref Guid filterKey);

    [DllImport(
        BINARY_NAME,
        EntryPoint = "IPFilterCreateAppFilter",
        CallingConvention = CallingConvention.Cdecl)]
    public static extern uint CreateAppFilter(
        IntPtr sessionHandle,
        [In] ref Guid providerKey,
        [In] ref Guid sublayerKey,
        ref DisplayData displayData,
        uint layer,
        uint action,
        uint weight,
        [In] ref Guid calloutKey,
        [In] ref Guid providerContextKey,
        [MarshalAs(UnmanagedType.LPWStr)] string appPath,
        uint persistent,
        [In, Out] ref Guid filterKey);

    [DllImport(
        BINARY_NAME,
        EntryPoint = "IPFilterCreateRemoteTCPPortFilter",
        CallingConvention = CallingConvention.Cdecl)]
    public static extern uint CreateRemoteTCPPortFilter(
        IntPtr sessionHandle,
        [In] ref Guid providerKey,
        [In] ref Guid sublayerKey,
        ref DisplayData displayData,
        uint layer,
        uint action,
        uint weight,
        uint port,
        uint persistent,
        [In, Out] ref Guid filterKey);

    [DllImport(
        BINARY_NAME,
        EntryPoint = "IPFilterCreateRemoteUDPPortFilter",
        CallingConvention = CallingConvention.Cdecl)]
    public static extern uint CreateRemoteUDPPortFilter(
        IntPtr sessionHandle,
        [In] ref Guid providerKey,
        [In] ref Guid sublayerKey,
        ref DisplayData displayData,
        uint layer,
        uint action,
        uint weight,
        uint port,
        uint persistent,
        [In, Out] ref Guid filterKey);

    [DllImport(
        BINARY_NAME,
        EntryPoint = "IPFilterCreateRemoteNetworkIPFilter",
        CallingConvention = CallingConvention.Cdecl)]
    public static extern uint CreateRemoteNetworkIPFilter(
        IntPtr sessionHandle,
        [In] ref Guid providerKey,
        [In] ref Guid sublayerKey,
        ref DisplayData displayData,
        uint layer,
        uint action,
        uint weight,
        [In] ref Guid calloutKey,
        [In] ref Guid providerContextKey,
        ref NetworkAddress addr,
        uint persistent,
        [In, Out] ref Guid filterKey);

    [DllImport(
        BINARY_NAME,
        EntryPoint = "IPFilterCreateNetInterfaceFilter",
        CallingConvention = CallingConvention.Cdecl)]
    public static extern uint CreateNetInterfaceFilter(
        IntPtr sessionHandle,
        [In] ref Guid providerKey,
        [In] ref Guid sublayerKey,
        ref DisplayData displayData,
        uint layer,
        uint action,
        uint weight,
        uint index,
        uint persistent,
        [In, Out] ref Guid filterKey);

    [DllImport(
        BINARY_NAME,
        EntryPoint = "IPFilterCreateLoopbackFilter",
        CallingConvention = CallingConvention.Cdecl)]
    public static extern uint CreateLoopbackFilter(
        IntPtr sessionHandle,
        [In] ref Guid providerKey,
        [In] ref Guid sublayerKey,
        ref DisplayData displayData,
        uint layer,
        uint action,
        uint weight,
        uint persistent,
        [In, Out] ref Guid filterKey);

    [DllImport(
        BINARY_NAME,
        EntryPoint = "BlockOutsideDns",
        CallingConvention = CallingConvention.Cdecl)]
    public static extern uint BlockOutsideDns(
        IntPtr sessionHandle,
        [In] ref Guid providerKey,
        [In] ref Guid sublayerKey,
        ref DisplayData displayData,
        uint layer,
        uint action,
        uint weight,
        [In] ref Guid calloutKey,
        uint index,
        uint persistent,
        [In, Out] ref Guid filterKey);

    [DllImport(
        BINARY_NAME,
        EntryPoint = "BlockOutsideOpenVpn",
        CallingConvention = CallingConvention.Cdecl)]
    public static extern uint BlockOutsideOpenVpn(
        IntPtr sessionHandle,
        [In] ref Guid providerKey,
        [In] ref Guid sublayerKey,
        ref DisplayData displayData,
        uint layer,
        uint weight,
        [MarshalAs(UnmanagedType.LPWStr)] string appPath,
        string serverIpAddress,
        uint persistent,
        [In, Out] ref Guid filterKey);

    [DllImport(
        BINARY_NAME,
        EntryPoint = "PermitRouterSolicitationMessage",
        CallingConvention = CallingConvention.Cdecl)]
    public static extern uint PermitRouterSolicitationMessage(
        IntPtr sessionHandle,
        [In] ref Guid providerKey,
        [In] ref Guid sublayerKey,
        ref DisplayData displayData,
        uint layer,
        uint action,
        uint weight,
        [In] ref Guid calloutKey,
        [In] ref Guid providerContextKey,
        uint persistent,
        [In, Out] ref Guid filterKey);

    [DllImport(
        BINARY_NAME,
        EntryPoint = "PermitRouterAdvertisementMessage",
        CallingConvention = CallingConvention.Cdecl)]
    public static extern uint PermitRouterAdvertisementMessage(
        IntPtr sessionHandle,
        [In] ref Guid providerKey,
        [In] ref Guid sublayerKey,
        ref DisplayData displayData,
        uint layer,
        uint action,
        uint weight,
        [In] ref Guid calloutKey,
        [In] ref Guid providerContextKey,
        uint persistent,
        [In, Out] ref Guid filterKey);

    [DllImport(
        BINARY_NAME,
        EntryPoint = "PermitNeighborSolicitationMessage",
        CallingConvention = CallingConvention.Cdecl)]
    public static extern uint PermitNeighborSolicitationMessage(
        IntPtr sessionHandle,
        [In] ref Guid providerKey,
        [In] ref Guid sublayerKey,
        ref DisplayData displayData,
        uint layer,
        uint action,
        uint weight,
        [In] ref Guid calloutKey,
        [In] ref Guid providerContextKey,
        uint persistent,
        [In, Out] ref Guid filterKey);

    [DllImport(
        BINARY_NAME,
        EntryPoint = "PermitNeighborAdvertisementMessage",
        CallingConvention = CallingConvention.Cdecl)]
    public static extern uint PermitNeighborAdvertisementMessage(
        IntPtr sessionHandle,
        [In] ref Guid providerKey,
        [In] ref Guid sublayerKey,
        ref DisplayData displayData,
        uint layer,
        uint action,
        uint weight,
        [In] ref Guid calloutKey,
        [In] ref Guid providerContextKey,
        uint persistent,
        [In, Out] ref Guid filterKey);

    [DllImport(
        BINARY_NAME,
        EntryPoint = "PermitIcmpRedirectMessage",
        CallingConvention = CallingConvention.Cdecl)]
    public static extern uint PermitIcmpRedirectMessage(
        IntPtr sessionHandle,
        [In] ref Guid providerKey,
        [In] ref Guid sublayerKey,
        ref DisplayData displayData,
        uint layer,
        uint action,
        uint weight,
        [In] ref Guid calloutKey,
        [In] ref Guid providerContextKey,
        uint persistent,
        [In, Out] ref Guid filterKey);

    [DllImport(
        BINARY_NAME,
        EntryPoint = "PermitOutboundIpv6Dhcp",
        CallingConvention = CallingConvention.Cdecl)]
    public static extern uint PermitOutboundIpv6Dhcp(
        IntPtr sessionHandle,
        [In] ref Guid providerKey,
        [In] ref Guid sublayerKey,
        ref DisplayData displayData,
        uint layer,
        uint action,
        uint weight,
        [In] ref Guid calloutKey,
        [In] ref Guid providerContextKey,
        uint persistent,
        [In, Out] ref Guid filterKey);

    [DllImport(
        BINARY_NAME,
        EntryPoint = "PermitInboundIpv6Dhcp",
        CallingConvention = CallingConvention.Cdecl)]
    public static extern uint PermitInboundIpv6Dhcp(
        IntPtr sessionHandle,
        [In] ref Guid providerKey,
        [In] ref Guid sublayerKey,
        ref DisplayData displayData,
        uint layer,
        uint action,
        uint weight,
        [In] ref Guid calloutKey,
        [In] ref Guid providerContextKey,
        uint persistent,
        [In, Out] ref Guid filterKey);
}