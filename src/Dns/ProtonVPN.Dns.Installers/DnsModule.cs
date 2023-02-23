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
using ProtonVPN.Dns.AlternativeRouting;
using ProtonVPN.Dns.Caching;
using ProtonVPN.Dns.HttpClients;
using ProtonVPN.Dns.NameServers;
using ProtonVPN.Dns.Resolvers;

namespace ProtonVPN.Dns.Installers
{
    public class DnsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<HttpClientFactory>().AsImplementedInterfaces().SingleInstance();

            builder.RegisterType<NameServersResolver>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<NameServersLoader>().AsImplementedInterfaces().SingleInstance();
            
            builder.RegisterType<DnsOverUdpResolver>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<DnsOverHttpsResolver>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<DnsOverHttpsTxtRecordsResolver>().AsImplementedInterfaces().SingleInstance();

            builder.RegisterType<DnsManager>().AsImplementedInterfaces().SingleInstance();

            builder.RegisterType<AlternativeRoutingHostGenerator>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<AlternativeHostsManager>().AsImplementedInterfaces().SingleInstance();
            
            builder.RegisterType<DnsOverHttpsProvidersManager>().AsImplementedInterfaces().SingleInstance();
            
            builder.RegisterType<DnsCacheManager>().AsImplementedInterfaces().SingleInstance();
        }
    }
}