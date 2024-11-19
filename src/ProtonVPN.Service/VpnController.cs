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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using ProtonVPN.Common.Helpers;
using ProtonVPN.Common.Threading;
using ProtonVPN.Common.Vpn;
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.ConnectLogs;
using ProtonVPN.Logging.Contracts.Events.DisconnectLogs;
using ProtonVPN.ProcessCommunication.Contracts.Controllers;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Auth;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Settings;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;
using ProtonVPN.Service.ProcessCommunication;
using ProtonVPN.Service.Settings;
using ProtonVPN.Vpn.Common;
using ProtonVPN.Vpn.PortMapping;
using Timer = System.Timers.Timer;

namespace ProtonVPN.Service
{
    public class VpnController : IVpnController
    {
        private readonly TimeSpan _retryIdCleanupTimerInterval = TimeSpan.FromMinutes(10);
        private readonly TimeSpan _retryIdExpirationInterval = TimeSpan.FromHours(1);

        private readonly IVpnConnection _vpnConnection;
        private readonly ILogger _logger;
        private readonly IServiceSettings _serviceSettings;
        private readonly ITaskQueue _taskQueue;
        private readonly IPortMappingProtocolClient _portMappingProtocolClient;
        private readonly IClientControllerSender _clientControllerSender;
        private readonly IEntityMapper _entityMapper;
        private readonly Timer _timer;

        private readonly object _lock = new object();
        private readonly ConcurrentDictionary<Guid, DateTimeOffset> _receivedRetryIds = [];

        public VpnController(
            IVpnConnection vpnConnection,
            ILogger logger,
            IServiceSettings serviceSettings,
            ITaskQueue taskQueue,
            IPortMappingProtocolClient portMappingProtocolClient,
            IClientControllerSender clientControllerSender,
            IEntityMapper entityMapper)
        {
            _vpnConnection = vpnConnection;
            _logger = logger;
            _serviceSettings = serviceSettings;
            _taskQueue = taskQueue;
            _portMappingProtocolClient = portMappingProtocolClient;
            _clientControllerSender = clientControllerSender;
            _entityMapper = entityMapper;

            _timer = new(_retryIdCleanupTimerInterval);
            _timer.Elapsed += OnTimedEvent;
            _timer.AutoReset = true;
            _timer.Enabled = true;
        }

        private void OnTimedEvent(object sender, EventArgs e)
        {
            lock(_lock)
            {
                List<KeyValuePair<Guid, DateTimeOffset>> list = _receivedRetryIds.ToList();
                foreach (KeyValuePair<Guid, DateTimeOffset> pair in list)
                {
                    if (pair.Value <= DateTimeOffset.UtcNow)
                    {
                        _receivedRetryIds.TryRemove(pair.Key, out _);
                    }
                }
            }
        }

        public async Task Connect(ConnectionRequestIpcEntity connectionRequest, CancellationToken cancelToken)
        {
            Ensure.NotNull(connectionRequest, nameof(connectionRequest));
            EnforceRetryId(connectionRequest.RetryId);

            _logger.Info<ConnectLog>("Connect requested");
            _serviceSettings.Apply(connectionRequest.Settings);

            VpnConfig config = _entityMapper.Map<VpnConfigIpcEntity, VpnConfig>(connectionRequest.Config);
            config.OpenVpnAdapter = _serviceSettings.OpenVpnAdapter;
            IReadOnlyList<VpnHost> endpoints = _entityMapper.Map<VpnServerIpcEntity, VpnHost>(connectionRequest.Servers);
            VpnCredentials credentials = _entityMapper.Map<VpnCredentialsIpcEntity, VpnCredentials>(connectionRequest.Credentials);
            _vpnConnection.Connect(endpoints, config, credentials);
        }

        private void EnforceRetryId(Guid retryId, [CallerMemberName] string sourceMemberName = "")
        {
            lock (_lock)
            {
                if (_receivedRetryIds.ContainsKey(retryId))
                {
                    string message = $"{sourceMemberName} request dropped because the retry ID is repeated.";
                    _logger.Info<ConnectLog>(message);
                    throw new ArgumentException(message);
                }
                DateTimeOffset expirationDate = DateTimeOffset.UtcNow + _retryIdExpirationInterval;
                _receivedRetryIds.AddOrUpdate(retryId, _ => expirationDate, (_, _) => expirationDate);
            }
        }

        public async Task Disconnect(DisconnectionRequestIpcEntity disconnectionRequest, CancellationToken cancelToken)
        {
            Ensure.NotNull(disconnectionRequest, nameof(disconnectionRequest));
            EnforceRetryId(disconnectionRequest.RetryId);

            _logger.Info<DisconnectLog>($"Disconnect requested (Error: {disconnectionRequest.ErrorType})");
            _serviceSettings.Apply(disconnectionRequest.Settings);
            _vpnConnection.Disconnect((VpnError)disconnectionRequest.ErrorType);
        }

        public async Task UpdateConnectionCertificate(ConnectionCertificateIpcEntity certificate, CancellationToken cancelToken)
        {
            _vpnConnection.UpdateAuthCertificate(certificate.Pem);
        }

        public async Task<TrafficBytesIpcEntity> GetTrafficBytes(CancellationToken cancelToken)
        {
            return _entityMapper.Map<InOutBytes, TrafficBytesIpcEntity>(_vpnConnection.Total);
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
                await _clientControllerSender.SendCurrentVpnStateAsync();
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
    }
}