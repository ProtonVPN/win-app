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

using System.Collections.Generic;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Vpn.Common;

namespace ProtonVPN.Vpn.Connection
{
    public interface IVpnEndpointCandidates
    {
        VpnEndpoint Current { get; }

        /// <summary>Excludes the last returned host [IP+Label] (if not the first call) and returns the next endpoint</summary>
        VpnEndpoint NextHost(VpnConfig config);
        /// <summary>Excludes the last returned IP (if not the first call) and returns the next endpoint</summary>
        VpnEndpoint NextIp(VpnConfig config);
        void Set(IReadOnlyList<VpnHost> servers);
        void Reset();
        bool Contains(VpnEndpoint endpoint);
        int CountHosts();
        int CountIPs();
    }
}