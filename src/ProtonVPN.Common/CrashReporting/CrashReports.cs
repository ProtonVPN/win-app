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

using System.Diagnostics;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Service;
using Sentry;

namespace ProtonVPN.Common.CrashReporting
{
    public static class CrashReports
    {
        public static void Init(Config config, ILogger logger = null)
        {
            SentryOptions options = new()
            {
                Release = $"vpn.windows-{config.AppVersion}",
                AttachStacktrace = true,
                Dsn = !string.IsNullOrEmpty(GlobalConfig.SentryDsn) ? new Dsn(GlobalConfig.SentryDsn) : null,
                ReportAssemblies = false,
            };

            if (logger != null)
            {
                options.Debug = true;
                options.DiagnosticLogger = new SentryDiagnosticLogger(logger);
            }

            options.BeforeSend = e =>
            {
                e.SetTag("ProcessName", Process.GetCurrentProcess().ProcessName);
                e.User.Id = config.DeviceId;

                return e;
            };

            SentrySdk.Init(options);
        }
    }
}