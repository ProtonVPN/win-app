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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using Autofac;
using ProtonVPN.Api.Installers;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.Installers.Extensions;
using ProtonVPN.Common.OS.Architecture;
using ProtonVPN.Common.OS.Processes;
using ProtonVPN.Common.Vpn;
using ProtonVPN.IssueReporting.Installers;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppServiceLogs;
using ProtonVPN.Logging.Installers;
using ProtonVPN.Native.PInvoke;
using ProtonVPN.Service.Config;
using ProtonVPN.Service.Settings;
using ProtonVPN.Service.Vpn;
using ProtonVPN.Update.Installers;
using ProtonVPN.Vpn.Common;
using ProtonVPN.Vpn.Networks;
using ProtonVPN.Vpn.OpenVpn;

namespace ProtonVPN.Service.Start
{
    internal class Bootstrapper
    {
        private IContainer _container;
        private T Resolve<T>() => _container.Resolve<T>();

        public Bootstrapper()
        {
            IssueReportingInitializer.Run();
        }

        public void Initialize()
        {
            SetDllDirectories();
            Configure();
            Start();
        }

        private void Configure()
        {
            IConfiguration config = new ConfigFactory().Config();
            new ConfigDirectories(config).Prepare();

            ContainerBuilder builder = new();
            builder.RegisterLoggerConfiguration(c => c.ServiceLogDefaultFullFilePath)
                   .RegisterModule<ServiceModule>()
                   .RegisterModule<ApiModule>()
                   .RegisterAssemblyModule<LoggingModule>()
                   .RegisterAssemblyModule<UpdateModule>();
            _container = builder.Build();
        }

        private void Start()
        {
            IConfiguration config = Resolve<IConfiguration>();
            ILogger logger = Resolve<ILogger>();

            logger.Info<AppServiceStartLog>(
                $"= Booting ProtonVPN Service version: {config.AppVersion} os: {Environment.OSVersion.VersionString} {OsArchitecture.Value} =");

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            RegisterEvents();

            Resolve<ILogCleaner>().Clean(config.ServiceLogFolder, 10);
            FixNetworkAdapters();

            VpnService vpnService = Resolve<VpnService>();
            ServiceBase.Run(vpnService);
            vpnService.CancellationToken.WaitHandle.WaitOne();

            logger.Info<AppServiceStopLog>("= ProtonVPN Service has exited =");
        }

        private void FixNetworkAdapters()
        {
            INetworkAdapterManager networkAdapterManager = Resolve<INetworkAdapterManager>();
            networkAdapterManager.DisableDuplicatedWireGuardAdapters();
            networkAdapterManager.EnableOpenVpnAdapters();
        }

        private void RegisterEvents()
        {
            Resolve<IVpnConnection>().StateChanged += (_, e) =>
            {
                VpnState state = e.Data;
                IEnumerable<IVpnStateAware> instances = Resolve<IEnumerable<IVpnStateAware>>();
                foreach (IVpnStateAware instance in instances)
                {
                    switch (state.Status)
                    {
                        case VpnStatus.Connecting:
                        case VpnStatus.Reconnecting:
                            instance.OnVpnConnecting(state);
                            break;
                        case VpnStatus.Connected:
                            instance.OnVpnConnected(state);
                            break;
                        case VpnStatus.Disconnecting:
                        case VpnStatus.Disconnected:
                            instance.OnVpnDisconnected(state);
                            break;
                    }
                }
            };

            Resolve<IServiceSettings>().SettingsChanged += (_, e) =>
            {
                IEnumerable<IServiceSettingsAware> instances = Resolve<IEnumerable<IServiceSettingsAware>>();
                foreach (IServiceSettingsAware instance in instances)
                {
                    instance.OnServiceSettingsChanged(e);
                }
            };
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            IConfiguration config = Resolve<IConfiguration>();
            IOsProcesses processes = Resolve<IOsProcesses>();
            Resolve<IVpnConnection>().Disconnect();
            Resolve<OpenVpnProcess>().Stop();
            processes.KillProcesses(config.AppName);
        }

        private static void SetDllDirectories()
        {
            Kernel32.SetDefaultDllDirectories(Kernel32.SetDefaultDllDirectoriesFlags.LOAD_LIBRARY_SEARCH_DEFAULT_DIRS);
        }
    }
}