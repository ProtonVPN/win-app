/*
 * Copyright (c) 2020 Proton Technologies AG
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
            if (e.GetBaseException() is PipeException pipeException && 
                pipeException.Message.Contains("There was an error reading from the pipe: The pipe has been ended. (109, 0x6d)."))
            {
                _logger.Info("The service communication pipe has been ended, most likely because the user is exiting the app. " +
                             "If that is the case, the following pipe exception message can be ignored: " + e.CombinedMessage());
             
                return false;
            }

            _logger.Error(e.CombinedMessage());

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
