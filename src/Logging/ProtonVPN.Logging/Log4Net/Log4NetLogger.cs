﻿/*
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

using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using ProtonVPN.Common.Helpers;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Logging.Log4Net
{
    public class Log4NetLogger : Log4NetLoggerInitializer, ILogger
    {
        private readonly IList<string> _recentLogs = new List<string>();

        public Log4NetLogger(ILoggerConfiguration loggerConfiguration)
            : base(loggerConfiguration)
        {
        }

        public IList<string> GetRecentLogs()
        {
            return _recentLogs;
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
                InternalLogger.Debug(fullLogMessage);
                AddMessageToRecentLogs(fullLogMessage);
            }
            else
            {
                InternalLogger.Debug(fullLogMessage, exception);
                AddMessageToRecentLogs(fullLogMessage, exception);
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
                InternalLogger.Info(fullLogMessage);
                AddMessageToRecentLogs(fullLogMessage);
            }
            else
            {
                InternalLogger.Info(fullLogMessage, exception);
                AddMessageToRecentLogs(fullLogMessage, exception);
            }
        }

        public void Warn<TEvent>(string message, Exception exception = null, string sourceFilePath = "", string sourceMemberName = "",
            int sourceLineNumber = 0) where TEvent : ILogEvent, new()
        {
            CallerProfile callerProfile = new(sourceFilePath, sourceMemberName, sourceLineNumber);
            string fullLogMessage = CreateFullLogMessage<TEvent>(message, callerProfile);
            if (exception == null)
            {
                InternalLogger.Warn(fullLogMessage);
                AddMessageToRecentLogs(fullLogMessage);
            }
            else
            {
                InternalLogger.Warn(fullLogMessage, exception);
                AddMessageToRecentLogs(fullLogMessage, exception);
            }
        }

        public void Error<TEvent>(string message, Exception exception = null, string sourceFilePath = "", string sourceMemberName = "",
            int sourceLineNumber = 0) where TEvent : ILogEvent, new()
        {
            CallerProfile callerProfile = new(sourceFilePath, sourceMemberName, sourceLineNumber);
            string fullLogMessage = CreateFullLogMessage<TEvent>(message, callerProfile);
            if (exception == null)
            {
                InternalLogger.Error(fullLogMessage);
                AddMessageToRecentLogs(fullLogMessage);
            }
            else
            {
                InternalLogger.Error(fullLogMessage, exception);
                AddMessageToRecentLogs(fullLogMessage, exception);
            }
        }

        public void Fatal<TEvent>(string message, Exception exception = null, string sourceFilePath = "", string sourceMemberName = "",
            int sourceLineNumber = 0) where TEvent : ILogEvent, new()
        {
            CallerProfile callerProfile = new(sourceFilePath, sourceMemberName, sourceLineNumber);
            string fullLogMessage = CreateFullLogMessage<TEvent>(message, callerProfile);
            if (exception == null)
            {
                InternalLogger.Fatal(fullLogMessage);
                AddMessageToRecentLogs(fullLogMessage);
            }
            else
            {
                InternalLogger.Fatal(fullLogMessage, exception);
                AddMessageToRecentLogs(fullLogMessage, exception);
            }
        }

        private void AddMessageToRecentLogs(string message, Exception exception = null, [CallerMemberName] string level = "")
        {
            message = $"{DateTime.UtcNow:O} | {level.ToUpper()} | {message}";
            if (exception != null)
            {
                message += $" {exception}";
            }
            _recentLogs.Add(message);

            while (_recentLogs.Count > 100)
            {
                _recentLogs.RemoveAt(0);
            }
        }
    }
}