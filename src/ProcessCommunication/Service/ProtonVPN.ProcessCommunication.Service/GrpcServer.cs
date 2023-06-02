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
using ProtonVPN.Logging.Contracts;
using ProtonVPN.ProcessCommunication.Common;
using ProtonVPN.ProcessCommunication.Contracts.Controllers;
using ProtonVPN.ProcessCommunication.Contracts.Registration;
using static Grpc.Core.Server;

namespace ProtonVPN.ProcessCommunication.Service
{
    public class GrpcServer : GrpcServerBase
    {
        private readonly IVpnController _vpnController;
        private readonly IUpdateController _updateController;
        private readonly IServiceServerPortRegister _serviceServerPortRegister;
        private readonly IAppServerPortRegister _appServerPortRegister;

        public GrpcServer(ILogger logger, 
            IVpnController vpnController, 
            IUpdateController updateController,
            IServiceServerPortRegister serviceServerPortRegister,
            IAppServerPortRegister appServerPortRegister)
            : base(logger)
        {
            _vpnController = vpnController;
            _updateController = updateController;
            _serviceServerPortRegister = serviceServerPortRegister;
            _appServerPortRegister = appServerPortRegister;
        }

        protected override void RegisterServices(ServiceDefinitionCollection services)
        {
            services.AddCodeFirst(_vpnController);
            services.AddCodeFirst(_updateController);
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
            _appServerPortRegister.Delete();
            await base.ShutdownAsync();
        }

        public override async Task KillAsync()
        {
            _serviceServerPortRegister.Delete();
            _appServerPortRegister.Delete();
            await base.KillAsync();
        }
    }
}