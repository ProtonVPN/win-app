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

namespace ProtonVPN.ProcessCommunication.Common.Channels
{
    public class GrpcChannelWrapperFactory : IGrpcChannelWrapperFactory
    {
        private readonly ILogger _logger;

        public GrpcChannelWrapperFactory(ILogger logger)
        {
            _logger = logger;
        }

        public IGrpcChannelWrapper Create(int serverPort)
        {
            ValidatePort(serverPort);
            return new GrpcChannelWrapper(serverPort);
        }

        public static bool IsPortValid(int serverPort)
        {
            return serverPort > 0 && serverPort <= ushort.MaxValue;
        }

        private void ValidatePort(int serverPort)
        {
            if (!IsPortValid(serverPort))
            {
                string errorMessage = $"Cannot create a gRPC Client to Server Port {serverPort}. " +
                    $"It needs to be between [1-{ushort.MaxValue}].";
                _logger.Error<ProcessCommunicationErrorLog>(errorMessage);
                throw new ArgumentException(errorMessage, nameof(serverPort));
            }
        }
    }
}