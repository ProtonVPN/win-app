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
using ProtonVPN.ProTun.Adapters;
using ProtonVPN.ProTun.Dns;
using ProtonVPN.ProTun.Logging;
using ProtonVPN.ProTun.StateChanges;
using ProtonVPN.ProTun.StatsResponses;
using ProtonVPN.ProTun.Traffic;

namespace ProtonVPN.ProTun.Installers;

public class ProTunModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<ProTunLogger>().As<IProTunLogger>().SingleInstance();
        builder.RegisterType<AdapterDetailsCache>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<ProTunDnsServersCreator>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<ProTunStateChangeHandler>().As<IProTunStateChangeHandler>().SingleInstance();
        builder.RegisterType<ProTunEventsResponseHandler>().As<IProTunEventsResponseHandler>().SingleInstance();
        builder.RegisterType<ProTunManager>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<ProTunTrafficManager>().AsImplementedInterfaces().SingleInstance();
    }
}