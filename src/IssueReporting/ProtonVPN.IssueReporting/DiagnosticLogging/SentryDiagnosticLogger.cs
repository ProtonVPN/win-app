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
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;
using Sentry;

namespace ProtonVPN.IssueReporting.DiagnosticLogging
{
    public class SentryDiagnosticLogger : ISentryDiagnosticLogger
    {
        private ILogger _logger;

        public void SetLogger(ILogger logger)
        {
            _logger = logger;
        }

        public bool IsEnabled(SentryLevel level)
        {
            return level != SentryLevel.Debug;
        }

        public void Log(SentryLevel logLevel, string message, Exception exception = null, params object[] args)
        {
            string logMessage = $"SENTRY: {string.Format(message, args)}";
            switch (logLevel)
            {
                case SentryLevel.Debug:
                    _logger?.Debug<AppLog>(logMessage, exception);
                    break;
                case SentryLevel.Info:
                    _logger?.Info<AppLog>(logMessage);
                    break;
                case SentryLevel.Warning:
                    _logger?.Warn<AppLog>(logMessage);
                    break;
                case SentryLevel.Error:
                    _logger?.Error<AppLog>(logMessage);
                    break;
                case SentryLevel.Fatal:
                    _logger?.Fatal<AppCrashLog>(logMessage);
                    break;
            }
        }
    }
}