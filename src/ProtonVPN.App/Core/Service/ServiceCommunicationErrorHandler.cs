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
using System.ServiceProcess;
using System.Threading.Tasks;
using ProtonVPN.Common.Helpers;
using ProtonVPN.Core.Service.Vpn;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.ProcessCommunicationLogs;
using ProtonVPN.ProcessCommunication.Contracts;

namespace ProtonVPN.Core.Service
{
    public class ServiceCommunicationErrorHandler : IServiceCommunicationErrorHandler
    {
        private readonly TimeSpan _retryTimeoutInterval = TimeSpan.FromSeconds(3);
        private readonly ILogger _logger;
        private readonly IGrpcClient _grpcClient;
        private readonly IMonitoredVpnService _monitoredVpnService;
        private readonly IVpnManager _vpnManager;
        private readonly SingleFunctionExecutor _singleFunctionExecutor;

        public ServiceCommunicationErrorHandler(ILogger logger,
            IGrpcClient grpcClient,
            IMonitoredVpnService monitoredVpnService,
            IVpnManager vpnManager)
        {
            _logger = logger;
            _grpcClient = grpcClient;
            _monitoredVpnService = monitoredVpnService;
            _vpnManager = vpnManager;
            _singleFunctionExecutor = new(Execute, retryTimeoutInterval: _retryTimeoutInterval);
        }

        public async Task HandleAsync()
        {
            await _singleFunctionExecutor.ExecuteAsync();
        }

        private async Task Execute()
        {
            await StartServiceIfStoppedAsync();
            RecreateGrpcChannelIfPipeNameChanged();
        }

        private async Task StartServiceIfStoppedAsync()
        {
            bool isStartCalled = false;
            while (true)
            {
                ServiceControllerStatus? serviceStatus = _monitoredVpnService.GetStatus();
                if (serviceStatus is null)
                {
                    return; // Service not found
                }

                switch (serviceStatus)
                {
                    case ServiceControllerStatus.Stopped:
                        _logger.Info<ProcessCommunicationErrorLog>("The service status is Stopped. Starting the service.");
                        _monitoredVpnService.StartAsync();
                        isStartCalled = true;
                        break;

                    case ServiceControllerStatus.StartPending:
                    case ServiceControllerStatus.StopPending:
                        _logger.Debug<ProcessCommunicationErrorLog>($"The service status is {serviceStatus}. Waiting.");
                        break;

                    case ServiceControllerStatus.Running:
                        if (isStartCalled)
                        {
                            _logger.Info<ProcessCommunicationErrorLog>("The service status is Running. Requesting state update and reconnect (if a connection existed).");
                            _vpnManager.GetStateAsync();
                            _vpnManager.ReconnectAsync();
                        }
                        else
                        {
                            _logger.Info<ProcessCommunicationErrorLog>("The service status is Running. No request to start it was done by this handler.");
                        }
                        return; // Service up and running

                    default:
                        _logger.Error<ProcessCommunicationErrorLog>($"The service status is {serviceStatus}, which is not supported.");
                        return; // Service returned an invalid status (Such as Pause, which we don't support)
                }

                await WaitAsync();
            }
        }

        private async Task WaitAsync()
        {
            await Task.Delay(TimeSpan.FromMilliseconds(300));
        }

        private void RecreateGrpcChannelIfPipeNameChanged()
        {
            _grpcClient.CreateIfPipeNameChanged();
        }
    }
}