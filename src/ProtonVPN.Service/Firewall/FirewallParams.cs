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

using ProtonVPN.Common.Core.Dns;
using ProtonVPN.NetworkFilter;

namespace ProtonVPN.Service.Firewall;

public class FirewallParams
{
    public static FirewallParams Empty => new()
    {
        ServerIp = string.Empty,
    };

    public string ServerIp { get; set; }

    public bool DnsLeakOnly { get; set; }

    public uint InterfaceIndex { get; set; }

    public bool Persistent { get; set; }

    public bool AddInterfaceFilters { get; set; }

    public bool PermanentStateAfterReboot { get; set; }

    public bool IsLocalAreaNetworkAccessEnabled { get; set; }

    public DnsBlockMode DnsBlockMode { get; set; }

    public SessionType SessionType => Persistent ? SessionType.Permanent : SessionType.Dynamic;
}