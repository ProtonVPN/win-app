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

using ProtoBuf.Grpc.Server;
using ProtonVPN.Common.Logging;
using ProtonVPN.ProcessCommunication.Common;
using ProtonVPN.ProcessCommunication.Contracts.Controllers;
using ProtonVPN.ProcessCommunication.Contracts.Registration;
using static Grpc.Core.Server;

namespace ProtonVPN.ProcessCommunication.Service
{
    public class GrpcServer : GrpcServerBase
    {
        private readonly IServiceController _serviceController;
        private readonly IServiceServerPortRegister _serviceServerPortRegister;

        public GrpcServer(ILogger logger, IServiceController serviceController,
            IServiceServerPortRegister serviceServerPortRegister)
            : base(logger)
        {
            _serviceController = serviceController;
            _serviceServerPortRegister = serviceServerPortRegister;
        }

        protected override void RegisterServices(ServiceDefinitionCollection services)
        {
            services.AddCodeFirst<IServiceController>(_serviceController);
        }

        public override void CreateAndStart()
        {
            base.CreateAndStart();
            int? serverPort = Port;
            if (serverPort.HasValue && serverPort.Value > 0)
            {
                _serviceServerPortRegister.Write(serverPort.Value);
            }
        }

        public override async Task ShutdownAsync()
        {
            _serviceServerPortRegister.Delete();
            await base.ShutdownAsync();
        }

        public override async Task KillAsync()
        {
            _serviceServerPortRegister.Delete();
            await base.KillAsync();
        }
    }
}