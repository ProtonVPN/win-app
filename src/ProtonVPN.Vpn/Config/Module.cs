﻿/*
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
using ProtonVPN.Common.Legacy.OS.Net;
using ProtonVPN.Common.Legacy.OS.Processes;
using ProtonVPN.Common.Legacy.OS.Services;
using ProtonVPN.Common.Legacy.Threading;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Configurations.Contracts.Entities;
using ProtonVPN.Crypto.Contracts;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Vpn.Common;
using ProtonVPN.Vpn.Connection;
using ProtonVPN.Vpn.ConnectionCertificates;
using ProtonVPN.Vpn.Gateways;
using ProtonVPN.Vpn.LocalAgent;
using ProtonVPN.Vpn.Management;
using ProtonVPN.Vpn.NetShield;
using ProtonVPN.Vpn.NetworkAdapters;
using ProtonVPN.Vpn.Networks;
using ProtonVPN.Vpn.OpenVpn;
using ProtonVPN.Vpn.PortMapping;
using ProtonVPN.Vpn.PortMapping.Serializers.Common;
using ProtonVPN.Vpn.PortMapping.UdpClients;
using ProtonVPN.Vpn.ServerValidation;
using ProtonVPN.Vpn.SplitTunnel;
using ProtonVPN.Vpn.SynchronizationEvent;
using ProtonVPN.Vpn.WireGuard;

namespace ProtonVPN.Vpn.Config;

public class Module
{
    public void Load(ContainerBuilder builder)
    {
        builder.RegisterType<ConnectionCertificateCache>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<ServerValidator>().As<IServerValidator>().SingleInstance();
        builder.RegisterType<GatewayCache>().As<IGatewayCache>().SingleInstance();
        builder.RegisterType<VpnEndpointScanner>().SingleInstance();
        builder.RegisterType<TcpPortScanner>().SingleInstance();
        builder.RegisterType<NetworkInterfaceLoader>().As<INetworkInterfaceLoader>().SingleInstance();
        builder.RegisterType<SplitTunnelRouting>().SingleInstance();
        builder.RegisterType<UdpPingClient>().SingleInstance();
        builder.RegisterType<WintunAdapter>().SingleInstance();
        builder.RegisterType<TapAdapter>().SingleInstance();
        builder.RegisterType<NetShieldStatisticEventManager>().AsImplementedInterfaces().SingleInstance();
        builder.Register(c =>
            {
                ILogger logger = c.Resolve<ILogger>();
                IStaticConfiguration staticConfig = c.Resolve<IStaticConfiguration>();

                return new OpenVpnProcess(
                    logger,
                    c.Resolve<IOsProcesses>(),
                    new OpenVpnExitEvent(logger,
                        new SystemSynchronizationEvents(logger),
                        staticConfig.OpenVpn.ExitEventName),
                    staticConfig);
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
        ITaskQueue taskQueue = c.Resolve<ITaskQueue>();
        TcpPortScanner tcpPortScanner = c.Resolve<TcpPortScanner>();
        tcpPortScanner.Config(c.Resolve<IStaticConfiguration>().OpenVpn.StaticKey);
        IEndpointScanner endpointScanner = c.Resolve<VpnEndpointScanner>();
        VpnEndpointCandidates candidates = new();
        IIssueReporter issueReporter = c.Resolve<IIssueReporter>();
        IPortMappingProtocolClient portMappingProtocolClient = c.Resolve<IPortMappingProtocolClient>();
        IServerValidator serverValidator = c.Resolve<IServerValidator>();

        return new LoggingWrapper(
            logger,
                new ReconnectingWrapper(
                    logger,
                    candidates,
                    serverValidator,
                    endpointScanner,
                    new HandlingRequestsWrapper(
                        logger,
                        taskQueue,
                        new ServerAuthenticatorWrapper(
                            serverValidator,
                            new BestPortWrapper(
                                logger,
                                taskQueue,
                                endpointScanner,
                                new NetworkAdapterStatusWrapper(
                                    logger,
                                    issueReporter,
                                    networkAdapterManager,
                                    networkInterfaceLoader,
                                    c.Resolve<WintunAdapter>(),
                                    c.Resolve<TapAdapter>(),
                                    new QueueingEventsWrapper(
                                        taskQueue,
                                        new PortForwardingWrapper(
                                            logger,
                                            portMappingProtocolClient,
                                            new VpnProtocolWrapper(GetOpenVpnConnection(c), GetWireguardConnection(c))))))))));
    }

    private ISingleVpnConnection GetWireguardConnection(IComponentContext c)
    {
        ILogger logger = c.Resolve<ILogger>();
        IStaticConfiguration staticConfig = c.Resolve<IStaticConfiguration>();
        IConfiguration config = c.Resolve<IConfiguration>();
        IGatewayCache gatewayCache = c.Resolve<IGatewayCache>();
        INetShieldStatisticEventManager netShieldStatisticEventManager = c.Resolve<INetShieldStatisticEventManager>();
        IX25519KeyGenerator x25519KeyGenerator = c.Resolve<IX25519KeyGenerator>();
        IConnectionCertificateCache connectionCertificateCache = c.Resolve<IConnectionCertificateCache>();

        return new LocalAgentWrapper(logger, new EventReceiver(logger, netShieldStatisticEventManager), c.Resolve<SplitTunnelRouting>(),
            gatewayCache,
            connectionCertificateCache,
            new WireGuardConnection(logger, config, gatewayCache,
                new WireGuardService(logger, staticConfig, new SafeService(
                    new LoggingService(logger,
                        new SystemService(staticConfig.WireGuard.ServiceName, c.Resolve<IOsProcesses>())))),
                new TrafficManager(staticConfig.WireGuard.ConfigFileName, logger),
                new StatusManager(logger, staticConfig.WireGuard.LogFilePath),
                x25519KeyGenerator));
    }

    private ISingleVpnConnection GetOpenVpnConnection(IComponentContext c)
    {
        ILogger logger = c.Resolve<ILogger>();
        IOpenVpnConfigurations openVpnConfig = c.Resolve<IStaticConfiguration>().OpenVpn;
        IGatewayCache gatewayCache = c.Resolve<IGatewayCache>();
        INetShieldStatisticEventManager netShieldStatisticEventManager = c.Resolve<INetShieldStatisticEventManager>();
        IConnectionCertificateCache connectionCertificateCache = c.Resolve<IConnectionCertificateCache>();

        return new LocalAgentWrapper(logger, new EventReceiver(logger, netShieldStatisticEventManager), c.Resolve<SplitTunnelRouting>(),
            gatewayCache,
            connectionCertificateCache,
            new OpenVpnConnection(
                logger,
                c.Resolve<IStaticConfiguration>(),
                c.Resolve<INetworkInterfaceLoader>(),
                c.Resolve<OpenVpnProcess>(),
                c.Resolve<IRandomStringGenerator>(),
                new ManagementClient(
                    logger,
                    gatewayCache,
                    new ConcurrentManagementChannel(
                        new TcpManagementChannel(
                            logger,
                            openVpnConfig.ManagementHost)))));
    }
}