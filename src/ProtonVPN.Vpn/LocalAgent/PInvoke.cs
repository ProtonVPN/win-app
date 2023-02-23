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
using ProtonVPN.Common.Go;

namespace ProtonVPN.Vpn.LocalAgent
{
    internal class PInvoke
    {
        private static int Bits => Environment.Is64BitOperatingSystem ? 64 : 32;

        private const string DllName = "LocalAgent";

        private static string BinaryPath => $"Resources\\{Bits}-bit\\{DllName}.dll";

        static PInvoke()
        {
            Environment.SetEnvironmentVariable("GODEBUG", "cgocheck=0");

            IntPtr handle = LoadLibrary(BinaryPath);
            if (handle == IntPtr.Zero)
            {
                throw new DllNotFoundException($"Failed to load binary: {BinaryPath}.");
            }
        }

        [DllImport("Kernel32.dll")]
        private static extern IntPtr LoadLibrary(string path);

        [DllImport(DllName, EntryPoint = "Connect", CallingConvention = CallingConvention.Cdecl)]
        public static extern GoBytes Connect(GoString clientCertPem, GoString clientKeyPem, GoString serverCaPem,
            GoString host, GoString certServerName, GoString featuresJson, bool connectivity);

        [DllImport(DllName, EntryPoint = "Ping", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool Ping(GoString ip, int port, GoString serverKeyBase64, int timeoutInSeconds);

        [DllImport(DllName, EntryPoint = "Close", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Close();

        [DllImport(DllName, EntryPoint = "GetEvent", CallingConvention = CallingConvention.Cdecl)]
        public static extern GoBytes GetEvent();

        [DllImport(DllName, EntryPoint = "GetStatus", CallingConvention = CallingConvention.Cdecl)]
        public static extern GoBytes GetStatus();

        [DllImport(DllName, EntryPoint = "SetFeatures", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetFeatures(GoString featuresJson);
    }
}