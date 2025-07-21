/*
 * Copyright (c) 2025 Proton AG
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

namespace ProtonVPN.Client.Logic.Auth.Installers;

public class AuthLogicModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<UserAuthenticator>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<UnauthSessionManager>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<SrpAuthenticator>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<SsoAuthenticator>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<ConnectionCertificateManager>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<ConnectionKeyManager>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<ConnectionCertificateUpdater>().AsImplementedInterfaces().AutoActivate().SingleInstance();
        builder.RegisterType<UserHashGenerator>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<WebAuthenticator>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<UserSession>().AsImplementedInterfaces().SingleInstance();
    }
}