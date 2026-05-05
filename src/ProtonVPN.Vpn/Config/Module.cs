/*
 * Copyright (c) 2026 Proton AG
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
using ProtonVPN.Vpn.Connection;
using ProtonVPN.Vpn.Gateways;
using ProtonVPN.Vpn.LocalAgent;
using ProtonVPN.Vpn.Management;
using ProtonVPN.Vpn.NetworkAdapters;
using ProtonVPN.Vpn.NRPT;
using ProtonVPN.Vpn.OpenVpn;
using ProtonVPN.Vpn.OpenVpn.DnsServers;
using ProtonVPN.Vpn.PortMapping;
using ProtonVPN.Vpn.PortMapping.Serializers.Common;
using ProtonVPN.Vpn.PortMapping.UdpClients;
using ProtonVPN.Vpn.PortScanning;
using ProtonVPN.Vpn.ProTun;
using ProtonVPN.Vpn.ServerValidation;
using ProtonVPN.Vpn.SplitTunnel;
using ProtonVPN.Vpn.SynchronizationEvent;
using ProtonVPN.Vpn.Wintun;
using ProtonVPN.Vpn.WireGuard;

namespace ProtonVPN.Vpn.Config;

public class Module
{
    public void Load(ContainerBuilder builder)
    {
        builder.RegisterType<LocalAgentTlsCredentialsCache>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<ServerValidator>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<GatewayCache>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<DnsServerCache>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<OpenVpnDnsServersCreator>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<VpnEndpointScanner>().SingleInstance();
        builder.RegisterType<TcpPortScanner>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<SplitTunnelRouting>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<UdpPingClient>().SingleInstance();
        builder.RegisterType<WintunAdapter>().SingleInstance();
        builder.RegisterType<TapAdapter>().SingleInstance();
        builder.RegisterType<VpnEndpointCandidates>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<VpnEndpointScanner>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<NrptWrapper>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<NtTrafficManager>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<WireGuardStateMonitor>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<WintunTrafficManager>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<WireGuardService>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<WireGuardConfigGenerator>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<LocalAgent.LocalAgent>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<LocalAgentEventReceiver>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<ProTunConnection>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<WireGuardConnection>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<OpenVpnConnection>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<ManagementClient>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<OpenVpnProcess>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<OpenVpnExitEvent>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<SystemSynchronizationEvents>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<TcpManagementChannel>().As<ITcpManagementChannel>().SingleInstance();
        builder.RegisterType<ConcurrentManagementChannel>().As<IConcurrentManagementChannel>().SingleInstance();
        builder.RegisterType<WintunAdapter>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<TapAdapter>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<MessagingManagementChannel>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<WireGuardServerRouteManager>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<WintunRegistryFixer>().AsImplementedInterfaces().SingleInstance();

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
}