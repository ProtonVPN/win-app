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

using System.Collections.Concurrent;
using ProtonVPN.Client.Common.Enums;
using ProtonVPN.Dns.Contracts;

namespace ProtonVPN.Client.Settings.Contracts;

public interface IGlobalSettings
{
    string Language { get; set; }
    string? Username { get; set; }
    int? WindowWidth { get; set; }
    int? WindowHeight { get; set; }
    int? WindowXPosition { get; set; }
    int? WindowYPosition { get; set; }
    bool IsWindowMaximized { get; set; }
    bool IsAutoLaunchEnabled { get; set; }
    AutoLaunchMode AutoLaunchMode { get; set; }
    int[] OpenVpnTcpPorts { get; set; }
    int[] OpenVpnUdpPorts { get; set; }
    ConcurrentDictionary<string, DnsResponse>? DnsCache { get; set; }
    bool DoHEnabled { get; set; }
}