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

using System.Net.Http;
using Autofac;
using ProtonVPN.Api;
using ProtonVPN.Api.Contracts;
using ProtonVPN.Api.Handlers.Retries;
using ProtonVPN.Api.Handlers.TlsPinning;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Log4Net;
using ProtonVPN.Common.OS.Net;
using ProtonVPN.Common.OS.Net.Http;
using ProtonVPN.Common.OS.Net.NetworkInterface;
using ProtonVPN.Common.OS.Processes;
using ProtonVPN.Common.OS.Registry;
using ProtonVPN.Common.OS.Services;
using ProtonVPN.Common.Threading;
using ProtonVPN.Config.Url;
using ProtonVPN.Core.Auth;
using ProtonVPN.Core.Config;
using ProtonVPN.Core.Events;
using ProtonVPN.Core.Network;
using ProtonVPN.Core.OS;
using ProtonVPN.Core.OS.Net;
using ProtonVPN.Core.OS.Net.Dns;
using ProtonVPN.Core.OS.Net.DoH;
using ProtonVPN.Core.ReportAnIssue;
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Service;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Threading;
using ProtonVPN.Core.Update;
using ProtonVPN.Core.Vpn;
using ProtonVPN.Core.Windows;
using ProtonVPN.HumanVerification;
using ProtonVPN.HumanVerification.Contracts;
using ProtonVPN.Modals.ApiActions;
using ProtonVPN.Vpn;
using CoreDnsClient = ProtonVPN.Core.OS.Net.Dns.DnsClient;
using Module = Autofac.Module;

namespace ProtonVPN.Core.Ioc
{
    public class CoreModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);
            
            builder.RegisterType<HumanVerifier>().As<IHumanVerifier>().SingleInstance();
            builder.RegisterType<ServicePointConfiguration>().SingleInstance();
            builder.RegisterType<ServerManager>().SingleInstance();
            builder.RegisterType<ActiveUrls>().As<IActiveUrls>().SingleInstance();
            builder.RegisterType<ApiAppVersion>().As<IApiAppVersion>().SingleInstance();
            builder.RegisterType<SystemProcesses>().As<IOsProcesses>().SingleInstance();
            builder.Register(c =>
                    new SafeSystemNetworkInterfaces(c.Resolve<ILogger>(), new SystemNetworkInterfaces()))
                .As<INetworkInterfaces>().SingleInstance();
            builder.RegisterType<NetworkInterfaceLoader>().As<INetworkInterfaceLoader>().SingleInstance();
            builder.RegisterType<HttpClients>().As<IHttpClients>().SingleInstance();
            builder.Register(c => Schedulers.FromApplicationDispatcher()).As<IScheduler>().SingleInstance();

            builder.RegisterType<AppLanguageCache>().AsImplementedInterfaces().SingleInstance();
            
            builder.RegisterType<TokenClient>().As<ITokenClient>().SingleInstance();

            builder.RegisterType<ActionableFailureApiResultEventHandler>().SingleInstance();

            builder.RegisterType<Log4NetLoggerFactory>().As<ILoggerFactory>().SingleInstance();
            builder.Register(c => c.Resolve<ILoggerFactory>().Get(c.Resolve<IConfiguration>().AppLogDefaultFullFilePath))
                .As<ILogger>().SingleInstance();
            builder.RegisterType<LogCleaner>().SingleInstance();
            builder.RegisterType<SafeServiceAction>().As<ISafeServiceAction>().SingleInstance();
            builder.RegisterType<UpdateService>().AsSelf().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<UserValidator>().SingleInstance();
            builder.RegisterType<UserAuthenticator>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<NetworkClient>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<DnsClients>().As<IDnsClients>().SingleInstance();
            builder.Register(c =>
                    new SafeDnsClient(
                        new CoreDnsClient(
                            c.Resolve<IDnsClients>(),
                            c.Resolve<INetworkInterfaces>())))
                .As<IDnsClient>().SingleInstance();

            builder.Register(c =>
                    new CachingReportClient(
                        new ReportClient(c.Resolve<IActiveUrls>().TlsReportUrl.Uri)))
                .As<IReportClient>()
                .SingleInstance();

            builder.RegisterType<EventClient>().SingleInstance();
            builder.RegisterType<UserInfoHandler>().AsImplementedInterfaces().SingleInstance();

            builder.RegisterType<VpnProfileHandler>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<VpnCredentialProvider>().As<IVpnCredentialProvider>().SingleInstance();
            builder.Register(c => new EventTimer(
                    c.Resolve<EventClient>(),
                    c.Resolve<IConfiguration>().EventCheckInterval.RandomizedWithDeviation(0.2))) // REMOVE THIS CUSTOM REGISTRATION
                .AsSelf()
                .AsImplementedInterfaces()
                .SingleInstance();
            builder.Register(c => new SafeSystemProxy(c.Resolve<ILogger>(), new SystemProxy()))
                .AsImplementedInterfaces()
                .SingleInstance();
            builder.Register(c => new DohClients(
                c.Resolve<IConfiguration>().DoHProviders, // REMOVE THIS CUSTOM REGISTRATION
                c.Resolve<IConfiguration>().DohClientTimeout))
                .SingleInstance();
            builder.RegisterType<MainWindowState>().As<IMainWindowState>().SingleInstance();
            builder.RegisterType<SingleActionFactory>().As<ISingleActionFactory>().SingleInstance();
            builder.RegisterType<LastServerLoadTimeProvider>().As<ILastServerLoadTimeProvider>().SingleInstance();
            builder.RegisterType<ClientConfig>().AsImplementedInterfaces().SingleInstance();
            builder.Register(c =>
                    new NtpClient(c.Resolve<IConfiguration>().NtpServerUrl, c.Resolve<ILogger>())) // REMOVE THIS CUSTOM REGISTRATION
                .As<INtpClient>().SingleInstance();
            builder.RegisterType<ReportAnIssueFormDataProvider>().As<IReportAnIssueFormDataProvider>().SingleInstance();
            builder.RegisterType<SystemState>().As<ISystemState>().SingleInstance();
            builder.RegisterType<VpnAuthCertificateUpdater>().AsImplementedInterfaces().AsSelf().SingleInstance();
        }
    }
}