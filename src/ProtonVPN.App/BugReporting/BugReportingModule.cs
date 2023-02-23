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

using Autofac;
using ProtonVPN.BugReporting.Attachments;
using ProtonVPN.BugReporting.Attachments.Sources;
using ProtonVPN.BugReporting.Diagnostic;
using ProtonVPN.BugReporting.FormElements;

namespace ProtonVPN.BugReporting
{
    public class BugReportingModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterType<BugReport>().As<IBugReport>().SingleInstance();
            
            builder.RegisterType<DiagnosticsLogFileSource>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<AppLogFileSource>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<ServiceLogFileSource>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<AttachmentsLoader>().AsImplementedInterfaces().SingleInstance();
            
            builder.RegisterType<InstalledAppsLog>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<DriverInstallLog>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<UserSettingsLog>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<NetworkAdapterLog>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<RoutingTableLog>().AsImplementedInterfaces().SingleInstance();

            builder.RegisterType<NetworkLogWriter>().SingleInstance();
            builder.RegisterType<FormElementBuilder>().As<IFormElementBuilder>().SingleInstance();
            builder.RegisterType<ReportFieldProvider>().AsImplementedInterfaces().SingleInstance();
        }
    }
}