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
using ProtonVPN.Common.Installers.Extensions;
using ProtonVPN.Common.Legacy.OS.DeviceIds;
using ProtonVPN.Common.Legacy.OS.Net.Http;
using ProtonVPN.Common.Legacy.OS.Processes;
using ProtonVPN.Common.Legacy.Threading;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.EntityMapping.Installers;
using ProtonVPN.Files.Installers;
using ProtonVPN.IPv6.Contracts;
using ProtonVPN.IssueReporting.Installers;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.OperatingSystems.Network.Contracts;
using ProtonVPN.OperatingSystems.NRPT.Installers;
using ProtonVPN.OperatingSystems.Processes.Contracts;
using ProtonVPN.OperatingSystems.Processes.Installers;
using ProtonVPN.OperatingSystems.Registries.Installers;
using ProtonVPN.OperatingSystems.Services.Contracts;
using ProtonVPN.OperatingSystems.Services.Installers;
using ProtonVPN.ProcessCommunication.Installers;
using ProtonVPN.ProcessCommunication.Server.Installers;
using ProtonVPN.Serialization.Installers;
using ProtonVPN.Service.ControllerRetries;
using ProtonVPN.Service.Driver;
using ProtonVPN.Service.Firewall;
using ProtonVPN.Service.ProcessCommunication;
using ProtonVPN.Service.Settings;
using ProtonVPN.Service.SplitTunneling;
using ProtonVPN.Service.Update;
using ProtonVPN.Service.Vpn;
using ProtonVPN.Vpn.Common;
using ProtonVPN.Vpn.Connection;
using Module = Autofac.Module;

namespace ProtonVPN.Service.Start;

internal class ServiceModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        base.Load(builder);

        builder.RegisterType<Bootstrapper>().SingleInstance();
        builder.RegisterType<VpnController>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<UpdateController>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<ClientControllerSender>().AsImplementedInterfaces().SingleInstance();

        builder.Register(c => new CalloutDriver(c.Resolve<IServiceFactory>().Get(c.Resolve<IStaticConfiguration>().CalloutServiceName)))
            .AsImplementedInterfaces()
            .AsSelf()
            .SingleInstance();

        builder.RegisterType<SettingsFileStorage>().AsImplementedInterfaces().SingleInstance();

        ProtonVPN.Vpn.Config.Module vpnModule = new();
        vpnModule.Load(builder);

        builder.Register(c => GetVpnConnection(c, vpnModule.GetVpnConnection(c))).As<IVpnConnection>().SingleInstance();
        builder.Register(_ => new SerialTaskQueue()).As<ITaskQueue>().SingleInstance();
        builder.RegisterType<KillSwitch.KillSwitch>().AsImplementedInterfaces().AsSelf().SingleInstance();
        builder.RegisterType<VpnService>().SingleInstance();
        builder.RegisterType<ServiceSettings>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<Ipv6>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<ObservableNetworkInterfaces>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<Firewall.Firewall>().AsImplementedInterfaces().SingleInstance();

        builder.RegisterType<IpFilter>().AsImplementedInterfaces().AsSelf().SingleInstance();
        builder.RegisterType<IpLayer>().AsSelf().SingleInstance();
        builder.RegisterType<SplitTunnel>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<SystemProcesses>().As<IOsProcesses>().SingleInstance();
        builder.RegisterType<PermittedRemoteAddress>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<AppFilter>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<SplitTunnelNetworkFilters>().SingleInstance();
        builder.RegisterType<SplitTunnelClient>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<WintunRegistryFixer>().SingleInstance();
        builder.Register(c => new NetworkSettings(c.Resolve<ILogger>(), c.Resolve<INetworkInterfaceLoader>(), c.Resolve<INetworkUtilities>(), c.Resolve<WintunRegistryFixer>()))
            .AsImplementedInterfaces()
            .AsSelf()
            .SingleInstance();

        builder.RegisterType<HttpClients>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<FeedUrlProvider>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<CurrentAppVersionProvider>().AsImplementedInterfaces().SingleInstance();

        builder.RegisterType<DeviceIdCache>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<ControllerRetryManager>().AsImplementedInterfaces().SingleInstance();

        RegisterModules(builder);
    }

    private void RegisterModules(ContainerBuilder builder)
    {
        builder.RegisterAssemblyModule<EntityMappingModule>()
               .RegisterAssemblyModule<RegistriesModule>()
               .RegisterAssemblyModule<ProcessCommunicationModule>()
               .RegisterAssemblyModule<ServerProcessCommunicationModule>()
               .RegisterAssemblyModule<SerializationModule>()
               .RegisterAssemblyModule<FilesModule>()
               .RegisterAssemblyModule<IssueReportingModule>()
               .RegisterAssemblyModule<PowerEventsModule>()
               .RegisterAssemblyModule<ProcessesModule>()
               .RegisterAssemblyModule<ServicesModule>()
               .RegisterAssemblyModule<NameResolutionPolicyTableModule>();
    }

    private IVpnConnection GetVpnConnection(IComponentContext c, IVpnConnection connection)
    {
        return new ObservableConnection(
            new FilteringStateWrapper(
                new QueuingRequestsWrapper(
                    c.Resolve<ITaskQueue>(),
                    new Ipv6HandlingWrapper(
                        c.Resolve<IIpv6>(),
                        c.Resolve<ILogger>(),
                        c.Resolve<IFirewall>(),
                        c.Resolve<IServiceSettings>(),
                        c.Resolve<IFakeIPv6AddressGenerator>(),
                        c.Resolve<ICommandLineCaller>(),
                        c.Resolve<INetworkInterfaceLoader>(),
                        c.Resolve<ISystemNetworkInterfaces>(),
                        c.Resolve<IObservableNetworkInterfaces>(),
                        connection)))); 
    }
}