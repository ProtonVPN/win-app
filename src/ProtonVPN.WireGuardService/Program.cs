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
using System.Text;
using ProtonVPN.Configurations.Defaults;
using ProtonVPN.Logging.Events;

namespace ProtonVPN.WireGuardService;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length != 2)
        {
            LogError(args);
            return;
        }

        string configFilePath = args[0];
        string protocol = args[1];

        switch (protocol)
        {
            case "udp":
                RunWireGuardNT(configFilePath);
                break;
            case "tcp" or "tls":
                RunWireGuardWintun(configFilePath, protocol);
                break;
        }
    }

    private static void LogError(string[] args)
    {
        string message = GetErrorMessage(args);
        EventLogger.LogError(DefaultConfiguration.WireGuard.ServiceName, message);
    }

    private static string GetErrorMessage(string[] args)
    {
        StringBuilder message = new();
        message.AppendLine($"Failed to start {DefaultConfiguration.WireGuard.ServiceName} Service due to incorrect number of arguments.");
        message.AppendLine($"Expected to receive 2 arguments, got {args.Length}.");

        if (args.Length > 0)
        {
            message.AppendLine("The arguments provided were:");
            message.AppendLine();
            message.AppendJoin("\n", args);
        }

        return message.ToString();
    }

    [DllImport("tunnel.dll", EntryPoint = "WireGuardTunnelService", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool RunWireGuardNT([MarshalAs(UnmanagedType.LPWStr)] string configFile);

    [DllImport("wireguard-tunnel-tcp.dll", EntryPoint = "WireGuardTunnelService", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool RunWireGuardWintun(
        [MarshalAs(UnmanagedType.LPWStr)] string configFile,
        [MarshalAs(UnmanagedType.LPWStr)] string protocol);
}