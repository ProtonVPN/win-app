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

using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using ProtonVPN.Builds.Variables;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.OS.DeviceIds;
using ProtonVPN.IssueReporting.DiagnosticLogging;
using ProtonVPN.IssueReporting.HttpHandlers;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;
using Sentry;

namespace ProtonVPN.IssueReporting
{
    public static class SentryInitializer
    {
        private static readonly ISentryDiagnosticLogger _sentryDiagnosticLogger = new SentryDiagnosticLogger();
        private static ILogger _logger;

        public static void SetLogger(ILogger logger)
        {
            _logger = logger;
            _sentryDiagnosticLogger?.SetLogger(logger);
            DeviceIdStaticBuilder.SetLogger(logger);
        }

        public static void Run()
        {
            SentryOptions options = GetSentryOptions();
            SentrySdk.Init(options);
        }

        private static SentryOptions GetSentryOptions()
        {
            SentryOptions options = new()
            {
                Release = $"vpn.windows-{GetAppVersion()}",
                AttachStacktrace = true,
                Dsn = GlobalConfig.SentryDsn,
                ReportAssembliesMode = ReportAssembliesMode.None,
                CreateHttpMessageHandler = () => new SentryHttpClientHandler(),
                AutoSessionTracking = false,
                Debug = true,
                DiagnosticLogger = _sentryDiagnosticLogger,
            };

            options.SetBeforeSend(e =>
            {
                LogSentryEvent(e);
                e.SetTag("ProcessName", Process.GetCurrentProcess().ProcessName);
                e.User.Id = DeviceIdStaticBuilder.GetDeviceId();
                e.SetExtra("logs", GetLogs());

                return e;
            });

            return options;
        }

        private static string GetAppVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString(3);
        }

        private static void LogSentryEvent(SentryEvent e)
        {
            switch (e.Level)
            {
                case SentryLevel.Debug:
                    _logger?.Debug<AppLog>(e.Message?.Message);
                    break;
                case SentryLevel.Info:
                    _logger?.Info<AppLog>(e.Message?.Message);
                    break;
                case SentryLevel.Warning:
                    _logger?.Warn<AppLog>(e.Message?.Message);
                    break;
                case SentryLevel.Error:
                    string message = e.Message?.Message;
                    if (message.IsNullOrEmpty())
                    {
                        message = "Exception handled by Sentry";
                    }
                    _logger?.Error<AppLog>(message, e.Exception);
                    break;
                case SentryLevel.Fatal:
                    _logger?.Fatal<AppCrashLog>(e.Message?.Message);
                    break;
            }
        }

        private static string GetLogs()
        {
            IList<string> logs = _logger?.GetRecentLogs() ?? new List<string>();
            StringBuilder sb = new();
            foreach (string log in logs)
            {
                sb.AppendLine(log);
            }

            return sb.ToString();
        }
    }
}
