/*
 * Copyright (c) 2025 Proton AG
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
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using ProtonVPN.Common.Core.Extensions;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Common.Legacy;
using ProtonVPN.Common.Legacy.Threading;
using ProtonVPN.Common.Legacy.Vpn;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Crypto.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.ConnectLogs;
using ProtonVPN.Logging.Contracts.Events.DisconnectLogs;
using ProtonVPN.ProTun.Contracts;
using ProtonVPN.ProTun.Contracts.Adapters;
using ProtonVPN.ProTun.Contracts.ConnectionArguments;
using ProtonVPN.ProTun.Contracts.Traffic;
using ProtonVPN.Vpn.Common;
using ProtonVPN.Vpn.Gateways;

namespace ProtonVPN.Vpn.ProTun;

public class ProTunConnection : IAdapterSingleVpnConnection
{
    private const int MIN_CONNECTION_TIMEOUT = 5000;
    private const int MAX_CONNECTION_TIMEOUT = 30000;

    private readonly ILogger _logger;
    private readonly IStaticConfiguration _staticConfig;
    private readonly IGatewayCache _gatewayCache;
    private readonly IProTunManager _proTunManager;
    private readonly IProTunTrafficManager _proTunTrafficManager;
    private readonly IX25519KeyGenerator _x25519KeyGenerator;
    private readonly IAdapterDetailsCache _adapterDetailsCache;
    private readonly SingleAction _connectAction;
    private readonly SingleAction _disconnectAction;

    private VpnError _lastVpnError;
    private VpnEndpoint _endpoint;
    private VpnCredentials _credentials;
    private VpnConfig _vpnConfig;
    private bool _isConnected;
    private VpnStatus _vpnStatus;
    private CancellationTokenSource _disconnectCancellationTokenSource;

    public ProTunConnection(
        ILogger logger,
        IStaticConfiguration staticConfig,
        IGatewayCache gatewayCache,
        IProTunManager proTunManager,
        IProTunTrafficManager proTunTrafficManager,
        IX25519KeyGenerator x25519KeyGenerator,
        IAdapterDetailsCache adapterDetailsCache)
    {
        _logger = logger;
        _staticConfig = staticConfig;
        _gatewayCache = gatewayCache;
        _proTunManager = proTunManager;
        _proTunTrafficManager = proTunTrafficManager;
        _x25519KeyGenerator = x25519KeyGenerator;
        _adapterDetailsCache = adapterDetailsCache;
        _proTunManager.OnStateChanged += OnStateChangedAsync;
        _proTunManager.OnTrafficUpdated += OnTrafficUpdatedAsync;
        _connectAction = new SingleAction(ConnectActionAsync);
        _connectAction.Completed += OnConnectActionCompleted;
        _disconnectAction = new SingleAction(DisconnectActionAsync);
        _disconnectAction.Completed += OnDisconnectActionCompleted;
    }

    public event EventHandler<EventArgs<VpnState>> StateChanged;
    public event EventHandler<ConnectionDetails> ConnectionDetailsChanged;
    public NetworkTraffic NetworkTraffic { get; private set; } = NetworkTraffic.Zero;

    public void Connect(VpnEndpoint endpoint, VpnCredentials credentials, VpnConfig config)
    {
        _endpoint = endpoint;
        _credentials = credentials;
        _vpnConfig = config;

        _connectAction.Run();
    }

    private async Task ConnectActionAsync(CancellationToken cancellationToken)
    {
        _logger.Info<ConnectStartLog>("Connect action started.");

        InvokeStatusChange(VpnStatus.Connecting);
        UpdateGatewayCache();

        ConnectionArgs connectionArgs = CreateConnectionArgs();
        _proTunManager.ConnectAsync(connectionArgs).FireAndForget();

        CancellationToken linkedCancellationToken = CreateLinkedCancellationToken(cancellationToken);
        int timeout = Math.Clamp((int)_vpnConfig.WireGuardConnectionTimeout.TotalMilliseconds, MIN_CONNECTION_TIMEOUT, MAX_CONNECTION_TIMEOUT);
        await Task.Delay(timeout, linkedCancellationToken);
        if (!_isConnected)
        {
            _logger.Warn<ConnectLog>($"{timeout}ms timeout reached, disconnecting.");
            Disconnect(VpnError.AdapterTimeoutError);
        }
    }

    private void InvokeStatusChange(VpnStatus status, VpnError error = VpnError.None)
    {
        _vpnStatus = status;
        VpnState vpnState = CreateVpnState(status, error);
        StateChanged?.Invoke(this, new EventArgs<VpnState>(vpnState));
    }

    private void InvokeStateChange(VpnState state)
    {
        _vpnStatus = state.Status;
        VpnState vpnState = CreateVpnStateFromIncompleteState(state);
        StateChanged?.Invoke(this, new EventArgs<VpnState>(vpnState));
    }

    private VpnState CreateVpnStateFromIncompleteState(VpnState state)
    {
        string label = string.IsNullOrEmpty(state.Label) ? _endpoint?.Server.Label ?? string.Empty : state.Label;
        string localIp = state.LocalIp ?? _staticConfig.WireGuard.DefaultClientIpv4Address;
        string remoteIp = state.RemoteIp ?? _endpoint?.Server.Ip ?? string.Empty;
        int port = state.EndpointPort > 0 ? state.EndpointPort : _endpoint?.Port ?? 0;
        VpnProtocol vpnProtocol = state.VpnProtocol;
        bool portForwarding = false;

        if (_vpnConfig is not null)
        {
            if (vpnProtocol == VpnProtocol.Smart)
            {
                vpnProtocol = _vpnConfig.VpnProtocol;
            }

            portForwarding = _vpnConfig.PortForwarding;
        }

        return new VpnState(
            status: state.Status,
            error: state.Error,
            localIp: localIp,
            remoteIp: remoteIp,
            endpointPort: port,
            vpnProtocol: vpnProtocol,
            portForwarding: portForwarding,
            label: label);
    }

    private VpnState CreateVpnState(VpnStatus status, VpnError error)
    {
        if (_vpnConfig is null)
        {
            return new VpnState(status, error, _staticConfig.WireGuard.DefaultClientIpv4Address,
                _endpoint?.Server.Ip ?? string.Empty, _endpoint?.Port ?? 0, VpnProtocol.ProTunUdp,
                openVpnAdapter: null, label: _endpoint?.Server.Label ?? string.Empty);
        }

        return new VpnState(status, error, _staticConfig.WireGuard.DefaultClientIpv4Address,
            _endpoint?.Server.Ip ?? string.Empty, _endpoint?.Port ?? 0, _vpnConfig.VpnProtocol,
            _vpnConfig.PortForwarding, null, _endpoint?.Server.Label ?? string.Empty);
    }

    private void UpdateGatewayCache()
    {
        if (IPAddress.TryParse(_adapterDetailsCache.ServerGatewayIpv4Address, out IPAddress address) && address is not null)
        {
            _gatewayCache.Save(address);
        }
    }

    private ConnectionArgs CreateConnectionArgs()
    {
        return new()
        {
            WireGuardPrivateKey = GetX25519SecretKey().Bytes,
            Peers = CreatePeers(),
            IsIpv6Enabled = _vpnConfig.IsIpv6Enabled,
            CustomDnsServers = _vpnConfig.CustomDns
        };
    }

    private SecretKey GetX25519SecretKey()
    {
        return _x25519KeyGenerator.FromEd25519SecretKey(_credentials.ClientKeyPair.SecretKey);
    }

    private List<ConnectionPeer> CreatePeers()
    {
        return [new()
        {
            PeerId = $"{_endpoint.Server.Ip}@{_endpoint.Server.Label}",
            ServerIp = _endpoint.Server.Ip,
            ServerPublicKey = _endpoint.Server.X25519PublicKey.Bytes,
            UdpPorts = GetPorts(VpnProtocol.ProTunUdp),
            TcpPorts = GetPorts(VpnProtocol.ProTunTcp),
            TlsPorts = GetPorts(VpnProtocol.ProTunTls),
            Priority = 1
        }];
    }

    private ushort[] GetPorts(VpnProtocol protocol)
    {
        if (_endpoint.VpnProtocol is VpnProtocol.Smart || _endpoint.VpnProtocol == protocol)
        {
            bool hasPorts = _vpnConfig.Ports.TryGetValue(protocol, out IReadOnlyCollection<int> ports);
            return hasPorts ? ports.Select(p => (ushort)p).ToArray() : [];
        }

        return [];
    }

    private CancellationToken CreateLinkedCancellationToken(CancellationToken cancellationToken)
    {
        CancelDisconnectCancellationToken();
        _disconnectCancellationTokenSource = new CancellationTokenSource();
        CancellationTokenSource childCancellationTokenSource =
            CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _disconnectCancellationTokenSource.Token);
        return childCancellationTokenSource.Token;
    }

    private void CancelDisconnectCancellationToken()
    {
        _disconnectCancellationTokenSource?.Cancel();
    }

    public void Disconnect(VpnError error)
    {
        _lastVpnError = error;
        _disconnectAction.Run();
    }

    private async Task DisconnectActionAsync(CancellationToken cancellationToken)
    {
        _logger.Info<DisconnectLog>("Disconnect action started.");
        if (_vpnStatus is not VpnStatus.Disconnected)
        {
            InvokeStatusChange(VpnStatus.Disconnecting, _lastVpnError);
        }

        Task connectTask = _connectAction.Task;
        if (!connectTask.IsCompleted)
        {
            if (_isConnected)
            {
                _connectAction.Cancel();
            }

            await _connectAction.Task;
        }

        await _proTunManager.DisconnectAsync(_lastVpnError);

        _isConnected = false;
        CancelDisconnectCancellationToken();
    }

    private void OnConnectActionCompleted(object sender, TaskCompletedEventArgs e)
    {
        _logger.Info<ConnectLog>("Connect action completed.");
    }

    private void OnDisconnectActionCompleted(object sender, TaskCompletedEventArgs e)
    {
        _logger.Info<DisconnectLog>("Disconnect action completed.");
        InvokeStatusChange(VpnStatus.Disconnected, _lastVpnError);
        _lastVpnError = VpnError.None;
    }

    private async void OnTrafficUpdatedAsync(object sender, EventArgs<NetworkTraffic> traffic)
    {
        NetworkTraffic = traffic.Data;
    }

    private async void OnStateChangedAsync(object sender, EventArgs<VpnState> state)
    {
        switch (state.Data.Status)
        {
            case VpnStatus.Connected:
                OnVpnConnected(state);
                break;
            case VpnStatus.Disconnected:
                OnVpnDisconnected(state);
                break;
            case VpnStatus.Waiting:
                InvokeStateChange(state.Data);
                break;
            case VpnStatus.Connecting when _isConnected:
                await _proTunManager.DisconnectAsync(VpnError.NetworkUnavailable);
                break;
        }
    }

    private void OnVpnConnected(EventArgs<VpnState> state)
    {
        if (!_isConnected)
        {
            _isConnected = true;
            _logger.Info<ConnectConnectedLog>("Connected state received by ProTUN.");
            _proTunTrafficManager.Start();
            UpdateGatewayCache();
            InvokeStateChange(state.Data);
        }
    }

    private void OnVpnDisconnected(EventArgs<VpnState> state)
    {
        NetworkTraffic = NetworkTraffic.Zero;
        _isConnected = false;
        _proTunTrafficManager.Stop();
        InvokeStateChange(state.Data);
        CancelDisconnectCancellationToken();
    }
}