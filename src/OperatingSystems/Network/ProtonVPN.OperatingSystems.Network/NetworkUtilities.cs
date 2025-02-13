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

using System.Net;
using System.Runtime.InteropServices;
using ProtonVPN.OperatingSystems.Network.Contracts;

namespace ProtonVPN.OperatingSystems.Network;

public class NetworkUtilities : INetworkUtilities
{
    public const uint ERROR_SUCCESS = 0;

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

    public IPAddress GetBestInterfaceIp(string excludedIfaceHwid)
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
}