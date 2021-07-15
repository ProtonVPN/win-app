/*
 * Copyright (c) 2021 Proton Technologies AG
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
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.OS.Net;
using ProtonVPN.Common.OS.Processes;
using ProtonVPN.Common.OS.Services;
using ProtonVPN.Common.Threading;
using ProtonVPN.Crypto;
using ProtonVPN.Vpn.Common;
using ProtonVPN.Vpn.Connection;
using ProtonVPN.Vpn.LocalAgent;
using ProtonVPN.Vpn.Management;
using ProtonVPN.Vpn.OpenVpn;
using ProtonVPN.Vpn.SplitTunnel;
using ProtonVPN.Vpn.SynchronizationEvent;
using ProtonVPN.Vpn.WireGuard;

namespace ProtonVPN.Vpn.Config
{
    public class Module
    {
        public void Load(ContainerBuilder builder)
        {
            builder.RegisterType<OpenVpnEndpointScanner>().As<IEndpointScanner>().SingleInstance();
            builder.RegisterType<PingableOpenVpnPort>().SingleInstance();
            builder.RegisterType<NetworkInterfaceLoader>().As<INetworkInterfaceLoader>().SingleInstance();
            builder.RegisterType<SplitTunnelRouting>().SingleInstance();
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
        }

        public IVpnConnection GetOpenVpnConnection(IComponentContext c)
        {
            ILogger logger = c.Resolve<ILogger>();
            OpenVpnConfig config = c.Resolve<OpenVpnConfig>();
            ITaskQueue taskQueue = c.Resolve<ITaskQueue>();
            PingableOpenVpnPort pingableOpenVpnPort = c.Resolve<PingableOpenVpnPort>();
            pingableOpenVpnPort.Config(config.OpenVpnStaticKey);
            IEndpointScanner endpointScanner = c.Resolve<IEndpointScanner>();
            VpnEndpointCandidates candidates = new();

            ISingleVpnConnection openVpnConnection = new OpenVpnConnection(
                logger,
                c.Resolve<INetworkInterfaceLoader>(),
                c.Resolve<OpenVpnProcess>(),
                new ManagementClient(
                    logger,
                    new ConcurrentManagementChannel(
                        new TcpManagementChannel(
                            logger,
                            config.ManagementHost))));

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
                            new QueueingEventsWrapper(
                                taskQueue,
                                openVpnConnection)))));
        }

        public IVpnConnection GetWireguardConnection(IComponentContext c)
        {
            ILogger logger = c.Resolve<ILogger>();
            ITaskQueue taskQueue = c.Resolve<ITaskQueue>();
            ProtonVPN.Common.Configuration.Config config = c.Resolve<ProtonVPN.Common.Configuration.Config>();

            ISingleVpnConnection wireGuardVpnConnection = new WireGuardConnection(
                logger,
                config,
                new WireGuardService(logger, config, new SafeService(
                        new LoggingService(logger,
                            new SystemService(config.WireGuard.ServiceName, c.Resolve<IOsProcesses>())))),
                new TrafficManager(config.WireGuard.PipeName),
                new StatusManager(logger, config.WireGuard.LogFilePath),
                new X25519KeyGenerator());

            return new LoggingWrapper(logger,
                new ReconnectingWrapper(logger, new VpnEndpointCandidates(), new WireGuardEndpointScanner(),
                    new HandlingRequestsWrapper(logger, taskQueue,
                        new QueueingEventsWrapper(taskQueue,
                            new LocalAgentWrapper(logger, new EventReceiver(logger), c.Resolve<SplitTunnelRouting>(),
                                wireGuardVpnConnection)))));
        }
    }
}