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
using System.IO;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.AppServiceLogs;
using Sentry;
using Sentry.Protocol;

namespace ProtonVPN.Common.Service
{
    public class ErrorHandler : IErrorHandler
    {
        private readonly ILogger _logger;

        public ErrorHandler(ILogger logger)
        {
            _logger = logger;
        }

        public bool HandleError(Exception e)
        {
            if (e.GetBaseException() is PipeException pipeException && pipeException.Message.Contains("0x6d)"))
            {
                _logger.Info<AppServiceCommunicationFailedLog>("The service communication " + 
                    "pipe has been ended, most likely because the service is shutting down.");

                return false;
            }

            _logger.Error<AppServiceCommunicationFailedLog>(e.CombinedMessage());

            SentrySdk.WithScope(scope =>
            {
                scope.SetTag("captured_in", "Service_ChannelDispatcher_ErrorHandler");
                scope.Level = SentryLevel.Warning;
                SentrySdk.CaptureException(e);
            });

            return false;
        }

        public void ProvideFault(Exception error, MessageVersion version, ref Message fault)
        {
        }
    }
}