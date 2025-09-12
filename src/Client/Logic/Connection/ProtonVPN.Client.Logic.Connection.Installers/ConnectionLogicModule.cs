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
using ProtonVPN.Client.Logic.Connection.ConnectionErrors;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.EntityMapping;
using ProtonVPN.Client.Logic.Connection.GuestHole;
using ProtonVPN.Client.Logic.Connection.NetworkingTraffic;
using ProtonVPN.Client.Logic.Connection.RequestCreators;
using ProtonVPN.Client.Logic.Connection.ServerListGenerators;
using ProtonVPN.Client.Logic.Connection.Statistics;
using ProtonVPN.Client.Logic.Connection.Validators;
using ProtonVPN.EntityMapping.Common.Installers.Extensions;

namespace ProtonVPN.Client.Logic.Connection.Installers;

public class ConnectionLogicModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<ConnectionManager>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<PortForwardingManager>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<GuestHoleServersFileStorage>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<GuestHoleManager>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<NetworkAdapterValidator>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<VpnStateIpcEntityHandler>().AsImplementedInterfaces().AutoActivate().SingleInstance();
        builder.RegisterType<ConnectionErrorHandler>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<VpnServiceSettingsUpdater>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<ConnectedServerChecker>().AsImplementedInterfaces().AutoActivate().SingleInstance();
        builder.RegisterType<VpnStatePollingObserver>().AsImplementedInterfaces().AutoActivate().SingleInstance();
        builder.RegisterType<ChangeServerModerator>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<NetShieldStatsObserver>().AsImplementedInterfaces().SingleInstance().AutoActivate();
        builder.RegisterType<NetworkTrafficScheduler>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<NetworkTrafficManager>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<ConnectionErrorFactory>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<P2PTrafficObserver>().AsImplementedInterfaces().SingleInstance().AutoActivate();
        builder.RegisterType<ConnectionStatisticalEventsManager>().AsImplementedInterfaces().SingleInstance();

        RegisterRequestCreators(builder);
        RegisterServerListGenerators(builder);
        RegisterConnectionErrors(builder);

        builder.RegisterAllMappersInAssembly<ConnectionIntentMapper>();
    }

    private void RegisterConnectionErrors(ContainerBuilder builder)
    {
        builder.RegisterAssemblyTypes(typeof(ConnectionErrorBase).Assembly)
               .Where(typeof(IConnectionError).IsAssignableFrom)
               .AsSelf()
               .AsImplementedInterfaces()
               .SingleInstance();
    }

    private void RegisterRequestCreators(ContainerBuilder builder)
    {
        builder.RegisterType<ReconnectionRequestCreator>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<ConnectionRequestCreator>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<GuestHoleConnectionRequestCreator>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<DisconnectionRequestCreator>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<MainSettingsRequestCreator>().AsImplementedInterfaces().SingleInstance();
    }

    private void RegisterServerListGenerators(ContainerBuilder builder)
    {
        builder.RegisterType<ServerListGenerator>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<SmartServerListGenerator>().AsImplementedInterfaces().SingleInstance();
    }
}