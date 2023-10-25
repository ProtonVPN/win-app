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
using System.Threading.Tasks;
using ProtonVPN.Common.Legacy.Helpers;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.ConnectLogs;
using ProtonVPN.Logging.Contracts.Events.DisconnectLogs;
using ProtonVPN.Common.Legacy.Threading;
using ProtonVPN.Common.Legacy.Vpn;
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.ProcessCommunication.Contracts;
using ProtonVPN.ProcessCommunication.Contracts.Controllers;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Auth;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Communication;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Settings;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;
using ProtonVPN.Service.ProcessCommunication;
using ProtonVPN.Service.Settings;
using ProtonVPN.Vpn.Common;
using ProtonVPN.Vpn.PortMapping;

namespace ProtonVPN.Service
{
    public class VpnController : IVpnController
    {
        private readonly IVpnConnection _vpnConnection;
        private readonly ILogger _logger;
        private readonly IServiceSettings _serviceSettings;
        private readonly ITaskQueue _taskQueue;
        private readonly IPortMappingProtocolClient _portMappingProtocolClient;
        private readonly IServiceGrpcClient _grpcClient;
        private readonly IAppControllerCaller _appControllerCaller;
        private readonly IEntityMapper _entityMapper;

        public VpnController(
            IVpnConnection vpnConnection,
            ILogger logger,
            IServiceSettings serviceSettings,
            ITaskQueue taskQueue,
            IPortMappingProtocolClient portMappingProtocolClient,
            IServiceGrpcClient grpcClient,
            IAppControllerCaller appControllerCaller,
            IEntityMapper entityMapper)
        {
            _vpnConnection = vpnConnection;
            _logger = logger;
            _serviceSettings = serviceSettings;
            _taskQueue = taskQueue;
            _portMappingProtocolClient = portMappingProtocolClient;
            _grpcClient = grpcClient;
            _appControllerCaller = appControllerCaller;
            _entityMapper = entityMapper;
        }

        public async Task RegisterStateConsumer(StateConsumerIpcEntity stateConsumer)
        {
            if (stateConsumer?.ServerPort is null || stateConsumer.ServerPort < 1 || stateConsumer.ServerPort > ushort.MaxValue)
            {
                _logger.Error<ConnectLog>($"Received a new but invalid VPN Client gRPC port '{stateConsumer?.ServerPort}' to be registered.");
                return;
            }

            _logger.Info<ConnectLog>($"Received new VPN Client gRPC port {stateConsumer.ServerPort} to be registered.");
            await _grpcClient.CreateAsync(stateConsumer.ServerPort);
        }

        public async Task Connect(ConnectionRequestIpcEntity connectionRequest)
        {
            Ensure.NotNull(connectionRequest, nameof(connectionRequest));

            _logger.Info<ConnectLog>("Connect requested");

            _serviceSettings.Apply(connectionRequest.Settings);

            VpnConfig config = _entityMapper.Map<VpnConfigIpcEntity, VpnConfig>(connectionRequest.Config);
            config.OpenVpnAdapter = _serviceSettings.OpenVpnAdapter;
            IReadOnlyList<VpnHost> endpoints = _entityMapper.Map<VpnServerIpcEntity, VpnHost>(connectionRequest.Servers);
            VpnCredentials credentials = _entityMapper.Map<VpnCredentialsIpcEntity, VpnCredentials>(connectionRequest.Credentials);
            _vpnConnection.Connect(endpoints, config, credentials);
        }

        public async Task Disconnect(DisconnectionRequestIpcEntity disconnectionRequest)
        {
            _logger.Info<DisconnectLog>($"Disconnect requested (Error: {disconnectionRequest.ErrorType})");
            _serviceSettings.Apply(disconnectionRequest.Settings);
            _vpnConnection.Disconnect((VpnError)disconnectionRequest.ErrorType);
        }

        public async Task UpdateAuthCertificate(AuthCertificateIpcEntity certificate)
        {
            _vpnConnection.UpdateAuthCertificate(certificate.Certificate);
        }

        public async Task<TrafficBytesIpcEntity> GetTrafficBytes()
        {
            return _entityMapper.Map<InOutBytes, TrafficBytesIpcEntity>(_vpnConnection.Total);
        }

        public async Task ApplySettings(MainSettingsIpcEntity settings)
        {
            Ensure.NotNull(settings, nameof(settings));
            _serviceSettings.Apply(settings);
        }

        public async Task RepeatState()
        {
            _taskQueue.Enqueue(async () =>
            {
                await _appControllerCaller.SendCurrentVpnStateAsync();
            });
        }

        public async Task RepeatPortForwardingState()
        {
            _portMappingProtocolClient.RepeatState();
        }

        public async Task RequestNetShieldStats()
        {
            _vpnConnection.RequestNetShieldStats();
        }
    }
}