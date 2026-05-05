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

using System.Threading.Channels;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.ConnectionLogs;
using ProtonVPN.Logging.Contracts.Events.DisconnectLogs;
using ProtonVPN.ProTun.Adapters;
using ProtonVPN.ProTun.Contracts;
using ProtonVPN.ProTun.Contracts.Adapters;
using ProtonVPN.ProTun.Contracts.ConnectionArguments;
using ProtonVPN.ProTun.Generated;
using ProtonVPN.ProTun.Logging;
using ProtonVPN.ProTun.StateChanges;
using ProtonVPN.ProTun.StatsResponses;
using ProTunApi = ProtonVPN.ProTun.Generated.ProTun;
using ProTunConnection = ProtonVPN.ProTun.Generated.Connection;
using ProTunWindowsConnection = ProtonVPN.ProTun.Generated.WindowsConnection;

namespace ProtonVPN.ProTun;

public class ProTunManager : IProTunManager
{
    private const LogLevel LOG_LEVEL = LogLevel.Info;
    private const ushort MTU = 1420;
    private const uint UDP_SEND_BUFFER_SIZE = 2 * 1024 * 1024; // 2 MiB
    private const uint UDP_RECEIVE_BUFFER_SIZE = 4 * 1024 * 1024; // 4 MiB
    private const uint WINTUN_BUFFER_SIZE = 4 * 1024 * 1024; // 4 MiB (Needs to be a power of two between 131072 and 67108864 inclusive)

    private readonly IProTunLogger _proTunLogger;
    private readonly IProTunStateChangeHandler _proTunStateChangeHandler;
    private readonly IProTunEventsResponseHandler _proTunEventsResponseHandler;
    private readonly IAdapterDetailsCache _adapterDetailsCache;
    private readonly ILogger _logger;

    private readonly SemaphoreSlim _protunSemaphore = new(1, 1);
    private readonly SemaphoreSlim _connectionSemaphore = new(1, 1);

    private ProTunApi? _protun;
    private ProTunWindowsConnection? _windowsConnection;
    private ProTunConnection? _connection;

    public Channel<VpnState> StateChannel { get; }
    public Channel<NetworkTraffic> TrafficChannel { get; }

    public ProTunManager(IProTunLogger proTunLogger,
        IProTunStateChangeHandler proTunStateChangeHandler,
        IProTunEventsResponseHandler proTunEventsResponseHandler,
        IAdapterDetailsCache adapterDetailsCache,
        ILogger logger)
    {
        _proTunLogger = proTunLogger;
        _proTunStateChangeHandler = proTunStateChangeHandler;
        _proTunEventsResponseHandler = proTunEventsResponseHandler;
        _adapterDetailsCache = adapterDetailsCache;
        _logger = logger;

        ProTunDllLoader.Register();

        StateChannel = _proTunStateChangeHandler.StateChannel;
        TrafficChannel = _proTunEventsResponseHandler.TrafficChannel;
    }

    public async Task InitializeAsync()
    {
        await _protunSemaphore.WaitAsync();

        try
        {
            if (_protun is null)
            {
                _protun = ProTunApi.Initialize(LOG_LEVEL, _proTunLogger);
                if (_protun is null)
                {
                    _logger.Error<ConnectionErrorLog>("Failed to initializing ProTUN");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error<ConnectionErrorLog>("Error when initializing ProTUN", ex);
        }
        finally
        {
            _protunSemaphore.Release();
        }
    }

    public async Task ConnectAsync(ConnectionArgs args, CancellationToken cancellationToken)
    {
        await _connectionSemaphore.WaitAsync();
        VpnState? disconnectState = null;
        try
        {
            TryDisconnect();
            await InitializeAsync();
            if (_protun is null)
            {
                _logger.Error<ConnectionErrorLog>("Cannot connect because ProTUN object doesn't exist");
            }
            else
            {
                InitialConnectionConfig initialConnectionConfig = CreateInitialConnectionConfig(args);
                NetworkConfig networkConfig = CreateNetworkConfig(args);
                _proTunStateChangeHandler.SetCancellationToken(cancellationToken);
                _proTunEventsResponseHandler.SetCancellationToken(cancellationToken);
                _windowsConnection = ProTunWindowsConnection.Connect(initialConnectionConfig, networkConfig,
                    _proTunStateChangeHandler, _proTunEventsResponseHandler);
                AdapterDetails adapterDetails = _windowsConnection.GetAdapterDetails().Map();
                _adapterDetailsCache.Set(adapterDetails);
                _connection = _windowsConnection.GetConnection();
            }
        }
        catch (Exception ex)
        {
            _logger.Error<ConnectionErrorLog>("Error when connecting with ProTUN", ex);
            disconnectState = TryDisconnect(VpnError.AdapterTimeoutError);
        }
        finally
        {
            _connectionSemaphore.Release();
        }

        if (disconnectState != null)
        {
            await InvokeStateChangeAsync(disconnectState, cancellationToken);
        }
    }

    private async Task InvokeStateChangeAsync(VpnState vpnState, CancellationToken cancellationToken)
    {
        await StateChannel.Writer.WriteAsync(vpnState, cancellationToken);
    }

    private void TryDisconnect()
    {
        Disconnect();
        DestroyConnection();
    }

    private VpnState TryDisconnect(VpnError vpnError)
    {
        Disconnect();
        DestroyConnection();

        return new(VpnStatus.Disconnected, vpnError, VpnProtocol.Smart);
    }

    private void Disconnect()
    {
        try
        {
            _connection?.DisconnectAndWait();
        }
        catch (Exception ex)
        {
            _logger.Error<DisconnectLog>("Error when disconnecting with ProTUN", ex);
        }

        _connection = null;
    }

    private void DestroyConnection()
    {
        try
        {
            _windowsConnection?.Destroy();
        }
        catch (Exception ex)
        {
            _logger.Error<DisconnectLog>("Error when destroying ProTUN connection", ex);
        }

        _windowsConnection = null;
    }

    private static InitialConnectionConfig CreateInitialConnectionConfig(ConnectionArgs args)
    {
        return new InitialConnectionConfig(
            wgPrivateKey: args.WireGuardPrivateKey,
            peers: MapPeers(args.Peers).ToArray(),
            networkAvailable: true,
            pcapFile: null
        );
    }

    private static IEnumerable<PeerInfo> MapPeers(List<ConnectionPeer> peers)
    {
        foreach (ConnectionPeer peer in peers)
        {
            if (peer is not null)
            {
                yield return MapPeer(peer);
            }
        }
    }

    private static PeerInfo MapPeer(ConnectionPeer peer)
    {
        return new(
            peerId: peer.PeerId,
            serverIp: peer.ServerIp,
            serverPublicKey: peer.ServerPublicKey,
            udpPorts: peer.UdpPorts,
            tcpPorts: peer.TcpPorts,
            tlsPorts: peer.TlsPorts,
            priority: peer.Priority
        );
    }

    private static NetworkConfig CreateNetworkConfig(ConnectionArgs args)
    {
        return new(CreateAdapterConfig(args), CreateUdpSocketConfig());
    }

    private static AdapterConfig CreateAdapterConfig(ConnectionArgs args)
    {
        return new AdapterConfig(
            customDnsServerIps: args.CustomDnsServers.ToArray(),
            isIpv6Enabled: args.IsIpv6Enabled,
            mtu: MTU,
            bufferSizeBytes: WINTUN_BUFFER_SIZE
        );
    }

    private static SocketConfig CreateUdpSocketConfig()
    {
        return new SocketConfig(
            sendBufferSizeBytes: UDP_SEND_BUFFER_SIZE,
            receiveBufferSizeBytes: UDP_RECEIVE_BUFFER_SIZE
        );
    }

    public async Task DisconnectAsync()
    {
        await _connectionSemaphore.WaitAsync();
        try
        {
            TryDisconnect();
        }
        finally
        {
            _connectionSemaphore.Release();
        }
    }

    public async Task RequestStatsAsync()
    {
        await _connectionSemaphore.WaitAsync();
        try
        {
            _connection?.GetStats();
        }
        catch (Exception ex)
        {
            _logger.Error<ConnectionErrorLog>("Error when requesting stats from ProTUN", ex);
        }
        finally
        {
            _connectionSemaphore.Release();
        }
    }
}