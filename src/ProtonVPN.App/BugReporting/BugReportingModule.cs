/*
 * Copyright (c) 2022 Proton Technologies AG
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

using Autofac;
using ProtonVPN.BugReporting.Attachments.Source;
using ProtonVPN.BugReporting.Diagnostic;
using ProtonVPN.BugReporting.FormElements;
using ProtonVPN.Common.Helpers;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.OS.Net.NetworkInterface;
using ProtonVPN.Common.OS.Processes;
using ProtonVPN.Core.Settings;

namespace ProtonVPN.BugReporting
{
    public class BugReportingModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterType<BugReport>().As<IBugReport>().SingleInstance();

            builder.Register(c =>
            {
                Common.Configuration.Config appConfig = c.Resolve<Common.Configuration.Config>();
                ILogger logger = c.Resolve<ILogger>();

                return new Attachments.Attachments(
                    new FilesToAttachments(
                        new ConcatenatedSequence<string>(
                            new SafeFileSource(logger,
                                new LogFileSource(
                                    appConfig.ReportBugMaxFileSize,
                                    appConfig.DiagnosticsLogFolder,
                                    appConfig.MaxDiagnosticLogsAttached)),
                            new SafeFileSource(logger,
                                new LogFileSource(
                                    appConfig.ReportBugMaxFileSize,
                                    appConfig.AppLogFolder,
                                    appConfig.MaxAppLogsAttached)),
                            new SafeFileSource(logger,
                                new LogFileSource(
                                    appConfig.ReportBugMaxFileSize,
                                    appConfig.ServiceLogFolder,
                                    appConfig.MaxServiceLogsAttached)))));
            }).SingleInstance();

            builder.Register(c => new InstalledAppsLog(c.Resolve<Common.Configuration.Config>().DiagnosticsLogFolder))
                .As<ILog>()
                .SingleInstance();

            builder.Register(c => new DriverInstallLog(c.Resolve<Common.Configuration.Config>().DiagnosticsLogFolder))
                .As<ILog>()
                .SingleInstance();

            builder.Register(c => new UserSettingsLog(
                    c.Resolve<IAppSettings>(),
                    c.Resolve<Common.Configuration.Config>().DiagnosticsLogFolder))
                .As<ILog>()
                .SingleInstance();

            builder.Register(c => new NetworkAdapterLog(
                    c.Resolve<INetworkInterfaces>(),
                    c.Resolve<Common.Configuration.Config>().DiagnosticsLogFolder))
                .As<ILog>()
                .SingleInstance();

            builder.Register(c => new RoutingTableLog(
                    c.Resolve<IOsProcesses>(),
                    c.Resolve<Common.Configuration.Config>().DiagnosticsLogFolder))
                .As<ILog>()
                .SingleInstance();

            builder.RegisterType<NetworkLogWriter>().SingleInstance();
            builder.RegisterType<FormElementBuilder>().As<IFormElementBuilder>().SingleInstance();
            builder.RegisterType<ReportFieldProvider>().AsImplementedInterfaces().SingleInstance();
        }
    }
}