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

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ProtonVPN.Common.Core.Helpers;
using ProtonVPN.Common.Core.LocalAgent;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Common.Legacy.Threading;
using ProtonVPN.Common.Legacy.Vpn;
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.ConnectLogs;
using ProtonVPN.Logging.Contracts.Events.DisconnectLogs;
using ProtonVPN.ProcessCommunication.Contracts.Controllers;
using ProtonVPN.ProcessCommunication.Contracts.Entities.LocalAgent;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Settings;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;
using ProtonVPN.Service.ControllerRetries;
using ProtonVPN.Service.ProcessCommunication;
using ProtonVPN.Service.Settings;
using ProtonVPN.Service.StateMachine;
using ProtonVPN.Service.Vpn;
using ProtonVPN.Vpn.Connection;
using ProtonVPN.Vpn.LocalAgent;
using ProtonVPN.Vpn.PortMapping;

namespace ProtonVPN.Service;

public class VpnController : IVpnController
{
    private readonly ILogger _logger;
    private readonly IServiceSettings _serviceSettings;
    private readonly ITaskQueue _taskQueue;
    private readonly IPortMappingProtocolClient _portMappingProtocolClient;
    private readonly IClientControllerSender _appControllerCaller;
    private readonly IEntityMapper _entityMapper;
    private readonly ILocalAgentTlsCredentialsCache _localAgentTlsCredentialsCache;
    private readonly IControllerRetryManager _controllerRetryManager;
    private readonly IVpnConnectionStateMachine _stateMachine;
    private readonly ITunnelOrchestrator _tunnelOrchestrator;
    private readonly ILocalAgent _localAgent;
    private readonly ILocalAgentEventReceiver _localAgentEventReceiver;

    public VpnController(
        ILogger logger,
        IServiceSettings serviceSettings,
        ITaskQueue taskQueue,
        IPortMappingProtocolClient portMappingProtocolClient,
        IClientControllerSender appControllerCaller,
        IEntityMapper entityMapper,
        ILocalAgentTlsCredentialsCache localAgentTlsCredentialsCache,
        IControllerRetryManager controllerRetryManager,
        IVpnConnectionStateMachine stateMachine,
        ITunnelOrchestrator tunnelOrchestrator,
        ILocalAgent localAgent,
        ILocalAgentEventReceiver localAgentEventReceiver)
    {
        _logger = logger;
        _serviceSettings = serviceSettings;
        _taskQueue = taskQueue;
        _portMappingProtocolClient = portMappingProtocolClient;
        _appControllerCaller = appControllerCaller;
        _entityMapper = entityMapper;
        _localAgentTlsCredentialsCache = localAgentTlsCredentialsCache;
        _controllerRetryManager = controllerRetryManager;
        _stateMachine = stateMachine;
        _tunnelOrchestrator = tunnelOrchestrator;
        _localAgent = localAgent;
        _localAgentEventReceiver = localAgentEventReceiver;
    }

    public async Task Connect(ConnectionRequestIpcEntity connectionRequest, CancellationToken cancelToken)
    {
        Ensure.NotNull(connectionRequest, nameof(connectionRequest));
        _controllerRetryManager.EnforceRetryId(connectionRequest);

        _logger.Info<ConnectLog>("Connect requested");

        if (_stateMachine.LastError == VpnError.BaseFilteringEngineServiceNotRunning || cancelToken.IsCancellationRequested)
        {
            return;
        }

        _serviceSettings.Apply(connectionRequest.Settings);

        VpnConfig config = _entityMapper.Map<VpnConfigIpcEntity, VpnConfig>(connectionRequest.Config);
        config.OpenVpnAdapter = _serviceSettings.OpenVpnAdapter;
        IReadOnlyList<VpnHost> endpoints = _entityMapper.Map<VpnServerIpcEntity, VpnHost>(connectionRequest.Servers);
        VpnCredentials credentials = _entityMapper.Map<VpnCredentialsIpcEntity, VpnCredentials>(connectionRequest.Credentials);
        if (string.IsNullOrEmpty(credentials.ClientCertPem) &&
            string.IsNullOrEmpty(credentials.Username) &&
            string.IsNullOrEmpty(credentials.Password))
        {
            _logger.Error<ConnectLog>("Connection credentials are missing, aborting connection.");
            return;
        }

        await _localAgentTlsCredentialsCache.SetAsync(new LocalAgentTlsCredentials(
            new ConnectionCertificate(credentials.ClientCertPem, credentials.ClientCertificateExpirationDateUtc),
            credentials.ClientKeyPair), cancelToken);
        _stateMachine.Connect(endpoints, config, credentials);
    }

    public async Task Disconnect(DisconnectionRequestIpcEntity disconnectionRequest, CancellationToken cancelToken)
    {
        Ensure.NotNull(disconnectionRequest, nameof(disconnectionRequest));
        _controllerRetryManager.EnforceRetryId(disconnectionRequest);

        _logger.Info<DisconnectLog>($"Disconnect requested (Error: {disconnectionRequest.ErrorType})");
        _serviceSettings.Apply(disconnectionRequest.Settings);

        VpnError error = _entityMapper.Map<VpnErrorTypeIpcEntity, VpnError>(disconnectionRequest.ErrorType);

        _stateMachine.Disconnect(error);
    }

    public async Task UpdateLocalAgentTlsCredentialsAsync(LocalAgentTlsCredentialsIpcEntity credentialsIpcEntity, CancellationToken cancelToken)
    {
        LocalAgentTlsCredentials credentials = _entityMapper.Map<LocalAgentTlsCredentialsIpcEntity, LocalAgentTlsCredentials>(credentialsIpcEntity);
        await _localAgentTlsCredentialsCache.SetAsync(credentials, cancelToken);
    }

    public Task<NetworkTrafficIpcEntity> GetNetworkTraffic(CancellationToken cancelToken)
    {
        return Task.FromResult(_entityMapper.Map<NetworkTraffic, NetworkTrafficIpcEntity>(_tunnelOrchestrator.NetworkTraffic));
    }

    public async Task ApplySettings(MainSettingsIpcEntity settings, CancellationToken cancelToken)
    {
        Ensure.NotNull(settings, nameof(settings));
        _serviceSettings.Apply(settings);
    }

    public async Task RepeatState(CancellationToken cancelToken)
    {
        _taskQueue.Enqueue(async () =>
        {
            await _appControllerCaller.SendCurrentVpnStateAsync();
        });
    }

    public async Task RepeatPortForwardingState(CancellationToken cancelToken)
    {
        _portMappingProtocolClient.RepeatState();
    }

    public async Task RequestNetShieldStats(CancellationToken cancelToken)
    {
        _localAgent.RequestNetShieldStats();
    }

    public async Task RequestConnectionDetails(CancellationToken cancelToken)
    {
        await _localAgentEventReceiver.RequestConnectionDetailsAsync(cancelToken);
    }
}