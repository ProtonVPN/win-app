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
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using ProtonVPN.Common.Core.Extensions;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Common.Legacy.Vpn;
using ProtonVPN.Crypto.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.ConnectLogs;
using ProtonVPN.Logging.Contracts.Events.ProtocolLogs;
using ProtonVPN.ProTun.Contracts;
using ProtonVPN.ProTun.Contracts.Adapters;
using ProtonVPN.ProTun.Contracts.ConnectionArguments;
using ProtonVPN.ProTun.Contracts.Traffic;
using ProtonVPN.Vpn.Common;
using ProtonVPN.Vpn.Gateways;

namespace ProtonVPN.Vpn.ProTun;

public class ProTunConnection : IProTunConnection
{
    private const int MIN_CONNECTION_TIMEOUT = 5000;
    private const int MAX_CONNECTION_TIMEOUT = 30000;

    private readonly ILogger _logger;
    private readonly IGatewayCache _gatewayCache;
    private readonly IProTunManager _proTunManager;
    private readonly IProTunTrafficManager _proTunTrafficManager;
    private readonly IX25519KeyGenerator _x25519KeyGenerator;
    private readonly IAdapterDetailsCache _adapterDetailsCache;

    private readonly Channel<VpnState> _stateChannel = Channel.CreateUnbounded<VpnState>();

    private CancellationTokenSource _cts = new();
    private volatile bool _isConnected;

    private TaskCompletionSource<bool>? _connectionTaskCompletionSource;
    private VpnCredentials _credentials;
    private VpnEndpoint? _endpoint;
    private VpnConfig? _vpnConfig;

    public VpnError LastError { get; private set; }
    public NetworkTraffic NetworkTraffic { get; private set; } = NetworkTraffic.Zero;
    public string LocalIpv4Address => _adapterDetailsCache.ClientIpv4Address;

    public ProTunConnection(
        ILogger logger,
        IGatewayCache gatewayCache,
        IProTunManager proTunManager,
        IProTunTrafficManager proTunTrafficManager,
        IX25519KeyGenerator x25519KeyGenerator,
        IAdapterDetailsCache adapterDetailsCache)
    {
        _logger = logger;
        _gatewayCache = gatewayCache;
        _proTunManager = proTunManager;
        _proTunTrafficManager = proTunTrafficManager;
        _x25519KeyGenerator = x25519KeyGenerator;
        _adapterDetailsCache = adapterDetailsCache;
    }

    public async Task<VpnError> ConnectAsync(VpnEndpoint endpoint, VpnCredentials credentials,
         VpnConfig config, CancellationToken cancellationToken)
    {
        _credentials = credentials;
        _endpoint = endpoint;
        _vpnConfig = config;

        _connectionTaskCompletionSource = new();
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _isConnected = false;

        NetworkTraffic = NetworkTraffic.Zero;
        LastError = VpnError.None;

        UpdateGatewayCache();

        StartMonitoringVpnStateAsync(_cts.Token);

        ConnectionArgs? connectionArgs = CreateConnectionArgs();
        if (connectionArgs is null)
        {
            const string ERR_MSG = "The endpoint or the config are null when creating the ProTun connection args.";
            _logger.Error<ProTunProtocolLog>(ERR_MSG);
            throw new NotImplementedException(ERR_MSG);
        }

        _proTunManager.ConnectAsync(connectionArgs, _cts.Token).FireAndForget();

        int timeout = Math.Clamp((int)_vpnConfig.WireGuardConnectionTimeout.TotalMilliseconds, MIN_CONNECTION_TIMEOUT, MAX_CONNECTION_TIMEOUT);
        // cancellationToken instead of _cts.Token to avoid cancelling the delay when disconnecting
        Task timeoutTask = Task.Delay(timeout, cancellationToken);

        Task completedTask = await Task.WhenAny(timeoutTask, _connectionTaskCompletionSource.Task);
        cancellationToken.ThrowIfCancellationRequested();

        if (completedTask == timeoutTask)
        {
            _logger.Warn<ConnectLog>($"{timeout}ms timeout reached, disconnecting.");
            return VpnError.AdapterTimeoutError;
        }

        if (!_connectionTaskCompletionSource.Task.IsCompleted || !_connectionTaskCompletionSource.Task.Result)
        {
            return LastError;
        }

        StartMonitoringNetworkTrafficAsync(_cts.Token);

        return VpnError.None;
    }

    private void StartMonitoringVpnStateAsync(CancellationToken cancellationToken)
    {
        _ = Task.Run(async () => await MonitorVpnStateAsync(cancellationToken), cancellationToken);
    }

    private async Task MonitorVpnStateAsync(CancellationToken cancellationToken)
    {
        try
        {
            await foreach (VpnState state in WatchStatesAsync(cancellationToken))
            {
                if (state.Status == VpnStatus.Connected)
                {
                    _isConnected = true;
                    SetConnectionTaskResult(true);
                    UpdateGatewayCache();
                    _proTunTrafficManager.StartAsync(cancellationToken).FireAndForget();
                }
                else
                {
                    if (state.Error != VpnError.None)
                    {
                        LastError = state.Error;
                        SetConnectionTaskResult(false);

                        if (!_isConnected)
                        {
                            _cts.Cancel();
                            return;
                        }
                    }

                    await _stateChannel.Writer.WriteAsync(state, cancellationToken);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // expected on cancellation
        }
        catch (Exception ex)
        {
            _logger.Error<WireGuardProtocolLog>("Status monitor failed.", ex);
        }
    }

    private async IAsyncEnumerable<VpnState> WatchStatesAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            yield return await _proTunManager.StateChannel.Reader.ReadAsync(cancellationToken);
        }
    }

    private void UpdateGatewayCache()
    {
        if (IPAddress.TryParse(_adapterDetailsCache.ServerGatewayIpv4Address, out IPAddress? address) && address is not null)
        {
            _gatewayCache.Save(address);
        }
    }

    private ConnectionArgs? CreateConnectionArgs()
    {
        VpnConfig? config = _vpnConfig;
        VpnEndpoint? endpoint = _endpoint;

        return config == null || endpoint is null ? null: new()
        {
            WireGuardPrivateKey = GetX25519SecretKey().Bytes,
            Peers = CreatePeers(config, endpoint),
            IsIpv6Enabled = config.IsIpv6Enabled,
            CustomDnsServers = config.CustomDns
        };
    }

    private SecretKey GetX25519SecretKey()
    {
        return _x25519KeyGenerator.FromEd25519SecretKey(_credentials.ClientKeyPair.SecretKey);
    }

    private List<ConnectionPeer> CreatePeers(VpnConfig config, VpnEndpoint endpoint)
    {
        return [new()
        {
            PeerId = $"{endpoint.Server.Ip}@{endpoint.Server.Label}",
            ServerIp = endpoint.Server.Ip,
            ServerPublicKey = endpoint.Server.X25519PublicKey.Bytes,
            UdpPorts = GetPorts(config, endpoint, VpnProtocol.ProTunUdp),
            TcpPorts = GetPorts(config, endpoint, VpnProtocol.ProTunTcp),
            TlsPorts = GetPorts(config, endpoint, VpnProtocol.ProTunTls),
            Priority = 1
        }];
    }

    private ushort[] GetPorts(VpnConfig config, VpnEndpoint endpoint, VpnProtocol protocol)
    {
        if (endpoint.VpnProtocol is VpnProtocol.Smart || endpoint.VpnProtocol == protocol)
        {
            bool hasPorts = config.Ports.TryGetValue(protocol, out IReadOnlyCollection<int>? ports);
            return hasPorts && ports is not null ? ports.Select(p => (ushort)p).ToArray() : [];
        }

        return [];
    }

    private void StartMonitoringNetworkTrafficAsync(CancellationToken cancellationToken)
    {
        _ = Task.Run(async () => await MonitorNetworkTrafficAsync(cancellationToken), cancellationToken);
    }

    private async Task MonitorNetworkTrafficAsync(CancellationToken cancellationToken)
    {
        try
        {
            await foreach (NetworkTraffic traffic in WatchTrafficAsync(cancellationToken))
            {
                NetworkTraffic = traffic;
            }
        }
        catch (OperationCanceledException)
        {
            // expected on cancellation
        }
        catch (Exception ex)
        {
            _logger.Error<WireGuardProtocolLog>("Traffic monitor failed.", ex);
        }
    }

    private async IAsyncEnumerable<NetworkTraffic> WatchTrafficAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            yield return await _proTunManager.TrafficChannel.Reader.ReadAsync(cancellationToken);
        }
    }

    public async Task DisconnectAsync()
    {
        await _proTunManager.DisconnectAsync();

        SetConnectionTaskResult(false);
    }

    private void SetConnectionTaskResult(bool result)
    {
        if (_connectionTaskCompletionSource?.Task.IsCompletedSuccessfully == false)
        {
            _connectionTaskCompletionSource?.SetResult(result);
        }
    }

    public async IAsyncEnumerable<VpnState> ObserveStatesAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            yield return await _stateChannel.Reader.ReadAsync(cancellationToken);
        }
    }
}