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
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Common.Legacy;
using ProtonVPN.Common.Legacy.NetShield;
using ProtonVPN.Common.Legacy.PortForwarding;
using ProtonVPN.Common.Legacy.Restrictions;
using ProtonVPN.Common.Legacy.Vpn;
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppServiceLogs;
using ProtonVPN.Logging.Contracts.Events.ConnectLogs;
using ProtonVPN.Logging.Contracts.Events.ProcessCommunicationLogs;
using ProtonVPN.ProcessCommunication.Contracts.Controllers;
using ProtonVPN.ProcessCommunication.Contracts.Entities.NetShield;
using ProtonVPN.ProcessCommunication.Contracts.Entities.PortForwarding;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Restrictions;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Settings;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Update;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;
using ProtonVPN.Service.KillSwitch;
using ProtonVPN.Service.Settings;
using ProtonVPN.Service.StateMachine;
using ProtonVPN.Vpn.Connection;
using ProtonVPN.Vpn.LocalAgent;
using ProtonVPN.Vpn.PortMapping;

namespace ProtonVPN.Service.ProcessCommunication;

public class ClientControllerSender : IClientController, IClientControllerSender, IServiceSettingsAware
{
    private readonly IKillSwitch _killSwitch;
    private readonly ILogger _logger;
    private readonly IEntityMapper _entityMapper;
    private readonly ILocalAgent _localAgent;
    private readonly ILocalAgentEventReceiver _localAgentEventReceiver;
    private readonly IVpnConnectionStateMachine _vpnControllerStateMachine;
    private readonly IPortMappingProtocolClient _portMappingProtocolClient;

    private VpnState _vpnState = VpnState.Default;
    private PortForwardingState? _portForwardingState;

    private CancellationTokenSource? _vpnStateCancellationTokenSource;
    private CancellationTokenSource? _portForwardingStateCancellationTokenSource;
    private CancellationTokenSource? _connectionDetailsCancellationTokenSource;
    private CancellationTokenSource? _netShieldStatisticCancellationTokenSource;
    private CancellationTokenSource? _restrictionsCancellationTokenSource;
    private CancellationTokenSource? _updateStateCancellationTokenSource;
    private readonly object _streamCancellationTokenLock = new();

    private readonly Channel<VpnStateIpcEntity> _vpnStateChannel = Channel.CreateUnbounded<VpnStateIpcEntity>();
    private readonly Channel<PortForwardingStateIpcEntity> _portForwardingStateChannel = Channel.CreateUnbounded<PortForwardingStateIpcEntity>();
    private readonly Channel<UpdateStateIpcEntity> _updateStateChannel = Channel.CreateUnbounded<UpdateStateIpcEntity>();

    public ClientControllerSender(
        IKillSwitch killSwitch,
        ILogger logger,
        IEntityMapper entityMapper,
        ILocalAgent localAgent,
        ILocalAgentEventReceiver localAgentEventReceiver,
        IVpnConnectionStateMachine vpnControllerStateMachine,
        IPortMappingProtocolClient portMappingProtocolClient)
    {
        _killSwitch = killSwitch;
        _logger = logger;
        _entityMapper = entityMapper;
        _localAgent = localAgent;
        _localAgentEventReceiver = localAgentEventReceiver;
        _vpnControllerStateMachine = vpnControllerStateMachine;

        _vpnControllerStateMachine.SubscribeToStateChanged(OnVpnStateChangedAsync);

        _portMappingProtocolClient = portMappingProtocolClient;
        _portMappingProtocolClient.StateChanged += OnPortForwardingStateChangedAsync;
    }

    public IAsyncEnumerable<VpnStateIpcEntity> StreamVpnStateChangeAsync(CancellationToken cancelToken)
    {
        CancellationTokenSource cts = new();
        lock (_streamCancellationTokenLock)
        {
            _vpnStateCancellationTokenSource?.Cancel();
            _vpnStateCancellationTokenSource = cts;
        }
        return StreamAsync(_vpnStateChannel.Reader, cts.Token);
    }

    private async IAsyncEnumerable<T> StreamAsync<T>(ChannelReader<T> reader,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            T entity = await reader.ReadAsync(cancellationToken);
            yield return entity;
        }
    }

    private async IAsyncEnumerable<TOut> MapStreamAsync<TIn, TOut>(
        ChannelReader<TIn> reader,
        Func<TIn, TOut> mapper,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (TIn entity in StreamAsync(reader, cancellationToken))
        {
            yield return mapper(entity);
        }
    }

    public IAsyncEnumerable<PortForwardingStateIpcEntity> StreamPortForwardingStateChangeAsync(CancellationToken cancelToken)
    {
        CancellationTokenSource cts = new();
        lock (_streamCancellationTokenLock)
        {
            _portForwardingStateCancellationTokenSource?.Cancel();
            _portForwardingStateCancellationTokenSource = cts;
        }
        return StreamAsync(_portForwardingStateChannel.Reader, cts.Token);
    }

    public IAsyncEnumerable<ConnectionDetailsIpcEntity> StreamConnectionDetailsChangeAsync(CancellationToken cancelToken)
    {
        CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(cancelToken);
        lock (_streamCancellationTokenLock)
        {
            _connectionDetailsCancellationTokenSource?.Cancel();
            _connectionDetailsCancellationTokenSource = cts;
        }

        return MapStreamAsync(
            _localAgentEventReceiver.ConnectionDetailsChannel.Reader,
            MapConnectionDetails,
            cts.Token);
    }

    public IAsyncEnumerable<NetShieldStatisticIpcEntity> StreamNetShieldStatisticChangeAsync(CancellationToken cancelToken)
    {
        CancellationTokenSource cts = new();
        lock (_streamCancellationTokenLock)
        {
            _netShieldStatisticCancellationTokenSource?.Cancel();
            _netShieldStatisticCancellationTokenSource = cts;
        }

        return MapStreamAsync(
            _localAgentEventReceiver.NetShieldStatsChannel.Reader,
            MapNetShieldStatistic,
            cts.Token);
    }

    public IAsyncEnumerable<RestrictionListIpcEntity> StreamRestrictionsChangeAsync(CancellationToken cancelToken)
    {
        CancellationTokenSource cts = new();
        lock (_streamCancellationTokenLock)
        {
            _restrictionsCancellationTokenSource?.Cancel();
            _restrictionsCancellationTokenSource = cts;
        }

        return MapStreamAsync(
            _localAgentEventReceiver.RestrictionsChannel.Reader,
            MapRestrictions,
            cts.Token);
    }

    public IAsyncEnumerable<UpdateStateIpcEntity> StreamUpdateStateChangeAsync(CancellationToken cancelToken)
    {
        CancellationTokenSource cts = new();
        lock (_streamCancellationTokenLock)
        {
            _updateStateCancellationTokenSource?.Cancel();
            _updateStateCancellationTokenSource = cts;
        }
        return StreamAsync(_updateStateChannel.Reader, cts.Token);
    }

    public async Task SendCurrentVpnStateAsync()
    {
        await SendStateChangeAsync(_vpnState);
    }

    private async Task OnVpnStateChangedAsync(VpnState state)
    {
        _vpnState = state;

        _logger.Info<AppServiceLog>($"VPN state changed - {GetVpnStatusLogMessage(state)}");
        await SendStateChangeAsync(state);
    }

    private static string GetVpnStatusLogMessage(VpnState state)
    {
        return $"Status '{state.Status}', Error: '{state.Error}', LocalIp: '{state.LocalIp}', " +
            $"RemoteIp: '{state.RemoteIp}', Port: {state.EndpointPort}, Label: '{state.Label}', " +
            $"VpnProtocol: '{state.VpnProtocol}', OpenVpnAdapter: '{state.OpenVpnAdapter}'";
    }

    private async Task SendStateChangeAsync(VpnState state)
    {
        _logger.Debug<ProcessCommunicationLog>($"Sending VPN state - {GetVpnStatusLogMessage(state)}");
        await _vpnStateChannel.Writer.WriteAsync(CreateVpnStateIpcEntity(state));
    }

    private VpnStateIpcEntity CreateVpnStateIpcEntity(VpnState state)
    {
        bool killSwitchEnabled = _killSwitch.GetExpectedLeakProtectionStatus(state);
        if (!killSwitchEnabled)
        {
            _vpnState = new VpnState(state.Status, state.Error, state.VpnProtocol);
        }

        return new VpnStateIpcEntity
        {
            Status = _entityMapper.Map<VpnStatus, VpnStatusIpcEntity>(state.Status),
            Error = _entityMapper.Map<VpnError, VpnErrorTypeIpcEntity>(state.Error),
            EndpointIp = state.RemoteIp,
            EndpointPort = state.EndpointPort,
            NetworkBlocked = killSwitchEnabled,
            OpenVpnAdapterType = _entityMapper.MapNullableStruct<OpenVpnAdapter, OpenVpnAdapterIpcEntity>(state.OpenVpnAdapter),
            VpnProtocol = _entityMapper.Map<VpnProtocol, VpnProtocolIpcEntity>(state.VpnProtocol),
            Label = state.Label,
            ConnectionCertificatePem = state.ConnectionCertificate?.Pem,
        };
    }

    private ConnectionDetailsIpcEntity MapConnectionDetails(ConnectionDetails connectionDetails)
    {
        _logger.Info<ProcessCommunicationLog>("Sending ConnectionDetails change while connected " +
            $"to server with '{connectionDetails.ServerIpAddress}'");

        return _entityMapper.Map<ConnectionDetails, ConnectionDetailsIpcEntity>(connectionDetails);
    }

    public async Task SendCurrentPortForwardingStateAsync()
    {
        if (_portForwardingState is not null)
        {
            await SendPortForwardingStateChangeAsync(_portForwardingState);
        }
    }

    public async Task SendUpdateStateAsync(UpdateStateIpcEntity updateState)
    {
        await _updateStateChannel.Writer.WriteAsync(updateState);
    }

    private async void OnPortForwardingStateChangedAsync(object? sender, EventArgs<PortForwardingState> e)
    {
        PortForwardingState state = e.Data;
        _logger.Debug<AppServiceLog>($"Port Forwarding state changed - {GetPortForwardingStateLogMessage(state)}");
        _portForwardingState = state;
        await SendPortForwardingStateChangeAsync(state);
    }

    private string GetPortForwardingStateLogMessage(PortForwardingState state)
    {
        StringBuilder logMessage = new StringBuilder()
            .Append($"Status '{state.Status}' triggered at '{state.TimestampUtc}'");
        if (state.MappedPort?.MappedPort is not null)
        {
            TemporaryMappedPort mappedPort = state.MappedPort;
            logMessage.Append($", Port pair {mappedPort.MappedPort}, expiring in " +
                              $"{mappedPort.Lifetime} at {mappedPort.ExpirationDateUtc}");
        }
        return logMessage.ToString();
    }

    private async Task SendPortForwardingStateChangeAsync(PortForwardingState state)
    {
        _logger.Debug<ProcessCommunicationLog>($"Sending Port Forwarding state - {GetPortForwardingStateLogMessage(state)}");
        PortForwardingStateIpcEntity stateIpcEntity = 
            _entityMapper.Map<PortForwardingState, PortForwardingStateIpcEntity>(state);
        await _portForwardingStateChannel.Writer.WriteAsync(stateIpcEntity);
    }

    private NetShieldStatisticIpcEntity MapNetShieldStatistic(NetShieldStatistic stats)
    {
        _logger.Info<ProcessCommunicationLog>($"Sending NetShield statistic triggered at '{stats.TimestampUtc}' " +
            $"[Ads: '{stats.NumOfAdvertisementUrlsBlocked}']" +
            $"[Malware: '{stats.NumOfMaliciousUrlsBlocked}']" +
            $"[Trackers: '{stats.NumOfTrackingUrlsBlocked}']" + 
            $"[Adult content: '{stats.NumOfAdultContentUrlsBlocked}']");

        return _entityMapper.Map<NetShieldStatistic, NetShieldStatisticIpcEntity>(stats);
    }

    private RestrictionListIpcEntity MapRestrictions(RestrictionsList restrictions)
    {
        _logger.Info<ProcessCommunicationLog>($"Sending restrictions '{string.Join(',', restrictions.Restrictions)}'");
        return _entityMapper.Map<RestrictionsList, RestrictionListIpcEntity>(restrictions);
    }

    public async void OnServiceSettingsChanged(MainSettingsIpcEntity settings)
    {
        VpnState vpnState = _vpnState;
        if (vpnState.Status == VpnStatus.Disconnected)
        {
            _logger.Info<ProcessCommunicationLog>($"Sending VPN Service Settings Change. " +
                $"Status: '{vpnState.Status}' (Error: '{vpnState.Error}')");
            await SendStateChangeAsync(vpnState);
        }
        else if (vpnState.Status == VpnStatus.Connected)
        {
            if (!settings.PortForwarding)
            {
                _logger.Debug<ConnectLog>("Requesting NAT-PMP client to stop.");
                await _portMappingProtocolClient.StopAsync();
            }

            VpnFeatures vpnFeatures = CreateVpnFeatures(settings);
            _vpnControllerStateMachine.UpdateVpnConfig(vpnFeatures);
            _localAgent.SetFeatures(vpnFeatures);
        }
    }

    private static VpnFeatures CreateVpnFeatures(MainSettingsIpcEntity settings)
    {
        return new()
        {
            SplitTcp = settings.SplitTcp,
            NetShieldMode = settings.NetShieldMode,
            PortForwarding = settings.PortForwarding,
            ModerateNat = settings.ModerateNat,
        };
    }
}