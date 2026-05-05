/*
 * Copyright (c) 2026 Proton AG
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

using ProtonVPN.ProTun.Contracts.Adapters;
using ProtonVPN.ProTun.Generated;

namespace ProtonVPN.ProTun.Adapters;

public static class ProTunAdapterDetailsExtensions
{
    public static AdapterDetails Map(this ProTunAdapterDetails arg)
    {
        return new()
        {
            InterfaceIndex = arg.interfaceIndex,
            ClientIpv4Addr = arg.clientIpv4Addr,
            ServerIpv4Addr = arg.serverIpv4Addr,
            ClientIpv6Addr = arg.clientIpv6Addr,
            ServerIpv6Addr = arg.serverIpv6Addr,
        };
    }
}