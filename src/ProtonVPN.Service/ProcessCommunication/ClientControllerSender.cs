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

using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Common.Legacy;
using ProtonVPN.Common.Legacy.NetShield;
using ProtonVPN.Common.Legacy.PortForwarding;
using ProtonVPN.Common.Legacy.Vpn;
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppServiceLogs;
using ProtonVPN.Logging.Contracts.Events.ProcessCommunicationLogs;
using ProtonVPN.ProcessCommunication.Contracts.Controllers;
using ProtonVPN.ProcessCommunication.Contracts.Entities.NetShield;
using ProtonVPN.ProcessCommunication.Contracts.Entities.PortForwarding;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Settings;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Update;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;
using ProtonVPN.Service.Settings;
using ProtonVPN.Vpn.Common;
using ProtonVPN.Vpn.NetShield;
using ProtonVPN.Vpn.PortMapping;

namespace ProtonVPN.Service.ProcessCommunication;

public class ClientControllerSender : IClientController, IClientControllerSender, IServiceSettingsAware
{
    private readonly KillSwitch.KillSwitch _killSwitch;
    private readonly ILogger _logger;
    private readonly IEntityMapper _entityMapper;
    private readonly IVpnConnection _vpnConnection;
    private readonly IPortMappingProtocolClient _portMappingProtocolClient;
    private readonly INetShieldStatisticEventManager _netShieldStatisticEventManager;

    private VpnState _vpnState = VpnState.Default;
    private PortForwardingState _portForwardingState;
    private ConnectionDetails _connectionDetails;
    private NetShieldStatistic _netShieldStatistic;

    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private readonly Channel<VpnStateIpcEntity> _vpnStateChannel = Channel.CreateUnbounded<VpnStateIpcEntity>();
    private readonly Channel<PortForwardingStateIpcEntity> _portForwardingStateChannel = Channel.CreateUnbounded<PortForwardingStateIpcEntity>();
    private readonly Channel<ConnectionDetailsIpcEntity> _connectionDetailsChannel = Channel.CreateUnbounded<ConnectionDetailsIpcEntity>();
    private readonly Channel<NetShieldStatisticIpcEntity> _netShieldStatisticChannel = Channel.CreateUnbounded<NetShieldStatisticIpcEntity>();
    private readonly Channel<UpdateStateIpcEntity> _updateStateChannel = Channel.CreateUnbounded<UpdateStateIpcEntity>();

    public ClientControllerSender(
        KillSwitch.KillSwitch killSwitch,
        ILogger logger,
        IEntityMapper entityMapper,
        IVpnConnection vpnConnection,
        IPortMappingProtocolClient portMappingProtocolClient,
        INetShieldStatisticEventManager netShieldStatisticEventManager)
    {
        _killSwitch = killSwitch;
        _logger = logger;
        _entityMapper = entityMapper;
        _vpnConnection = vpnConnection;
        _vpnConnection.StateChanged += OnVpnStateChanged;
        _vpnConnection.ConnectionDetailsChanged += OnConnectionDetailsChanged;
        _portMappingProtocolClient = portMappingProtocolClient;
        _portMappingProtocolClient.StateChanged += OnPortForwardingStateChanged;
        _netShieldStatisticEventManager = netShieldStatisticEventManager;
        _netShieldStatisticEventManager.NetShieldStatisticChanged += OnNetShieldStatisticChanged;
    }


    public IAsyncEnumerable<VpnStateIpcEntity> StreamVpnStateChangeAsync()
    {
        return StreamAsync(_vpnStateChannel);
    }

    private async IAsyncEnumerable<T> StreamAsync<T>(Channel<T> channel)
    {
        while (true)
        {
            T entity = await channel.Reader.ReadAsync(_cancellationTokenSource.Token);
            yield return entity;
        }
    }

    public IAsyncEnumerable<PortForwardingStateIpcEntity> StreamPortForwardingStateChangeAsync()
    {
        return StreamAsync(_portForwardingStateChannel);
    }

    public IAsyncEnumerable<ConnectionDetailsIpcEntity> StreamConnectionDetailsChangeAsync()
    {
        return StreamAsync(_connectionDetailsChannel);
    }

    public IAsyncEnumerable<NetShieldStatisticIpcEntity> StreamNetShieldStatisticChangeAsync()
    {
        return StreamAsync(_netShieldStatisticChannel);
    }

    public IAsyncEnumerable<UpdateStateIpcEntity> StreamUpdateStateChangeAsync()
    {
        return StreamAsync(_updateStateChannel);
    }

    public async Task SendCurrentVpnStateAsync()
    {
        await SendStateChangeAsync(_vpnState);
    }

    private async void OnVpnStateChanged(object sender, EventArgs<VpnState> e)
    {
        VpnState state = e.Data;
        _logger.Info<AppServiceLog>($"VPN state changed - {GetVpnStatusLogMessage(state)}");
        _vpnState = state ?? VpnState.Default;
        await SendStateChangeAsync(state);
    }

    private string GetVpnStatusLogMessage(VpnState state)
    {
        return $"Status '{state.Status}', Error: '{state.Error}', LocalIp: '{state.LocalIp}', " +
            $"RemoteIp: '{state.RemoteIp}', Label: '{state.Label}', " +
            $"VpnProtocol: '{state.VpnProtocol}', OpenVpnAdapter: '{state.OpenVpnAdapter}'";
    }

    private async Task SendStateChangeAsync(VpnState state)
    {
        _logger.Debug<ProcessCommunicationLog>($"Sending VPN state - {GetVpnStatusLogMessage(state)}");
        await _vpnStateChannel.Writer.WriteAsync(CreateVpnStateIpcEntity(state));
    }

    private VpnStateIpcEntity CreateVpnStateIpcEntity(VpnState state)
    {
        bool killSwitchEnabled = _killSwitch.ExpectedLeakProtectionStatus(state);
        if (!killSwitchEnabled)
        {
            _vpnState = new VpnState(state.Status, state.Error, state.VpnProtocol);
        }

        return new VpnStateIpcEntity
        {
            Status = _entityMapper.Map<VpnStatus, VpnStatusIpcEntity>(state.Status),
            Error = _entityMapper.Map<VpnError, VpnErrorTypeIpcEntity>(state.Error),
            EndpointIp = state.RemoteIp,
            NetworkBlocked = killSwitchEnabled,
            OpenVpnAdapterType = _entityMapper.MapNullableStruct<OpenVpnAdapter, OpenVpnAdapterIpcEntity>(state.OpenVpnAdapter),
            VpnProtocol = _entityMapper.Map<VpnProtocol, VpnProtocolIpcEntity>(state.VpnProtocol),
            Label = state.Label,
        };
    }

    private async void OnConnectionDetailsChanged(object sender, ConnectionDetails connectionDetails)
    {
        _connectionDetails = connectionDetails;
        await SendConnectionDetailsChangeAsync(connectionDetails);
    }

    private async Task SendConnectionDetailsChangeAsync(ConnectionDetails connectionDetails)
    {
        _logger.Info<ProcessCommunicationLog>("Sending ConnectionDetails change while connected " +
            $"to server with IP '{connectionDetails.ServerIpAddress}'");
        ConnectionDetailsIpcEntity connectionDetailsIpcEntity =
            _entityMapper.Map<ConnectionDetails, ConnectionDetailsIpcEntity>(connectionDetails);
        await _connectionDetailsChannel.Writer.WriteAsync(connectionDetailsIpcEntity);
    }

    public async Task SendCurrentPortForwardingStateAsync()
    {
        await SendPortForwardingStateChangeAsync(_portForwardingState);
    }

    public async Task SendUpdateStateAsync(UpdateStateIpcEntity updateState)
    {
        await _updateStateChannel.Writer.WriteAsync(updateState);
    }

    private async void OnPortForwardingStateChanged(object sender, EventArgs<PortForwardingState> e)
    {
        PortForwardingState state = e.Data;
        _logger.Info<AppServiceLog>($"Port Forwarding state changed - {GetPortForwardingStateLogMessage(state)}");
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

    private async void OnNetShieldStatisticChanged(object sender, NetShieldStatistic stats)
    {
        _netShieldStatistic = stats;
        _logger.Info<ProcessCommunicationLog>($"Sending NetShield statistic triggered at '{stats.TimestampUtc}' " +
            $"[Ads: '{stats.NumOfAdvertisementUrlsBlocked}']" +
            $"[Malware: '{stats.NumOfMaliciousUrlsBlocked}']" +
            $"[Trackers: '{stats.NumOfTrackingUrlsBlocked}']");
        NetShieldStatisticIpcEntity statsIpcEntity =
            _entityMapper.Map<NetShieldStatistic, NetShieldStatisticIpcEntity>(stats);
        await _netShieldStatisticChannel.Writer.WriteAsync(statsIpcEntity);
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
            _vpnConnection.SetFeatures(CreateVpnFeatures(settings));
        }
    }

    private VpnFeatures CreateVpnFeatures(MainSettingsIpcEntity settings)
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
