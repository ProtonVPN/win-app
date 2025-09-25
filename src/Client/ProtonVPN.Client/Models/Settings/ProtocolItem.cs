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

using ProtonVPN.Client.Core.Bases.Models;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Common.Core.Networking;

namespace ProtonVPN.Client.Models.Settings;

public class ProtocolItem : ModelBase
{
    public VpnProtocol Protocol { get; }

    public string Header => Localizer.GetVpnProtocol(Protocol);

    public bool IsSmartProtocol => Protocol == VpnProtocol.Smart;

    public bool IsWireGuardTlsProtocol => Protocol == VpnProtocol.WireGuardTls;

    public bool IsWireGuardUdpProtocol => Protocol == VpnProtocol.WireGuardUdp;

    public bool IsWireGuardTcpProtocol => Protocol == VpnProtocol.WireGuardTcp;

    public bool IsOpenVpnUdpProtocol => Protocol == VpnProtocol.OpenVpnUdp;

    public bool IsOpenVpnTcpProtocol => Protocol == VpnProtocol.OpenVpnTcp;

    public ProtocolItem(
        ILocalizationProvider localizer,
        VpnProtocol vpnProtocol)
        : base(localizer)
    {
        Protocol = vpnProtocol;
    }
}
