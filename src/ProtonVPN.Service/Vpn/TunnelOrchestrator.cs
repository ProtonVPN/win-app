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

using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using ProtonVPN.Common.Core.Extensions;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Common.Legacy.Vpn;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.ConnectLogs;
using ProtonVPN.Vpn.Common;
using ProtonVPN.Vpn.NRPT;
using ProtonVPN.Vpn.ServerValidation;

namespace ProtonVPN.Service.Vpn;

internal class TunnelOrchestrator : ITunnelOrchestrator
{
    private readonly ILogger _logger;
    private readonly IIPv6Manager _ipv6Manager;
    private readonly IProTunConnection _proTunConnection;
    private readonly IWireGuardConnection _wireGuardConnection;
    private readonly IOpenVpnConnection _openVpnConnection;
    private readonly IServerValidator _serverValidator;
    private readonly INrptWrapper _nrptWrapper;

    private VpnProtocol? _protocol;

    public IVpnConnection? VpnConnection =>
        _protocol?.IsProTun() == true
            ? _proTunConnection
            : _protocol?.IsWireGuard() == true
                ? _wireGuardConnection
                : _protocol?.IsOpenVpn() == true
                    ? _openVpnConnection
                    : null;

    public Channel<VpnState> StateChannel { get; } = Channel.CreateUnbounded<VpnState>();

    public NetworkTraffic NetworkTraffic => VpnConnection?.NetworkTraffic ?? NetworkTraffic.Zero;

    public TunnelOrchestrator(
        ILogger logger,
        IIPv6Manager ipv6Manager,
        IProTunConnection proTunConnection,
        IWireGuardConnection wireGuardConnection,
        IServerValidator serverValidator,
        IOpenVpnConnection openVpnConnection,
        INrptWrapper nrptWrapper)
    {
        _logger = logger;
        _ipv6Manager = ipv6Manager;
        _proTunConnection = proTunConnection;
        _wireGuardConnection = wireGuardConnection;
        _serverValidator = serverValidator;
        _openVpnConnection = openVpnConnection;
        _nrptWrapper = nrptWrapper;
    }

    public async Task<VpnError> ConnectAsync(VpnEndpoint endpoint, VpnCredentials credentials, VpnConfig vpnConfig, CancellationToken cancellationToken)
    {
        VpnError validationError = _serverValidator.Validate(endpoint.Server);
        if (validationError != VpnError.None)
        {
            return validationError;
        }

        _protocol = vpnConfig.VpnProtocol;

        IVpnConnection? connection = VpnConnection;
        if (connection is null)
        {
            _logger.Error<ConnectLog>($"Unsupported VPN protocol {_protocol}.");
            return VpnError.Unknown;
        }

        bool isIpv6Supported = vpnConfig.IsIpv6Enabled && endpoint.Server.IsIpv6Supported;
        SetNrptConnectionConfig(vpnConfig, isIpv6Supported);

        await _ipv6Manager.HandleIPv6OnConnectAsync(endpoint.VpnProtocol, vpnConfig.OpenVpnAdapter);

        _ = Task.Run(() => MonitorStatesAsync(cancellationToken), cancellationToken);

        return await connection.ConnectAsync(endpoint, credentials, vpnConfig, cancellationToken);
    }

    private void SetNrptConnectionConfig(VpnConfig vpnConfig, bool isIpv6Supported)
    {
        _nrptWrapper.SetConnectionConfig(vpnConfig.CustomDns, vpnConfig.VpnProtocol, isIpv6Supported);
    }

    public async Task DisconnectAsync()
    {
        if (VpnConnection is null)
        {
            return;
        }

        await VpnConnection.DisconnectAsync();
    }

    private async Task MonitorStatesAsync(CancellationToken cancellationToken)
    {
        if (VpnConnection is null)
        {
            return;
        }

        try
        {
            await foreach (VpnState vpnState in VpnConnection.ObserveStatesAsync(cancellationToken).WithCancellation(cancellationToken))
            {
                await StateChannel.Writer.WriteAsync(vpnState, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            _logger.Error<ConnectLog>("State monitor failed.", ex);
        }
    }
}