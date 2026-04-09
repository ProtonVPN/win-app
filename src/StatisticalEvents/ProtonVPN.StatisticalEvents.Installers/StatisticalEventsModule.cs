/*
 * Copyright (c) 2026 Proton AG
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
using ProtonVPN.StatisticalEvents.Dimensions.Builders.Bases;
using ProtonVPN.StatisticalEvents.Dimensions.Mappers;
using ProtonVPN.StatisticalEvents.Dimensions.Mappers.Bases;
using ProtonVPN.StatisticalEvents.Events.Senders;
using ProtonVPN.StatisticalEvents.Files;

namespace ProtonVPN.StatisticalEvents.Installers;

public class StatisticalEventsModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<StatisticalEventsFileReaderWriter>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<AuthenticatedStatisticalEventSender>().AsImplementedInterfaces().SingleInstance().AutoActivate();
        builder.RegisterType<UnauthenticatedStatisticalEventSender>().AsImplementedInterfaces().SingleInstance().AutoActivate();

        RegisterMappers(builder);
        RegisterBuilders(builder);
        RegisterReporters(builder);
    }

    private void RegisterMappers(ContainerBuilder builder)
    {
        builder.RegisterAssemblyTypes(typeof(IDimensionMapper).Assembly)
           .Where(t => typeof(IDimensionMapper).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
           .AsImplementedInterfaces()
           .SingleInstance();
    }

    private void RegisterBuilders(ContainerBuilder builder)
    {
        builder.RegisterAssemblyTypes(typeof(IDimensionsBuilder).Assembly)
           .Where(t => typeof(IDimensionsBuilder).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
           .AsImplementedInterfaces()
           .SingleInstance();
    }

    private void RegisterReporters(ContainerBuilder builder)
    {
        builder.RegisterType<UpsellDisplayReporter>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<UpsellSuccessReporter>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<UpsellUpgradeAttemptReporter>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<ClientInstallsReporter>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<VpnConnectionReporter>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<VpnDisconnectionReporter>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<SettingsHeartbeatReporter>().AsImplementedInterfaces().SingleInstance();
    }
}