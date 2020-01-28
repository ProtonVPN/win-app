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

using System;
using System.IO;
using System.ServiceModel;
using System.ServiceProcess;
using Autofac;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.CrashReporting;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.OS.Net.Http;
using ProtonVPN.Common.OS.Processes;
using ProtonVPN.Core.Api;
using ProtonVPN.Core.Api.Handlers;
using ProtonVPN.Core.Api.Handlers.TlsPinning;
using ProtonVPN.Core.OS.Net;
using ProtonVPN.Update;
using ProtonVPN.Update.Config;

namespace ProtonVPN.UpdateService
{
    internal class Bootstrapper
    {
        private IContainer _container;

        private T Resolve<T>() => _container.Resolve<T>();

        public void Initialize()
        {
            Configure();
            InitCrashReporting();
            Start();
        }

        private void Start()
        {
            var config = Resolve<Config>();
            var logger = Resolve<ILogger>();

            logger.Info($"= Booting ProtonVPN Update Service version: {config.AppVersion} os: {Environment.OSVersion.VersionString} {config.OsBits} bit =");

            Resolve<ServicePointConfiguration>().Apply();
            CreateLogFolder();
            Resolve<IAppUpdates>().Cleanup();
            Resolve<INotifyingAppUpdate>().StateChanged += (e, update) =>
            {
                if (update.Status == AppUpdateStatus.Updating)
                {
                    Resolve<UpdateService>().Stop();
                }
            };

            ServiceBase.Run(Resolve<UpdateService>());

            logger.Info("= ProtonVPN Update Service has exited =");
        }

        private void CreateLogFolder()
        {
            Directory.CreateDirectory(Resolve<Config>().UpdateServiceLogFolder);
        }

        private void InitCrashReporting()
        {
            CrashReports.Init(Resolve<Config>(), Resolve<ILogger>());
        }

        private void Configure()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new ConfigFactory().Config());
            builder.RegisterType<SystemProcesses>().As<IOsProcesses>().SingleInstance();
            builder.RegisterType<HttpClients>().As<IHttpClients>().SingleInstance();

            builder.Register(c => new NLogLoggingConfiguration(c.Resolve<Config>().UpdateServiceLogFolder, "updater"))
                .AsSelf().SingleInstance();
            builder.RegisterType<NLogLoggerFactory>().As<ILoggerFactory>().SingleInstance();
            builder.Register(c => c.Resolve<ILoggerFactory>().Logger())
                .As<ILogger>().SingleInstance();

            builder.Register(c => c.Resolve<UpdateServiceHostFactory>().Create()).As<ServiceHost>().SingleInstance();
            builder.RegisterType<ElevatedProcess>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<UpdateHandler>().SingleInstance();
            builder.RegisterType<UpdateServiceHostFactory>().SingleInstance();
            builder.RegisterType<UpdateService>().SingleInstance();
            builder.RegisterType<ServicePointConfiguration>().SingleInstance();

            builder.Register(c =>
                    new CachingReportClient(
                        new ReportClient(new Uri(c.Resolve<Config>().Urls.TlsReportUrl))))
                .As<IReportClient>()
                .SingleInstance();

            builder.Register(c =>
                new CertificateHandler(
                    c.Resolve<Config>().TlsPinningConfig,
                    c.Resolve<IReportClient>())).SingleInstance();

            builder.Register(c =>
                new LoggingHandler(
                        c.Resolve<ILogger>())
                    { InnerHandler = c.Resolve<CertificateHandler>() });

            builder.Register(c =>
                new RetryingHandler(
                        c.Resolve<Config>().ApiTimeout,
                        c.Resolve<Config>().ApiRetries,
                        (retryCount, response, context) => new SleepDurationProvider(response).Value())
                    { InnerHandler = c.Resolve<LoggingHandler>() }).SingleInstance();

            builder.Register(c => new DefaultAppUpdateConfig
                {
                    HttpClient = c.Resolve<IHttpClients>().Client(c.Resolve<RetryingHandler>()),
                    FeedUri = new Uri(c.Resolve<Config>().Urls.UpdateUrl),
                    UpdatesPath = c.Resolve<Config>().UpdatesPath,
                    CurrentVersion = Version.Parse(c.Resolve<Config>().AppVersion),
                    EarlyAccessCategoryName = "EarlyAccess",
                    MinProgressDuration = TimeSpan.FromSeconds(1.5)
                })
                .As<IAppUpdateConfig>().SingleInstance();

            new Update.Config.Module().Load(builder);

            _container = builder.Build();
        }
    }
}
