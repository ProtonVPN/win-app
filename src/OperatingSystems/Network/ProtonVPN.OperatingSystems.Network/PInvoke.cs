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

using System.Runtime.InteropServices;
using ProtonVPN.Common.Core.Helpers;

namespace ProtonVPN.OperatingSystems.Network;

public class PInvoke
{
    private const string BINARY_NAME = "ProtonVPN.NetworkUtil.dll";

    private static string BinaryPath => PathProvider.GetResourcesPath(BINARY_NAME);

    static PInvoke()
    {
        nint handle = LoadLibrary(BinaryPath);
        if (handle == nint.Zero)
        {
            throw new DllNotFoundException($"Failed to load binary: {BinaryPath}.");
        }
    }

    [DllImport("kernel32.dll")]
    private static extern nint LoadLibrary(string path);

    [DllImport(
        BINARY_NAME,
        EntryPoint = "NetworkUtilEnableIPv6OnAllAdapters",
        CallingConvention = CallingConvention.Cdecl)]
    public static extern uint EnableIPv6OnAllAdapters(
        [MarshalAs(UnmanagedType.LPWStr)] string appName,
        [MarshalAs(UnmanagedType.LPWStr)] string excludeId);

    [DllImport(
        BINARY_NAME,
        EntryPoint = "NetworkUtilDisableIPv6OnAllAdapters",
        CallingConvention = CallingConvention.Cdecl)]
    public static extern uint DisableIPv6OnAllAdapters(
        [MarshalAs(UnmanagedType.LPWStr)] string appName,
        [MarshalAs(UnmanagedType.LPWStr)] string excludeId);

    [DllImport(
        BINARY_NAME,
        EntryPoint = "NetworkUtilEnableIPv6",
        CallingConvention = CallingConvention.Cdecl)]
    public static extern uint EnableIPv6(
        [MarshalAs(UnmanagedType.LPWStr)] string appName,
        [MarshalAs(UnmanagedType.LPWStr)] string interfaceId);

    [DllImport(
        BINARY_NAME,
        EntryPoint = "GetBestInterfaceIp",
        CallingConvention = CallingConvention.Cdecl)]
    public static extern uint GetBestInterfaceIp(
        nint address,
        [MarshalAs(UnmanagedType.LPWStr)] string excludedIfaceHwid);

    [DllImport(
        BINARY_NAME,
        EntryPoint = "SetLowestTapMetric",
        CallingConvention = CallingConvention.Cdecl)]
    public static extern uint SetLowestTapMetric(uint index);

    [DllImport(
        BINARY_NAME,
        EntryPoint = "RestoreDefaultTapMetric",
        CallingConvention = CallingConvention.Cdecl)]
    public static extern uint RestoreDefaultTapMetric(uint index);
}