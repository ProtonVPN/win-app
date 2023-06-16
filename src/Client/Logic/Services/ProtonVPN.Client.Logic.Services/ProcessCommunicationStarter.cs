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

using ProtonVPN.Client.Logic.Services.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppServiceLogs;
using ProtonVPN.Logging.Contracts.Events.ProcessCommunicationLogs;
using ProtonVPN.ProcessCommunication.Contracts;

namespace ProtonVPN.Client.Logic.Services
{
    public class ProcessCommunicationStarter : IProcessCommunicationStarter
    {
        private readonly IGrpcServer _grpcServer;
        private readonly ILogger _logger;
        private readonly IServiceCaller _serviceCaller;

        public ProcessCommunicationStarter(IGrpcServer grpcServer,
            ILogger logger, IServiceCaller serviceCaller)
        {
            _grpcServer = grpcServer;
            _logger = logger;
            _serviceCaller = serviceCaller;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                int appServerPort = StartGrpcServerAndGetPort();
                _logger.Info<ProcessCommunicationLog>($"Sending app gRPC server port {appServerPort} to service.");
                await _serviceCaller.RegisterClientAsync(appServerPort, cancellationToken);
            }
            catch (Exception e)
            {
                _logger.Error<AppServiceStartFailedLog>("An error occurred when starting the gRPC server of the VPN client.", e);
                throw;
            }
        }

        private int StartGrpcServerAndGetPort()
        {
            _grpcServer.CreateAndStart();
            int? appServerPort = _grpcServer.Port;
            if (appServerPort.HasValue)
            {
                return appServerPort.Value;
            }
            throw new Exception("The gRPC server port is null.");
        }
    }
}