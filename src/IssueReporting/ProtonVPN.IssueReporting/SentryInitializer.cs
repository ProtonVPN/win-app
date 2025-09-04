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
using System.Linq;
using System.Text;
using ProtonVPN.Builds.Variables;
using ProtonVPN.Common.Core.Helpers;
using ProtonVPN.Common.Legacy.OS.DeviceIds;
using ProtonVPN.IssueReporting.DiagnosticLogging;
using ProtonVPN.IssueReporting.HttpHandlers;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;
using Sentry;

namespace ProtonVPN.IssueReporting;

public static class SentryInitializer
{
    private static bool _isEnabled;
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

    public static void SetEnabled(bool isEnabled)
    {
        _isEnabled = isEnabled;
    }

    private static SentryOptions GetSentryOptions()
    {
        SentryOptions options = new()
        {
            Release = $"vpn.windows-{AssemblyVersion.Get()}",
            AttachStacktrace = true,
            Dsn = GlobalConfig.SentryDsn,
            ReportAssembliesMode = ReportAssembliesMode.None,
            CreateHttpMessageHandler = () => new SentryHttpClientHandler(),
            AutoSessionTracking = false,
            Debug = false,
            DiagnosticLogger = _sentryDiagnosticLogger,
            IsGlobalModeEnabled = true,
            SendClientReports = false,
        };

        options.SetBeforeSend(e =>
        {
            LogSentryEvent(e);

            if (!_isEnabled)
            {
                _logger?.Info<AppLog>("Dropping event, because the user has disabled sending crash reports.");
                return null;
            }

            e.SetTag("ProcessName", Process.GetCurrentProcess().ProcessName);
            e.SetTag("Architecture", OSArchitecture.StringValue);
            e.SetTag("WindowsSKU", WindowsSku.GetHexString());
            e.User.Id = DeviceIdStaticBuilder.GetDeviceId();
            e.SetExtra("logs", GetLogs());

            return e;
        });

        return options;
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
                if (string.IsNullOrEmpty(message))
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
        IList<string> logs = _logger?.GetRecentLogs().Reverse().ToList() ?? [];
        StringBuilder sb = new();
        foreach (string log in logs)
        {
            sb.AppendLine(log);
        }

        return sb.ToString();
    }
}