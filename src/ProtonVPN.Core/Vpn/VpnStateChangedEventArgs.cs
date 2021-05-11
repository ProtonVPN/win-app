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

using ProtonVPN.Common.Helpers;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Servers.Models;

namespace ProtonVPN.Core.Vpn
{
    public class VpnStateChangedEventArgs
    {
        public VpnState State { get; }
        public VpnError Error { get; }
        public bool NetworkBlocked { get; }
        public VpnProtocol Protocol { get; }

        public VpnStateChangedEventArgs(VpnStatus status, VpnError error, string endpointIp, bool networkBlocked, VpnProtocol protocol, string label = "")
            : this(new VpnState(status, endpointIp, protocol, label), error, networkBlocked, protocol)
        {
        }

        public VpnStateChangedEventArgs(VpnStatus status, VpnError error, Server server, bool networkBlocked, VpnProtocol protocol)
            : this(new VpnState(status, server), error, networkBlocked, protocol)
        {
        }

        public VpnStateChangedEventArgs(VpnState state, VpnError error, bool networkBlocked, VpnProtocol protocol = VpnProtocol.Auto)
        {
            Ensure.NotNull(state, nameof(state));

            State = state;
            Error = error;
            NetworkBlocked = networkBlocked;
            Protocol = protocol;
        }

        public bool UnexpectedDisconnect => (State.Status == VpnStatus.Disconnected || State.Status == VpnStatus.Disconnecting) &&
                                            Error != VpnError.None;
    }
}
