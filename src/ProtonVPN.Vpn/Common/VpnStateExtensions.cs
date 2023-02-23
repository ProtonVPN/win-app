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

using ProtonVPN.Common.Vpn;

namespace ProtonVPN.Vpn.Common
{
    internal static class VpnStateExtensions
    {
        public static VpnState WithRemoteIp(this VpnState state, string remoteIp, string label)
        {
            return new(
                state.Status,
                state.Error,
                state.LocalIp,
                remoteIp,
                state.VpnProtocol,
                state.PortForwarding,
                state.OpenVpnAdapter,
                label);
        }

        public static VpnState WithError(this VpnState state, VpnError error)
        {
            return new(
                state.Status,
                error,
                state.LocalIp,
                state.RemoteIp,
                state.VpnProtocol,
                state.PortForwarding,
                state.OpenVpnAdapter,
                state.Label);
        }
    }
}