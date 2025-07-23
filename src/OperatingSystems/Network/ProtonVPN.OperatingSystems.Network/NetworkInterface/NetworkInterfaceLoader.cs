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

using ProtonVPN.Common.Core.Extensions;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.OperatingSystems.Network.Contracts;

namespace ProtonVPN.OperatingSystems.Network.NetworkInterface;

public class NetworkInterfaceLoader : INetworkInterfaceLoader
{
    private readonly IStaticConfiguration _config;
    private readonly ISystemNetworkInterfaces _networkInterfaces;

    public NetworkInterfaceLoader(IStaticConfiguration config, ISystemNetworkInterfaces networkInterfaces)
    {
        _networkInterfaces = networkInterfaces;
        _config = config;
    }

    public INetworkInterface GetOpenVpnTapInterface()
    {
        return _networkInterfaces.GetByDescription(_config.OpenVpn.TapAdapterDescription);
    }

    public INetworkInterface GetOpenVpnTunInterface()
    {
        return _networkInterfaces.GetByName(_config.OpenVpn.TunAdapterName);
    }

    public INetworkInterface GetWireGuardInterface(VpnProtocol protocol)
    {
        Guid guid = protocol switch
        {
            VpnProtocol.WireGuardUdp => _config.WireGuard.NtAdapterGuid,
            VpnProtocol.WireGuardTcp or VpnProtocol.WireGuardTls => _config.WireGuard.WintunAdapterGuid,
            _ => throw new Exception($"Can't provide GUID for protocol {protocol}")
        };

        return _networkInterfaces.GetById(guid);
    }

    public INetworkInterface GetByVpnProtocol(VpnProtocol vpnProtocol, OpenVpnAdapter? openVpnAdapter)
    {
        return vpnProtocol.IsWireGuard()
            ? GetWireGuardInterface(vpnProtocol)
            : GetByOpenVpnAdapter(openVpnAdapter);
    }

    public INetworkInterface GetByOpenVpnAdapter(OpenVpnAdapter? openVpnAdapter)
    {
        return openVpnAdapter switch
        {
            OpenVpnAdapter.Tap => GetOpenVpnTapInterface(),
            OpenVpnAdapter.Tun => GetOpenVpnTunInterface(),
            _ => new NullNetworkInterface()
        };
    }
}