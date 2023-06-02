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

using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.ProcessCommunicationLogs;
using ProtonVPN.ProcessCommunication.Common;
using ProtonVPN.ProcessCommunication.Common.Channels;
using ProtonVPN.ProcessCommunication.Contracts;
using ProtonVPN.ProcessCommunication.Contracts.Controllers;
using ProtonVPN.ProcessCommunication.Contracts.Registration;

namespace ProtonVPN.ProcessCommunication.Service
{
    public class ServiceGrpcClient : GrpcClientBase, IServiceGrpcClient
    {
        private readonly IAppServerPortRegister _appServerPortRegister;

        public IAppController AppController { get; private set; }

        public ServiceGrpcClient(ILogger logger, 
            IGrpcChannelWrapperFactory grpcChannelWrapperFactory, 
            IAppServerPortRegister appServerPortRegister) 
            : base(logger, grpcChannelWrapperFactory)
        {
            _appServerPortRegister = appServerPortRegister;
        }

        public async Task CreateAsync(int serverPort)
        {
            _appServerPortRegister.Write(serverPort);
            await CreateWithPortAsync(serverPort);
        }

        protected override void RegisterServices(IGrpcChannelWrapper channel)
        {
            AppController = channel.CreateService<IAppController>();
        }

        public async Task RecreateAsync()
        {
            int? serverPort = ServerPort;
            if (serverPort.HasValue)
            {
                Logger.Info<ProcessCommunicationClientStartLog>($"Request to recreate a gRPC Client to Server Port {serverPort}.");
                await CreateAsync(serverPort.Value);
            }
            else
            {
                Logger.Warn<ProcessCommunicationClientStartLog>($"Cannot recreate gRPC Client because there is no previous Server Port.");
            }
        }

        public bool IsRecreatable()
        {
            return ServerPort.HasValue;
        }
    }
}