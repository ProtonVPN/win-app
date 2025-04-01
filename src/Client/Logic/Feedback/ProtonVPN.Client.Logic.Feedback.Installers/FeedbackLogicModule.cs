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
using ProtonVPN.Client.Logic.Feedback.Attachments;
using ProtonVPN.Client.Logic.Feedback.Attachments.Sources;
using ProtonVPN.Client.Logic.Feedback.Diagnostics;
using ProtonVPN.Client.Logic.Feedback.Diagnostics.Logs;

namespace ProtonVPN.Client.Logic.Feedback.Installers;

public class FeedbackLogicModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<ReportIssueDataProvider>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<ReportIssueSender>().AsImplementedInterfaces().SingleInstance();

        builder.RegisterType<DiagnosticsLogFileSource>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<ClientLogFileSource>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<ServiceLogFileSource>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<AttachmentsLoader>().AsImplementedInterfaces().SingleInstance();

        builder.RegisterType<InstalledAppsLog>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<InstalledDriversLog>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<DriverInstallLog>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<UserSettingsLog>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<NetworkAdapterLog>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<RoutingTableLog>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<AppInstallLog>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<RegistryLog>().AsImplementedInterfaces().SingleInstance();

        builder.RegisterType<DiagnosticLogWriter>().AsImplementedInterfaces().SingleInstance();
    }
}