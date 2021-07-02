/*
 * Copyright (c) 2020 Proton Technologies AG
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
using ProtonVPN.Common.Threading;
using ProtonVPN.Vpn.Common;
using ProtonVPN.Vpn.Connection;
using ProtonVPN.Vpn.Management;
using ProtonVPN.Vpn.OpenVpn;
using ProtonVPN.Vpn.SynchronizationEvent;

namespace ProtonVPN.Vpn.Config
{
    public class Module
    {
        public void Load(ContainerBuilder builder)
        {
            builder.RegisterType<EndpointScanner>().As<IEndpointScanner>().SingleInstance();
            builder.RegisterType<PingableOpenVpnPort>().SingleInstance();
            builder.RegisterType<NetworkInterfaceLoader>().As<INetworkInterfaceLoader>().SingleInstance();
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

        public IVpnConnection VpnConnection(IComponentContext c)
        {
            ILogger logger = c.Resolve<ILogger>();
            OpenVpnConfig config = c.Resolve<OpenVpnConfig>();
            ITaskQueue taskQueue = c.Resolve<ITaskQueue>();
            PingableOpenVpnPort pingableOpenVpnPort = c.Resolve<PingableOpenVpnPort>();
            pingableOpenVpnPort.Config(config.OpenVpnStaticKey);
            IEndpointScanner endpointScanner = c.Resolve<IEndpointScanner>();
            VpnEndpointCandidates candidates = new();

            OpenVpnConnection vpnConnection = new(
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
                                vpnConnection)))));
        }
    }
}