/*
 * Copyright (c) 2020 Proton Technologies AG
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

using ProtonVPN.NetworkFilter;

namespace ProtonVPN.Service.Firewall
{
    public class FirewallParams
    {
        public FirewallParams(string serverIp, bool dnsLeakOnly, uint interfaceIndex, bool persistent)
        {
            ServerIp = serverIp;
            DnsLeakOnly = dnsLeakOnly;
            InterfaceIndex = interfaceIndex;
            Persistent = persistent;
        }

        public static FirewallParams Null => new(string.Empty, false, 0, false);

        public string ServerIp { get; }

        public bool DnsLeakOnly { get; }

        public uint InterfaceIndex { get; }

        public bool Persistent { get; }

        public SessionType SessionType => Persistent ? SessionType.Permanent : SessionType.Dynamic;
    }
}