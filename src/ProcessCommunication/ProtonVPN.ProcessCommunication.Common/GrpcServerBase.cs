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
using Grpc.Core;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.ProcessCommunicationLogs;
using ProtonVPN.ProcessCommunication.Contracts;
using static Grpc.Core.Server;

namespace ProtonVPN.ProcessCommunication.Common
{
    public abstract class GrpcServerBase : IGrpcServer
    {
        public int? Port { get; private set; }

        public event EventHandler? OnStart;

        protected ILogger Logger { get; private set; }
        protected Server? Server { get; private set; }

        protected GrpcServerBase(ILogger logger)
        {
            Logger = logger;
        }

        public virtual void CreateAndStart()
        {
            ServerPort serverPort = new("localhost", ServerPort.PickUnused, ServerCredentials.Insecure);
            Server = new Server() { Ports = { serverPort } };
            RegisterServices(Server.Services);
            Server.Start();
            int port = Server.Ports.First().BoundPort;
            Logger.Info<ProcessCommunicationServerStartLog>($"A new gRPC Server has been created using Port {port}.");
            Port = port;
            OnStart?.Invoke(this, EventArgs.Empty);
        }

        protected abstract void RegisterServices(ServiceDefinitionCollection services);

        public virtual async Task ShutdownAsync()
        {
            Server? server = Server;
            if (server is not null)
            {
                await server.ShutdownAsync();
            }
        }

        public virtual async Task KillAsync()
        {
            Server? server = Server;
            if (server is not null)
            {
                await server.KillAsync();
            }
        }
    }
}