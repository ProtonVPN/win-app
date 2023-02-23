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
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Helpers;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.AppLogs;
using ProtonVPN.Common.OS;
using ProtonVPN.Common.Service;
using Sentry;

namespace ProtonVPN.Common.Events
{
    public class EventPublisher : IEventPublisher
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _config;
        private readonly IDeviceInfoProvider _deviceInfoProvider;

        public EventPublisher(ILogger logger, IConfiguration config, IDeviceInfoProvider deviceInfoProvider)
        {
            _logger = logger;
            _config = config;
            _deviceInfoProvider = deviceInfoProvider;
        }

        public void Init()
        {
            SentryOptions options = GetSentryOptions();
            SentrySdk.Init(options);
        }

        private SentryOptions GetSentryOptions()
        {
            SentryOptions options = new()
            {
                Release = $"vpn.windows-{_config.AppVersion}",
                AttachStacktrace = true,
                Dsn = GlobalConfig.SentryDsn,
                ReportAssembliesMode = ReportAssembliesMode.None,
                CreateHttpClientHandler = () => new SentryHttpClientHandler(),
                AutoSessionTracking = false,
            };

            if (_logger != null)
            {
                options.Debug = true;
                options.DiagnosticLogger = new SentryDiagnosticLogger(_logger);
            }

            options.BeforeSend = e =>
            {
                LogSentryEvent(e);
                e.SetTag("ProcessName", Process.GetCurrentProcess().ProcessName);
                e.User.Id = _deviceInfoProvider.GetDeviceId();
                e.SetExtra("logs", GetLogs());

                return e;
            };

            return options;
        }

        public void CaptureError(
            Exception e,
            [CallerFilePath] string sourceFilePath = "",
            [CallerMemberName] string sourceMemberName = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            CallerProfile callerProfile = new(sourceFilePath, sourceMemberName, sourceLineNumber);
            SentrySdk.WithScope(scope =>
            {
                scope.Level = SentryLevel.Error;
                scope.SetTag("captured_in",
                    $"{callerProfile.SourceClassName}.{callerProfile.SourceMemberName}:{callerProfile.SourceLineNumber}");
                SentrySdk.CaptureException(e);
            });
        }

        public void CaptureError(string message)
        {
            SentrySdk.CaptureEvent(new SentryEvent { Message = message, Level = SentryLevel.Error });
        }

        public void CaptureMessage(string message)
        {
            SentrySdk.CaptureMessage(message);
        }

        private void LogSentryEvent(SentryEvent e)
        {
            if (_logger == null)
            {
                return;
            }

            switch (e.Level)
            {
                case SentryLevel.Debug:
                    _logger.Debug<AppLog>(e.Message?.Message);
                    break;
                case SentryLevel.Info:
                    _logger.Info<AppLog>(e.Message?.Message);
                    break;
                case SentryLevel.Warning:
                    _logger.Warn<AppLog>(e.Message?.Message);
                    break;
                case SentryLevel.Error:
                    string message = e.Message?.Message;
                    if (message.IsNullOrEmpty())
                    {
                        message = "Exception handled by Sentry";
                    }
                    _logger.Error<AppLog>(message, e.Exception);
                    break;
                case SentryLevel.Fatal:
                    _logger.Fatal<AppLog>(e.Message?.Message);
                    break;
            }
        }

        private string GetLogs()
        {
            IList<string> logs = _logger.GetRecentLogs();
            StringBuilder sb = new();
            foreach (string log in logs)
            {
                sb.AppendLine(log);
            }

            return sb.ToString();
        }
    }
}