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

using System.Net.NetworkInformation;
using ProtonVPN.Common.Core.Extensions;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.OperatingSystems.Network.Contracts.NetworkInterfaces;

namespace ProtonVPN.OperatingSystems.Network.NetworkInterfaces;

public class ConflictingNetworkInterfacesProvider : IConflictingNetworkInterfacesProvider
{
    // https://learn.microsoft.com/en-us/windows-hardware/drivers/network/ndis-interface-types
    private const NetworkInterfaceType PROPRIETARY_VIRTUAL_INTERFACE_TYPE = (NetworkInterfaceType)53;

    private readonly static string[] _conflictingDriverNames = [
        "wintun.sys",       // AmneziaVPN, CloudflareWARP, NordWhisper, Tailscale, WindscribeOpenVPN
        "wireguard.sys",    // NordLynx, WireGuard native client
        "ovpn-dco.sys",     // OpenVPN
        "Hamdrv.sys",       // Hamachi
        "RvNetMP60.sys",    // RadminVPN
        "tapnordvpn.sys",   // NordVPN OpenVPN
        "zttap300.sys",     // ZeroTier One
        ];

    private readonly IStaticConfiguration _staticConfiguration;
    private readonly INetworkInterfacesProvider _networkInterfacesProvider;

    public ConflictingNetworkInterfacesProvider(IStaticConfiguration staticConfiguration,
        INetworkInterfacesProvider networkInterfacesProvider)
    {
        _staticConfiguration = staticConfiguration;
        _networkInterfacesProvider = networkInterfacesProvider;
    }

    public IReadOnlyList<NetworkInterfaceInfo> Get()
    {
        return _networkInterfacesProvider.Get()
            .Where(ni =>
                ni.Type == PROPRIETARY_VIRTUAL_INTERFACE_TYPE ||
                (ni.Driver?.FileName is not null && _conflictingDriverNames.ContainsIgnoringCase(ni.Driver.FileName)))
            .Where(ni =>
                !Guid.TryParse(ni.Guid, out Guid niGuid) ||
                (niGuid != _staticConfiguration.ProTun.WintunAdapterGuid &&
                 niGuid != _staticConfiguration.WireGuard.NtAdapterGuid &&
                 niGuid != _staticConfiguration.WireGuard.WintunAdapterGuid))
            .Where(ni =>
                !ni.Name.EqualsIgnoringCase(_staticConfiguration.OpenVpn.TunAdapterName) &&
                !ni.Description.EqualsIgnoringCase(_staticConfiguration.OpenVpn.TapAdapterDescription))
            .Where(ni =>
                ni.Driver is null ||
                    ((ni.Driver.ComponentId is null || (
                        !ni.Driver.ComponentId.EqualsIgnoringCase(_staticConfiguration.OpenVpn.TunAdapterId) &&
                        !ni.Driver.ComponentId.EqualsIgnoringCase(_staticConfiguration.OpenVpn.TapAdapterId)))
                    &&
                    (ni.Driver.Description is null || (
                        !ni.Driver.Description.EqualsIgnoringCase(_staticConfiguration.OpenVpn.TapAdapterDescription)))))
            .ToList();
    }
}
