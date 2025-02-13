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

using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Common.Legacy.Helpers;
using ProtonVPN.Vpn.Common;

namespace ProtonVPN.Common.Legacy.Vpn;

public class VpnStateChangedEventArgs
{
    public VpnState State { get; }
    public VpnError Error { get; }
    public bool NetworkBlocked { get; }

    public VpnStateChangedEventArgs(VpnStatus status, VpnError error, string endpointIp, int endpointPort,
        bool networkBlocked, VpnProtocol vpnProtocol, OpenVpnAdapter? networkAdapterType = null,
        string label = "")
        : this(new VpnState(status, endpointIp, endpointPort, vpnProtocol, networkAdapterType, label), error, networkBlocked)
    {
    }

    public VpnStateChangedEventArgs(VpnState state, VpnError error, bool networkBlocked)
    {
        Ensure.NotNull(state, nameof(state));

        State = state;
        Error = error;
        NetworkBlocked = networkBlocked;
    }
}