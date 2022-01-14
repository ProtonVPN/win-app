/*
 * Copyright (c) 2021 Proton Technologies AG
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
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using log4net;
using Newtonsoft.Json;
using ProtonVPN.Common.Helpers;
using ProtonVPN.Common.Logging.Categorization;

namespace ProtonVPN.Common.Logging.Log4Net
{
    public class Log4NetLogger : ILogger
    {
        private readonly ILog _logger;

        public Log4NetLogger(ILog logger)
        {
            _logger = logger;
        }

        public void Debug<TEvent>(string message, Exception exception = null,
            [CallerFilePath] string sourceFilePath = "",
            [CallerMemberName] string sourceMemberName = "", 
            [CallerLineNumber] int sourceLineNumber = 0)
            where TEvent : ILogEvent, new()
        {
            CallerProfile callerProfile = new(sourceFilePath, sourceMemberName, sourceLineNumber);
            string fullLogMessage = CreateFullLogMessage<TEvent>(message, callerProfile);
            if (exception == null)
            {
                _logger.Debug(fullLogMessage);
            }
            else
            {
                _logger.Debug(fullLogMessage, exception);
            }
        }

        private string CreateFullLogMessage<TEvent>(string message, CallerProfile callerProfile) 
            where TEvent : ILogEvent, new()
        {
            string json = GenerateMetadataJson(callerProfile);
            return $"{new TEvent()} | {message} | {json}";
        }

        private string GenerateMetadataJson(CallerProfile callerProfile)
        {
            IDictionary<string, object> metadataDictionary = new Dictionary<string, object>();
            metadataDictionary.Add("Caller", 
                $"{callerProfile.SourceClassName}.{callerProfile.SourceMemberName}:{callerProfile.SourceLineNumber}");
            return JsonConvert.SerializeObject(metadataDictionary);
        }

        public void Info<TEvent>(string message, Exception exception = null, string sourceFilePath = "", string sourceMemberName = "",
            int sourceLineNumber = 0) where TEvent : ILogEvent, new()
        {
            CallerProfile callerProfile = new(sourceFilePath, sourceMemberName, sourceLineNumber);
            string fullLogMessage = CreateFullLogMessage<TEvent>(message, callerProfile);
            if (exception == null)
            {
                _logger.Info(fullLogMessage);
            }
            else
            {
                _logger.Info(fullLogMessage, exception);
            }
        }

        public void Warn<TEvent>(string message, Exception exception = null, string sourceFilePath = "", string sourceMemberName = "",
            int sourceLineNumber = 0) where TEvent : ILogEvent, new()
        {
            CallerProfile callerProfile = new(sourceFilePath, sourceMemberName, sourceLineNumber);
            string fullLogMessage = CreateFullLogMessage<TEvent>(message, callerProfile);
            if (exception == null)
            {
                _logger.Warn(fullLogMessage);
            }
            else
            {
                _logger.Warn(fullLogMessage, exception);
            }
        }

        public void Error<TEvent>(string message, Exception exception = null, string sourceFilePath = "", string sourceMemberName = "",
            int sourceLineNumber = 0) where TEvent : ILogEvent, new()
        {
            CallerProfile callerProfile = new(sourceFilePath, sourceMemberName, sourceLineNumber);
            string fullLogMessage = CreateFullLogMessage<TEvent>(message, callerProfile);
            if (exception == null)
            {
                _logger.Error(fullLogMessage);
            }
            else
            {
                _logger.Error(fullLogMessage, exception);
            }
        }

        public void Fatal<TEvent>(string message, Exception exception = null, string sourceFilePath = "", string sourceMemberName = "",
            int sourceLineNumber = 0) where TEvent : ILogEvent, new()
        {
            CallerProfile callerProfile = new(sourceFilePath, sourceMemberName, sourceLineNumber);
            string fullLogMessage = CreateFullLogMessage<TEvent>(message, callerProfile);
            if (exception == null)
            {
                _logger.Fatal(fullLogMessage);
            }
            else
            {
                _logger.Fatal(fullLogMessage, exception);
            }
        }
    }
}