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
using static Grpc.Core.Server;

namespace ProtonVPN.ProcessCommunication.App
{
    public class GrpcServer : GrpcServerBase
    {
        private readonly IAppController _appController;

        public GrpcServer(ILogger logger, IAppController appController)
            : base(logger)
        {
            _appController = appController;
        }

        protected override void RegisterServices(ServiceDefinitionCollection services)
        {
            services.AddCodeFirst<IAppController>(_appController);
        }
    }
}