/*
 * Copyright (c) 2024 Proton AG
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

using System.Runtime.InteropServices;
using ProtonVPN.Client.UnsecureWifiDetection.WlanInterop;
using ProtonVPN.Common.Legacy.Abstract;

namespace ProtonVPN.Client.UnsecureWifiDetection;

public class WlanClient
{
    private const int ERROR_SUCCESS = 0;
    private const string WLAN_API = "wlanapi.dll";

    public static IReadOnlyCollection<WifiConnection> GetActiveWifiConnections()
    {
        try
        {
            List<WifiConnection> list = [];
            int result = WlanOpenHandle(2, nint.Zero, out _, out nint clientHandle);

            if (result != ERROR_SUCCESS)
            {
                return list;
            }

            result = WlanEnumInterfaces(clientHandle, nint.Zero, out nint ptr);

            if (result != ERROR_SUCCESS)
            {
                return list;
            }

            object? headerObject = Marshal.PtrToStructure(ptr, typeof(WlanInterfaceInfoListHeader));
            if (headerObject is null)
            {
                return list;
            }

            WlanInterfaceInfoListHeader structure = (WlanInterfaceInfoListHeader)headerObject;
            long num = ptr.ToInt64() + Marshal.SizeOf(structure);

            for (int i = 0; i < structure.numberOfItems; i++)
            {
                object? interfaceInfoObject = Marshal.PtrToStructure(new nint(num), typeof(WlanInterfaceInfo));
                if (interfaceInfoObject is null)
                {
                    continue;
                }

                WlanInterfaceInfo info = (WlanInterfaceInfo)interfaceInfoObject;
                num += Marshal.SizeOf(info);

                Result<WlanConnectionAttributes> infoResult = GetInterfaceInfo(clientHandle, info.interfaceGuid);
                if (!infoResult.Success)
                {
                    continue;
                }

                WlanConnectionAttributes interfaceInfo = infoResult.Value;
                WifiConnection connection = new(
                    interfaceInfo.profileName,
                    interfaceInfo.wlanSecurityAttributes.securityEnabled);

                list.Add(connection);
            }

            WlanFreeMemory(ptr);
            WlanCloseHandle(clientHandle, nint.Zero);

            return list;
        }
        catch (DllNotFoundException)
        {
            return [];
        }
    }

    public static Result<WlanConnectionAttributes> GetInterfaceInfo(nint client, Guid guid)
    {
        try
        {
            int code = WlanQueryInterface(client, guid, WlanIntfOpcode.CurrentConnection, nint.Zero, out _, out nint ptr, out _);
            if (code == ERROR_SUCCESS)
            {
                try
                {
                    object? result = Marshal.PtrToStructure(ptr, typeof(WlanConnectionAttributes));

                    return result is not null
                        ? Result.Ok((WlanConnectionAttributes)result)
                        : Result.Fail<WlanConnectionAttributes>();
                }
                finally
                {
                    WlanFreeMemory(ptr);
                }
            }

            return Result.Fail<WlanConnectionAttributes>();
        }
        catch (DllNotFoundException)
        {
            return Result.Fail<WlanConnectionAttributes>();
        }
    }

    [DllImport(WLAN_API)]
    private static extern int WlanEnumInterfaces(
        [In] nint clientHandle,
        [In, Out] nint pReserved,
        out nint ppInterfaceList);

    [DllImport(WLAN_API)]
    private static extern void WlanFreeMemory(nint pMemory);

    [DllImport(WLAN_API)]
    private static extern int WlanOpenHandle(
        [In] uint clientVersion,
        [In, Out] nint pReserved,
        out uint negotiatedVersion,
        out nint clientHandle);

    [DllImport(WLAN_API)]
    private static extern int WlanCloseHandle(
        [In] nint clientHandle,
        [In, Out] nint pReserved);

    [DllImport(WLAN_API)]
    private static extern int WlanQueryInterface(
        [In] nint clientHandle,
        [In, MarshalAs(UnmanagedType.LPStruct)] Guid interfaceGuid,
        [In] WlanIntfOpcode opCode,
        [In, Out] nint pReserved,
        out int dataSize,
        out nint ppData,
        out WlanOpcodeValueType wlanOpcodeValueType);

    public static void Prelink()
    {
        Marshal.PrelinkAll(typeof(WlanClient));
    }
}