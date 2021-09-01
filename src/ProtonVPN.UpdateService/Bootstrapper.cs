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

using System;
using System.IO;
using System.ServiceModel;
using System.ServiceProcess;
using Autofac;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.CrashReporting;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Log4Net;
using ProtonVPN.Common.OS.Event;
using ProtonVPN.Common.OS.Net.Http;
using ProtonVPN.Common.OS.Processes;
using ProtonVPN.Core.Api;
using ProtonVPN.Core.Api.Handlers;
using ProtonVPN.Core.Api.Handlers.TlsPinning;
using ProtonVPN.Core.OS.Net;
using ProtonVPN.Update;
using ProtonVPN.Update.Config;
using Sentry;
using Sentry.Protocol;

namespace ProtonVPN.UpdateService
{
    internal class Bootstrapper
    {
        private IContainer _container;

        private T Resolve<T>() => _container.Resolve<T>();

        public void Initialize()
        {
            Configure();
            InitCrashLogging();
            InitCrashReporting();
            Start();
        }

        public void StartUpdate()
        {
            Configure();
            InitCrashLogging();
            InitCrashReporting();

            try
            {
                string[] content = System.IO.File.ReadAllText(Resolve<Config>().UpdateFilePath).Split('\n');
                if (content.Length != 2)
                {
                    return;
                }

                StartElevatedProcess(content[0], content[1]);
            }
            catch (Exception e)
            {
                SentrySdk.WithScope(scope =>
                {
                     scope.Level = SentryLevel.Error;
                     scope.SetTag("captured_in", "UpdateService_Bootstrapper_StartUpdate");
                     SentrySdk.CaptureException(e);
                });
            }
        }

        private void StartElevatedProcess(string path, string arguments)
        {
            Resolve<IOsProcesses>().ElevatedProcess(path, arguments).Start();
        }

        private void InitCrashLogging()
        {
            var logging = Resolve<UnhandledExceptionLogging>();
            logging.CaptureUnhandledExceptions();
            logging.CaptureTaskExceptions();
        }

        private void Start()
        {
            var config = Resolve<Config>();
            var logger = Resolve<ILogger>();

            logger.Info($"= Booting ProtonVPN Update Service version: {config.AppVersion} os: {Environment.OSVersion.VersionString} {config.OsBits} bit =");

            Resolve<ServicePointConfiguration>().Apply();
            CreateLogFolder();
            Resolve<IAppUpdates>().Cleanup();
            Resolve<LogCleaner>().Clean(config.UpdateServiceLogFolder, 10);

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
            
            builder.RegisterType<Log4NetLoggerFactory>().As<ILoggerFactory>().SingleInstance();
            builder.Register(c => c.Resolve<ILoggerFactory>().Get(c.Resolve<Common.Configuration.Config>().UpdateServiceLogDefaultFullFilePath))
                .As<ILogger>().SingleInstance();
            builder.RegisterType<LogCleaner>().SingleInstance();

            builder.Register(c => c.Resolve<UpdateServiceHostFactory>().Create()).As<ServiceHost>().SingleInstance();
            builder.RegisterType<EventBasedLaunchableFile>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<UpdateHandler>().SingleInstance();
            builder.RegisterType<UpdateServiceHostFactory>().SingleInstance();
            builder.RegisterType<UpdateService>().SingleInstance();
            builder.RegisterType<ServicePointConfiguration>().SingleInstance();
            builder.RegisterType<SystemEventLog>().SingleInstance();
            builder.RegisterType<UnhandledExceptionLogging>().SingleInstance();
            builder.RegisterType<ApiAppVersion>().As<IApiAppVersion>().SingleInstance();

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
                        c.Resolve<Config>().ApiUploadTimeout,
                        c.Resolve<Config>().ApiRetries,
                        (retryCount, response, context) => new SleepDurationProvider(response).Value())
                    { InnerHandler = c.Resolve<LoggingHandler>() }).SingleInstance();

            builder.Register(c => new DefaultAppUpdateConfig
                {
                    HttpClient = c.Resolve<IHttpClients>().Client(c.Resolve<RetryingHandler>(), c.Resolve<IApiAppVersion>().UserAgent()),
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
