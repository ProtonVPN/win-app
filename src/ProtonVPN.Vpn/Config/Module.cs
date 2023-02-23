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
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.Events;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.OS.Net;
using ProtonVPN.Common.OS.Processes;
using ProtonVPN.Common.OS.Services;
using ProtonVPN.Common.Threading;
using ProtonVPN.Crypto;
using ProtonVPN.Vpn.Common;
using ProtonVPN.Vpn.Connection;
using ProtonVPN.Vpn.Gateways;
using ProtonVPN.Vpn.LocalAgent;
using ProtonVPN.Vpn.Management;
using ProtonVPN.Vpn.Networks;
using ProtonVPN.Vpn.OpenVpn;
using ProtonVPN.Vpn.PortMapping;
using ProtonVPN.Vpn.PortMapping.Serializers.Common;
using ProtonVPN.Vpn.PortMapping.UdpClients;
using ProtonVPN.Vpn.SplitTunnel;
using ProtonVPN.Vpn.SynchronizationEvent;
using ProtonVPN.Vpn.WireGuard;

namespace ProtonVPN.Vpn.Config
{
    public class Module
    {
        public void Load(ContainerBuilder builder)
        {
            builder.RegisterType<GatewayCache>().As<IGatewayCache>().SingleInstance();
            builder.RegisterType<VpnEndpointScanner>().SingleInstance();
            builder.RegisterType<TcpPortScanner>().SingleInstance();
            builder.RegisterType<NetworkInterfaceLoader>().As<INetworkInterfaceLoader>().SingleInstance();
            builder.RegisterType<SplitTunnelRouting>().SingleInstance();
            builder.RegisterType<UdpPingClient>().SingleInstance();
            builder.Register(c =>
                {
                    ILogger logger = c.Resolve<ILogger>();
                    OpenVpnConfig config = c.Resolve<OpenVpnConfig>();

                    return new OpenVpnProcess(
                        logger,
                        c.Resolve<IOsProcesses>(),
                        new OpenVpnExitEvent(logger,
                            new SystemSynchronizationEvents(logger),
                            config.ExitEventName),
                        config);
                }
            ).SingleInstance();

            RegisterPortMapping(builder);
        }

        private void RegisterPortMapping(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(typeof(IMessageSerializer).Assembly)
                .Where(t => typeof(IMessageSerializer).IsAssignableFrom(t))
                .AsImplementedInterfaces()
                .SingleInstance();
            builder.RegisterType<MessageSerializerFactory>().As<IMessageSerializerFactory>().SingleInstance();
            builder.RegisterType<MessageSerializerProxy>().As<IMessageSerializerProxy>().SingleInstance();
            builder.RegisterType<UdpClientWrapper>().As<IUdpClientWrapper>().SingleInstance();
            builder.RegisterType<PortMappingProtocolClient>().As<IPortMappingProtocolClient>().SingleInstance();
        }

        public IVpnConnection GetVpnConnection(IComponentContext c)
        {
            ILogger logger = c.Resolve<ILogger>();
            INetworkAdapterManager networkAdapterManager = c.Resolve<INetworkAdapterManager>();
            INetworkInterfaceLoader networkInterfaceLoader = c.Resolve<INetworkInterfaceLoader>();
            OpenVpnConfig config = c.Resolve<OpenVpnConfig>();
            ITaskQueue taskQueue = c.Resolve<ITaskQueue>();
            TcpPortScanner tcpPortScanner = c.Resolve<TcpPortScanner>();
            tcpPortScanner.Config(config.OpenVpnStaticKey);
            IEndpointScanner endpointScanner = c.Resolve<VpnEndpointScanner>();
            VpnEndpointCandidates candidates = new();
            IEventPublisher eventPublisher = c.Resolve<IEventPublisher>();
            IPortMappingProtocolClient portMappingProtocolClient = c.Resolve<IPortMappingProtocolClient>();

            return new LoggingWrapper(
                logger,
                    new ReconnectingWrapper(
                        logger,
                        candidates,
                        endpointScanner,
                        new HandlingRequestsWrapper(
                            logger,
                            taskQueue,
                            new BestPortWrapper(
                                logger,
                                taskQueue,
                                endpointScanner,
                                new NetworkAdapterStatusWrapper(
                                    logger,
                                    eventPublisher,
                                    networkAdapterManager,
                                    networkInterfaceLoader,
                                    new QueueingEventsWrapper(
                                        taskQueue,
                                        new PortForwardingWrapper(
                                            logger,
                                            portMappingProtocolClient,
                                            new VpnProtocolWrapper(GetOpenVpnConnection(c), GetWireguardConnection(c)))))))));
        }

        private ISingleVpnConnection GetWireguardConnection(IComponentContext c)
        {
            ILogger logger = c.Resolve<ILogger>();
            IConfiguration config = c.Resolve<IConfiguration>();
            IGatewayCache gatewayCache = c.Resolve<IGatewayCache>();

            return new LocalAgentWrapper(logger, new EventReceiver(logger), c.Resolve<SplitTunnelRouting>(),
                gatewayCache,
                new WireGuardConnection(logger, config, gatewayCache,
                    new WireGuardService(logger, config, new SafeService(
                        new LoggingService(logger,
                            new SystemService(config.WireGuard.ServiceName, c.Resolve<IOsProcesses>())))),
                    new TrafficManager(config.WireGuard.ConfigFileName, logger),
                    new StatusManager(logger, config.WireGuard.LogFilePath),
                    new X25519KeyGenerator()));
        }

        private ISingleVpnConnection GetOpenVpnConnection(IComponentContext c)
        {
            ILogger logger = c.Resolve<ILogger>();
            OpenVpnConfig config = c.Resolve<OpenVpnConfig>();
            IGatewayCache gatewayCache = c.Resolve<IGatewayCache>();

            return new LocalAgentWrapper(logger, new EventReceiver(logger), c.Resolve<SplitTunnelRouting>(),
                gatewayCache,
                new OpenVpnConnection(
                    logger,
                    c.Resolve<IConfiguration>(),
                    c.Resolve<INetworkInterfaceLoader>(),
                    c.Resolve<OpenVpnProcess>(),
                    new ManagementClient(
                        logger,
                        gatewayCache,
                        new ConcurrentManagementChannel(
                            new TcpManagementChannel(
                                logger,
                                config.ManagementHost)))));
        }
    }
}