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
using System.Threading;
using System.Threading.Tasks;
using ProtonVPN.Common.Core.Helpers;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Common.Legacy.Threading;
using ProtonVPN.Common.Legacy.Vpn;
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.ConnectLogs;
using ProtonVPN.Logging.Contracts.Events.DisconnectLogs;
using ProtonVPN.ProcessCommunication.Contracts.Controllers;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Auth;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Settings;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;
using ProtonVPN.Service.ControllerRetries;
using ProtonVPN.Service.ProcessCommunication;
using ProtonVPN.Service.Settings;
using ProtonVPN.Vpn.Common;
using ProtonVPN.Vpn.ConnectionCertificates;
using ProtonVPN.Vpn.PortMapping;

namespace ProtonVPN.Service;

public class VpnController : IVpnController
{
    private readonly IVpnConnection _vpnConnection;
    private readonly ILogger _logger;
    private readonly IServiceSettings _serviceSettings;
    private readonly ITaskQueue _taskQueue;
    private readonly IPortMappingProtocolClient _portMappingProtocolClient;
    private readonly IClientControllerSender _appControllerCaller;
    private readonly IEntityMapper _entityMapper;
    private readonly IConnectionCertificateCache _connectionCertificateCache;
    private readonly IControllerRetryManager _controllerRetryManager;

    public VpnController(
        IVpnConnection vpnConnection,
        ILogger logger,
        IServiceSettings serviceSettings,
        ITaskQueue taskQueue,
        IPortMappingProtocolClient portMappingProtocolClient,
        IClientControllerSender appControllerCaller,
        IEntityMapper entityMapper,
        IConnectionCertificateCache connectionCertificateCache,
        IControllerRetryManager controllerRetryManager)
    {
        _vpnConnection = vpnConnection;
        _logger = logger;
        _serviceSettings = serviceSettings;
        _taskQueue = taskQueue;
        _portMappingProtocolClient = portMappingProtocolClient;
        _appControllerCaller = appControllerCaller;
        _entityMapper = entityMapper;
        _connectionCertificateCache = connectionCertificateCache;
        _controllerRetryManager = controllerRetryManager;
    }

    public async Task Connect(ConnectionRequestIpcEntity connectionRequest, CancellationToken cancelToken)
    {
        Ensure.NotNull(connectionRequest, nameof(connectionRequest));
        _controllerRetryManager.EnforceRetryId(connectionRequest);

        _logger.Info<ConnectLog>("Connect requested");

        _serviceSettings.Apply(connectionRequest.Settings);

        VpnConfig config = _entityMapper.Map<VpnConfigIpcEntity, VpnConfig>(connectionRequest.Config);
        config.OpenVpnAdapter = _serviceSettings.OpenVpnAdapter;
        IReadOnlyList<VpnHost> endpoints = _entityMapper.Map<VpnServerIpcEntity, VpnHost>(connectionRequest.Servers);
        VpnCredentials credentials = _entityMapper.Map<VpnCredentialsIpcEntity, VpnCredentials>(connectionRequest.Credentials);
        _connectionCertificateCache.Set(new ConnectionCertificate(credentials.ClientCertPem, credentials.ClientCertificateExpirationDateUtc));
        _vpnConnection.Connect(endpoints, config, credentials);
    }

    public async Task Disconnect(DisconnectionRequestIpcEntity disconnectionRequest, CancellationToken cancelToken)
    {
        Ensure.NotNull(disconnectionRequest, nameof(disconnectionRequest));
        _controllerRetryManager.EnforceRetryId(disconnectionRequest);

        _logger.Info<DisconnectLog>($"Disconnect requested (Error: {disconnectionRequest.ErrorType})");
        _serviceSettings.Apply(disconnectionRequest.Settings);
        _vpnConnection.Disconnect((VpnError)disconnectionRequest.ErrorType);
    }

    public async Task UpdateConnectionCertificate(ConnectionCertificateIpcEntity certificate, CancellationToken cancelToken)
    {
        _connectionCertificateCache.Set(new ConnectionCertificate(certificate.Pem, certificate.ExpirationDateUtc));
    }

    public async Task<NetworkTrafficIpcEntity> GetNetworkTraffic(CancellationToken cancelToken)
    {
        return _entityMapper.Map<NetworkTraffic, NetworkTrafficIpcEntity>(_vpnConnection.NetworkTraffic);
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
        _vpnConnection.RequestNetShieldStats();
    }

    public async Task RequestConnectionDetails(CancellationToken cancelToken)
    {
        _vpnConnection.RequestConnectionDetails();
    }
}