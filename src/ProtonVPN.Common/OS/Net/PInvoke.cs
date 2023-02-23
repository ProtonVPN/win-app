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

namespace ProtonVPN.Common.Os.Net
{
    internal class PInvoke
    {
        private const string BinaryName = "ProtonVPN.NetworkUtil.dll";

        private static string BinaryPath => $"{(Environment.Is64BitOperatingSystem ? "x64" : "x86")}\\{BinaryName}";

        static PInvoke()
        {
            var handle = LoadLibrary(BinaryPath);
            if (handle == IntPtr.Zero)
            {
                throw new DllNotFoundException($"Failed to load binary: {BinaryPath}.");
            }
        }

        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibrary(string path);

        [DllImport(
            BinaryName,
            EntryPoint = "NetworkUtilEnableIPv6OnAllAdapters",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern uint EnableIPv6OnAllAdapters(
            [MarshalAs(UnmanagedType.LPWStr)] string appName,
            [MarshalAs(UnmanagedType.LPWStr)] string excludeId);

        [DllImport(
            BinaryName,
            EntryPoint = "NetworkUtilDisableIPv6OnAllAdapters",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern uint DisableIPv6OnAllAdapters(
            [MarshalAs(UnmanagedType.LPWStr)] string appName,
            [MarshalAs(UnmanagedType.LPWStr)] string excludeId);

        [DllImport(
            BinaryName,
            EntryPoint = "NetworkUtilEnableIPv6",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern uint EnableIPv6(
            [MarshalAs(UnmanagedType.LPWStr)] string appName,
            [MarshalAs(UnmanagedType.LPWStr)] string interfaceId);

        [DllImport(
            BinaryName,
            EntryPoint = "GetBestInterfaceIp",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern uint GetBestInterfaceIp(
            IntPtr address,
            [MarshalAs(UnmanagedType.LPWStr)] string excludedIfaceHwid);

        [DllImport(
            BinaryName,
            EntryPoint = "SetLowestTapMetric",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern uint SetLowestTapMetric(uint index);

        [DllImport(
            BinaryName,
            EntryPoint = "RestoreDefaultTapMetric",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern uint RestoreDefaultTapMetric(uint index);
    }
}
