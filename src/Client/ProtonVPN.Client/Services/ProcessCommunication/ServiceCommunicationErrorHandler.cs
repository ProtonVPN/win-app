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

using System.ServiceProcess;
using ProtonVPN.Client.Contracts.ProcessCommunication;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Services.Contracts;
using ProtonVPN.Common.Core.Helpers;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppServiceLogs;
using ProtonVPN.ProcessCommunication.Contracts;
using ProtonVPN.StatisticalEvents.Contracts.Dimensions;

namespace ProtonVPN.Client.Services.ProcessCommunication;

public class ServiceCommunicationErrorHandler : IServiceCommunicationErrorHandler
{
    private readonly TimeSpan _retryTimeoutInterval = TimeSpan.FromSeconds(3);
    private readonly ILogger _logger;
    private readonly IGrpcClient _grpcClient;
    private readonly IServiceManager _serviceManager;
    private readonly IConnectionManager _connectionManager;
    private readonly SingleFunctionExecutor _singleFunctionExecutor;

    public ServiceCommunicationErrorHandler(ILogger logger,
        IGrpcClient grpcClient,
        IServiceManager serviceManager,
        IConnectionManager connectionManager)
    {
        _logger = logger;
        _grpcClient = grpcClient;
        _serviceManager = serviceManager;
        _connectionManager = connectionManager;
        _singleFunctionExecutor = new(ExecuteAsync, retryTimeoutInterval: _retryTimeoutInterval);
    }

    public async Task HandleAsync()
    {
        await _singleFunctionExecutor.ExecuteAsync();
    }

    private async Task ExecuteAsync()
    {
        await StartServiceIfStoppedAsync();
        RecreateGrpcChannelIfPipeNameChanged();
    }

    private async Task StartServiceIfStoppedAsync()
    {
        bool isStartCalled = false;
        while (true)
        {
            ServiceControllerStatus? serviceStatus = _serviceManager.GetStatus();
            if (serviceStatus is null)
            {
                _logger.Error<AppServiceStartFailedLog>("Cannot start the service because it was not found.");
                return;
            }

            switch (serviceStatus)
            {
                case ServiceControllerStatus.Stopped:
                    _logger.Info<AppServiceStartLog>("The service status is Stopped. Starting the service.");
                    _serviceManager.StartAsync();
                    isStartCalled = true;
                    break;

                case ServiceControllerStatus.StartPending:
                case ServiceControllerStatus.StopPending:
                    _logger.Debug<AppServiceLog>($"The service status is {serviceStatus}. Waiting.");
                    break;

                case ServiceControllerStatus.Running:
                    if (isStartCalled)
                    {
                        _logger.Info<AppServiceLog>("The service status is Running. Requesting reconnect (if a connection existed).");
                        _connectionManager.ReconnectAsync(VpnTriggerDimension.Auto);
                    }
                    else
                    {
                        _logger.Info<AppServiceLog>("The service status is Running. No request to start it was done by this handler.");
                    }
                    return; // Service up and running

                default:
                    _logger.Error<AppServiceStartFailedLog>($"The service status is {serviceStatus}, which is not supported.");
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