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

using System;
using ProtonVPN.Common.Core.Extensions;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Common.Legacy;
using ProtonVPN.Common.Legacy.Vpn;
using ProtonVPN.Vpn.Common;
using ProtonVPN.Vpn.NRPT;

namespace ProtonVPN.Vpn.Connection;

internal class VpnProtocolWrapper : ISingleVpnConnection
{
    private readonly ISingleVpnConnection _openVpnConnection;
    private readonly ISingleVpnConnection _wireGuardConnection;
    private readonly INrptWrapper _nrptWrapper;

    private VpnProtocol _vpnProtocol;

    public VpnProtocolWrapper(ISingleVpnConnection openVpnConnection,
        ISingleVpnConnection wireGuardConnection,
        INrptWrapper nrptWrapper)
    {
        _openVpnConnection = openVpnConnection;
        _wireGuardConnection = wireGuardConnection;
        _nrptWrapper = nrptWrapper;

        _openVpnConnection.StateChanged += OnStateChanged;
        _wireGuardConnection.StateChanged += OnStateChanged;
    }

    public event EventHandler<EventArgs<VpnState>> StateChanged;
    public event EventHandler<ConnectionDetails> ConnectionDetailsChanged
    {
        add
        {
            _openVpnConnection.ConnectionDetailsChanged += value;
            _wireGuardConnection.ConnectionDetailsChanged += value;
        }
        remove
        {
            _openVpnConnection.ConnectionDetailsChanged -= value;
            _wireGuardConnection.ConnectionDetailsChanged -= value;
        }
    }

    public NetworkTraffic NetworkTraffic => VpnConnection?.NetworkTraffic ?? NetworkTraffic.Zero;

    public void Connect(VpnEndpoint endpoint, VpnCredentials credentials, VpnConfig config)
    {
        _vpnProtocol = config.VpnProtocol;
        SetNrptConnectionConfig(endpoint, config);
        VpnConnection.Connect(endpoint, credentials, config);
    }

    private void SetNrptConnectionConfig(VpnEndpoint endpoint, VpnConfig config)
    {
        bool isIpv6Supported = config.IsIpv6Enabled && endpoint.Server.IsIpv6Supported;
        _nrptWrapper.SetConnectionConfig(config.CustomDns, _vpnProtocol, isIpv6Supported);
    }

    public void Disconnect(VpnError error)
    {
        if (VpnConnection == null)
        {
            _openVpnConnection.Disconnect(error);
            _wireGuardConnection.Disconnect(error);
            OnStateChanged(this, new EventArgs<VpnState>(new VpnState(VpnStatus.Disconnected, _vpnProtocol)));
        }
        else
        {
            VpnConnection.Disconnect(error);
        }
    }

    public void SetFeatures(VpnFeatures vpnFeatures)
    {
        VpnConnection?.SetFeatures(vpnFeatures);
    }

    public void RequestNetShieldStats()
    {
        VpnConnection?.RequestNetShieldStats();
    }

    public void RequestConnectionDetails()
    {
        VpnConnection?.RequestConnectionDetails();
    }

    private void OnStateChanged(object sender, EventArgs<VpnState> e)
    {
        StateChanged?.Invoke(this, e);
    }

    private ISingleVpnConnection VpnConnection => _vpnProtocol.IsWireGuard()
        ? _wireGuardConnection
        : _vpnProtocol.IsOpenVpn()
            ? _openVpnConnection
            : null;
}