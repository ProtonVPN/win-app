/*
 * Copyright (c) 2024 Proton AG
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
using ProtonVPN.EntityMapping.Common.Installers.Extensions;
using ProtonVPN.ProcessCommunication.EntityMapping.Client.Logic.Connection.Enums;
using ProtonVPN.ProcessCommunication.EntityMapping.Common.Core.Dns;
using ProtonVPN.ProcessCommunication.EntityMapping.Common.Legacy.NetShield;
using ProtonVPN.ProcessCommunication.EntityMapping.Crypto;
using ProtonVPN.ProcessCommunication.EntityMapping.Update;

namespace ProtonVPN.ProcessCommunication.Client.Installers;

public class ClientProcessCommunicationModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<GrpcClient>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<NamedPipesConnectionFactory>().AsImplementedInterfaces().SingleInstance();

        RegisterMappers(builder);
    }

    private static void RegisterMappers(ContainerBuilder builder)
    {
        builder.RegisterAllMappersInAssembly<ConnectionStatusMapper>();
        builder.RegisterAllMappersInAssembly<DnsBlockModeMapper>();
        builder.RegisterAllMappersInAssembly<NetShieldStatisticMapper>();
        builder.RegisterAllMappersInAssembly<AsymmetricKeyPairMapper>();
        builder.RegisterAllMappersInAssembly<ReleaseMapper>();
    }
}