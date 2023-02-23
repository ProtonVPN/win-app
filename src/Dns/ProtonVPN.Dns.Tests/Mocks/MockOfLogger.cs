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
using System.Collections.Generic;
using System.Linq;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization;

namespace ProtonVPN.Dns.Tests.Mocks
{
    public class MockOfLogger : ILogger
    {
        private readonly IList<Log> _logs = new List<Log>();
        public IReadOnlyList<Log> Logs => _logs as IReadOnlyList<Log>;
        private object _lock = new();

        public IList<string> GetRecentLogs()
        {
            IList<string> result;
            lock (_lock)
            {
                result = _logs.Select(l => l.Message).ToList();
            }
            return result;
        }

        public void Debug<TEvent>(string message, Exception exception = null, string sourceFilePath = "",
            string sourceMemberName = "", int sourceLineNumber = 0) where TEvent : ILogEvent, new()
        {
            lock (_lock)
            {
                _logs.Add(new Log(LogSeverity.Debug, typeof(TEvent), message, exception, sourceFilePath, sourceMemberName, sourceLineNumber));
            }
        }

        public void Info<TEvent>(string message, Exception exception = null, string sourceFilePath = "",
            string sourceMemberName = "", int sourceLineNumber = 0) where TEvent : ILogEvent, new()
        {
            lock (_lock)
            {
                _logs.Add(new Log(LogSeverity.Info, typeof(TEvent), message, exception, sourceFilePath, sourceMemberName, sourceLineNumber));
            }
        }

        public void Warn<TEvent>(string message, Exception exception = null, string sourceFilePath = "",
            string sourceMemberName = "", int sourceLineNumber = 0) where TEvent : ILogEvent, new()
        {
            lock (_lock)
            {
                _logs.Add(new Log(LogSeverity.Warn, typeof(TEvent), message, exception, sourceFilePath, sourceMemberName, sourceLineNumber));
            }
        }

        public void Error<TEvent>(string message, Exception exception = null, string sourceFilePath = "",
            string sourceMemberName = "", int sourceLineNumber = 0) where TEvent : ILogEvent, new()
        {
            lock (_lock)
            {
                _logs.Add(new Log(LogSeverity.Error, typeof(TEvent), message, exception, sourceFilePath, sourceMemberName, sourceLineNumber));
            }
        }

        public void Fatal<TEvent>(string message, Exception exception = null, string sourceFilePath = "",
            string sourceMemberName = "", int sourceLineNumber = 0) where TEvent : ILogEvent, new()
        {
            lock (_lock)
            {
                _logs.Add(new Log(LogSeverity.Fatal, typeof(TEvent), message, exception, sourceFilePath, sourceMemberName, sourceLineNumber));
            }
        }
    }
}