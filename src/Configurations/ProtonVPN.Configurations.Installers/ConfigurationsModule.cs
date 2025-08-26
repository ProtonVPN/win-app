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
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Configurations.Files;
using ProtonVPN.Configurations.Json;
using ProtonVPN.Configurations.Repositories;
using ProtonVPN.Configurations.WireGuard;
using ProtonVPN.Serialization.Contracts.Json;

namespace ProtonVPN.Configurations.Installers;

public class ConfigurationsModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<WireGuardDnsServersCreator>().AsImplementedInterfaces().SingleInstance();

        builder.RegisterType<JsonContractDeserializer>().As<IJsonContractDeserializer>().SingleInstance();
        
        builder.RegisterType<Configuration>().As<IConfiguration>().SingleInstance();
        builder.RegisterType<StaticConfiguration>().As<IStaticConfiguration>().SingleInstance();
        builder.RegisterType<ConfigurationRepository>().AsImplementedInterfaces().SingleInstance();

#if DEBUG
        builder.RegisterType<DebugConfigurationFileManager>().AsImplementedInterfaces().SingleInstance();
#else
        builder.RegisterType<ReleaseConfigurationFileManager>().AsImplementedInterfaces().SingleInstance();
#endif
    }
}