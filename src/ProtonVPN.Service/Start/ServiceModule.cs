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

using System;
using Autofac;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.Events;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Log4Net;
using ProtonVPN.Common.OS;
using ProtonVPN.Common.OS.Net;
using ProtonVPN.Common.OS.Net.NetworkInterface;
using ProtonVPN.Common.OS.Processes;
using ProtonVPN.Common.OS.Services;
using ProtonVPN.Common.Service;
using ProtonVPN.Common.Text.Serialization;
using ProtonVPN.Common.Threading;
using ProtonVPN.Service.Driver;
using ProtonVPN.Service.Firewall;
using ProtonVPN.Service.ServiceHosts;
using ProtonVPN.Service.Settings;
using ProtonVPN.Service.SplitTunneling;
using ProtonVPN.Service.Vpn;
using ProtonVPN.Vpn.Common;
using ProtonVPN.Vpn.Connection;
using ProtonVPN.Vpn.Networks;
using ProtonVPN.Vpn.Networks.Adapters;
using Module = Autofac.Module;

namespace ProtonVPN.Service.Start
{
    internal class ServiceModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.Register(_ => new ConfigFactory().Config()).AsSelf().As<IConfiguration>().SingleInstance(); // REMOVE AS SELF
            builder.RegisterType<Bootstrapper>().SingleInstance();

            builder.RegisterType<Log4NetLoggerFactory>().As<ILoggerFactory>().SingleInstance();
            builder.Register(c => c.Resolve<ILoggerFactory>().Get(c.Resolve<IConfiguration>().ServiceLogDefaultFullFilePath))
                .As<ILogger>().SingleInstance();
            builder.RegisterType<LogCleaner>().SingleInstance();

            builder.RegisterType<JsonSerializerFactory>().As<ITextSerializerFactory>().SingleInstance();

            builder.RegisterType<SettingsHandler>().SingleInstance();
            builder.RegisterType<VpnConnectionHandler>().AsImplementedInterfaces().AsSelf().SingleInstance();

            builder.Register(_ => new ServiceRetryPolicy(2, TimeSpan.FromSeconds(1))).SingleInstance();
            builder.Register(c => new CalloutDriver(
                    new ReliableService(
                        c.Resolve<ServiceRetryPolicy>(),
                        new SafeService(
                            new LoggingService(
                                c.Resolve<ILogger>(),
                                new DriverService(
                                    c.Resolve<IConfiguration>().CalloutServiceName,
                                    c.Resolve<IOsProcesses>()))))))
                .AsImplementedInterfaces().AsSelf().SingleInstance();

            builder.RegisterType<SettingsFileStorage>().AsImplementedInterfaces().SingleInstance();

            builder.Register(c => c.Resolve<IConfiguration>().OpenVpn).As<OpenVpnConfig>()
                .SingleInstance();
            
            ProtonVPN.Vpn.Config.Module vpnModule = new();
            vpnModule.Load(builder);

            builder.Register(c => GetVpnConnection(c, vpnModule.GetVpnConnection(c))).As<IVpnConnection>().SingleInstance();
            builder.RegisterType<ServiceSettingsHostFactory>().As<ServiceHostFactory>().SingleInstance();
            builder.RegisterType<VpnConnectionHostFactory>().As<ServiceHostFactory>().SingleInstance();
            builder.Register(_ => new SerialTaskQueue()).As<ITaskQueue>().SingleInstance();
            builder.RegisterType<KillSwitch.KillSwitch>().AsImplementedInterfaces().AsSelf().SingleInstance();
            builder.RegisterType<VpnService>().SingleInstance();
            builder.RegisterType<ServiceSettings>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<Ipv6>().SingleInstance();
            builder.RegisterType<ObservableNetworkInterfaces>().SingleInstance();
            builder.RegisterType<Firewall.Firewall>().AsImplementedInterfaces().SingleInstance();

            builder.RegisterType<IpFilter>().AsImplementedInterfaces().AsSelf().SingleInstance();
            builder.RegisterType<IncludeModeApps>().AsSelf().SingleInstance();
            builder.RegisterType<IpLayer>().AsSelf().SingleInstance();
            builder.Register(c => new SplitTunnel(
                    c.Resolve<IServiceSettings>(),
                    c.Resolve<ISplitTunnelClient>(),
                    c.Resolve<IncludeModeApps>(),
                    c.Resolve<AppFilter>(),
                    c.Resolve<PermittedRemoteAddress>()))
                .AsImplementedInterfaces()
                .AsSelf()
                .SingleInstance();
            builder.RegisterType<SystemProcesses>().As<IOsProcesses>().SingleInstance();
            builder.Register(c =>
                    new SafeSystemNetworkInterfaces(
                        c.Resolve<ILogger>(),
                        new SystemNetworkInterfaces()))
                .As<INetworkInterfaces>().SingleInstance();
            builder.RegisterType<PermittedRemoteAddress>().AsSelf().SingleInstance();
            builder.RegisterType<AppFilter>().AsSelf().SingleInstance();
            builder.RegisterType<SplitTunnelNetworkFilters>().SingleInstance();
            builder.RegisterType<BestNetworkInterface>().SingleInstance();
            builder.RegisterType<SplitTunnelClient>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<WintunRegistryFixer>().SingleInstance();
            builder.Register(c => new NetworkSettings(c.Resolve<ILogger>(), c.Resolve<INetworkInterfaceLoader>(), c.Resolve<WintunRegistryFixer>()))
                .AsImplementedInterfaces()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<NetworkAdaptersLoader>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<NetworkAdapterManager>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<EventPublisher>().As<IEventPublisher>().SingleInstance();
            builder.RegisterType<DeviceInfoProvider>().As<IDeviceInfoProvider>().SingleInstance();
        }

        private IVpnConnection GetVpnConnection(IComponentContext c, IVpnConnection connection)
        {
            return new ObservableConnection(
                new FilteringStateWrapper(
                    new QueuingRequestsWrapper(
                        c.Resolve<ITaskQueue>(),
                        new Ipv6HandlingWrapper(
                            c.Resolve<IServiceSettings>(),
                            c.Resolve<IFirewall>(),
                            c.Resolve<ObservableNetworkInterfaces>(),
                            c.Resolve<Ipv6>(),
                            c.Resolve<ITaskQueue>(),
                            connection))));
        }
    }
}