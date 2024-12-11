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
using ProtonVPN.Common.Legacy.Vpn;

namespace ProtonVPN.Core.Vpn;

public class VpnState
{
    public VpnStatus Status { get; }
    public string EntryIp { get; }
    public OpenVpnAdapter? NetworkAdapterType { get; }
    public VpnProtocol VpnProtocol { get; }
    public string Label { get; }

    public VpnState(VpnStatus status, string entryIp, VpnProtocol vpnProtocol, OpenVpnAdapter? networkAdapterType = null, string label = "")
    {
        Status = status;
        EntryIp = entryIp;
        VpnProtocol = vpnProtocol;
        NetworkAdapterType = networkAdapterType;
        Label = label;
    }

    public VpnState(VpnStatus status, VpnProtocol vpnProtocol = VpnProtocol.Smart, OpenVpnAdapter? networkAdapterType = null)
    {
        Status = status;
        VpnProtocol = vpnProtocol;
        NetworkAdapterType = networkAdapterType;
    }
}