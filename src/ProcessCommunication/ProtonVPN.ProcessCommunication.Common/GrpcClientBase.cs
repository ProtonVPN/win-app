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
using ProtonVPN.ProcessCommunication.Common.Channels;

namespace ProtonVPN.ProcessCommunication.Common
{
    public abstract class GrpcClientBase
    {
        protected int? ServerPort { get; private set; }
        protected ILogger Logger { get; private set; }

        private readonly IGrpcChannelWrapperFactory _grpcChannelWrapperFactory;
        private readonly object _channelLock = new();
        private IGrpcChannelWrapper _channel;

        protected GrpcClientBase(ILogger logger, IGrpcChannelWrapperFactory grpcChannelWrapperFactory)
        {
            Logger = logger;
            _grpcChannelWrapperFactory = grpcChannelWrapperFactory;
        }

        protected async Task CreateWithPortAsync(int serverPort)
        {
            IGrpcChannelWrapper oldChannel = null;
            lock(_channelLock)
            {
                oldChannel = _channel;
                IGrpcChannelWrapper newChannel = _grpcChannelWrapperFactory.Create(serverPort);
                RegisterServices(newChannel);
                _channel = newChannel;
                ServerPort = serverPort;
            }
            Logger.Info<ProcessCommunicationClientStartLog>($"A new gRPC Client has been created to Server Port {serverPort}.");
            await ShutdownChannelIfExists(oldChannel);
        }

        protected abstract void RegisterServices(IGrpcChannelWrapper channel);

        private async Task ShutdownChannelIfExists(IGrpcChannelWrapper channel)
        {
            if (channel is not null)
            {
                string serverEndpoint = channel.ResolvedTarget;
                try
                {
                    await channel.ShutdownAsync();
                }
                catch (Exception e)
                {
                    Logger.Error<ProcessCommunicationErrorLog>($"A gRPC Client has failed to stop. Server endpoint: '{serverEndpoint}'", e);
                    return;
                }
                Logger.Info<ProcessCommunicationClientStopLog>($"A gRPC Client has been stopped. Server endpoint: '{serverEndpoint}'");
            }
        }
    }
}