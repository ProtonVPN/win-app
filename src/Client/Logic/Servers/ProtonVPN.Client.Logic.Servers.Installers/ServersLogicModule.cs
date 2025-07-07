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
using ProtonVPN.Client.Logic.Servers.Cache;
using ProtonVPN.Client.Logic.Servers.Files;
using ProtonVPN.Client.Logic.Servers.Mappers;
using ProtonVPN.Client.Logic.Servers.Observers;
using ProtonVPN.EntityMapping.Common.Installers.Extensions;

namespace ProtonVPN.Client.Logic.Servers.Installers;

public class ServersLogicModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<ServersLoader>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<ServersCache>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<ServersFileReaderWriter>().AsImplementedInterfaces().SingleInstance();

        builder.RegisterType<DeviceLocationObserver>().AsImplementedInterfaces().AutoActivate().SingleInstance();
        builder.RegisterType<ServersObserver>().AsImplementedInterfaces().AutoActivate().SingleInstance();
        builder.RegisterType<ServersUpdater>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<ServerCountCache>().AsImplementedInterfaces().SingleInstance();
        
        builder.RegisterAllMappersInAssembly<LogicalServerMapper>();
    }
}