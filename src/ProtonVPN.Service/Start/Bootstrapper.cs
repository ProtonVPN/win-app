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
using System.IO;
using System.ServiceProcess;
using Autofac;
using ProtonVPN.Api.Installers;
using ProtonVPN.Common.Installers.Extensions;
using ProtonVPN.Common.Legacy.OS.Processes;
using ProtonVPN.Common.Legacy.Vpn;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Configurations.Installers;
using ProtonVPN.Crypto.Installers;
using ProtonVPN.IPv6.Installers;
using ProtonVPN.IssueReporting.Installers;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppServiceLogs;
using ProtonVPN.Logging.Installers;
using ProtonVPN.Native.PInvoke;
using ProtonVPN.OperatingSystems.Network.Installers;
using ProtonVPN.Service.Settings;
using ProtonVPN.Service.Vpn;
using ProtonVPN.Update.Installers;
using ProtonVPN.Vpn.Common;
using ProtonVPN.Vpn.OpenVpn;

namespace ProtonVPN.Service.Start;

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
        PrepareDirectories();
        Start();
    }

    private void Configure()
    {
        ContainerBuilder builder = new();
        builder.RegisterLoggerConfiguration(c => c.ServiceLogsFilePath)
               .RegisterModule<CryptoModule>()
               .RegisterModule<ServiceModule>()
               .RegisterModule<ApiModule>()
               .RegisterModule<NetworkModule>()
               .RegisterModule<ConfigurationsModule>()
               .RegisterAssemblyModule<LoggingModule>()
               .RegisterAssemblyModule<IPv6Module>()
               .RegisterAssemblyModule<UpdateModule>();
        _container = builder.Build();
    } 

    private void PrepareDirectories()
    {
        IStaticConfiguration staticConfig = Resolve<IStaticConfiguration>();

        Directory.CreateDirectory(staticConfig.ServiceLogsFolder);
        Directory.CreateDirectory(staticConfig.OpenVpn.TlsExportCertFolder);
    }

    private void Start()
    {
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

        RegisterEvents();

        Resolve<ILogCleaner>().Clean(Resolve<IStaticConfiguration>().ServiceLogsFolder, 10);

        VpnService vpnService = Resolve<VpnService>();
        ServiceBase.Run(vpnService);
        vpnService.CancellationToken.WaitHandle.WaitOne();

        Resolve<ILogger>().Info<AppServiceStopLog>("=== Proton VPN Service has exited ===");
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
                    case VpnStatus.AssigningIp:
                        instance.AssigningIp(state);
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

            IssueReportingInitializer.SetEnabled(e.IsShareCrashReportsEnabled);
        };
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        IStaticConfiguration config = Resolve<IStaticConfiguration>();
        IOsProcesses processes = Resolve<IOsProcesses>();
        Resolve<IVpnConnection>().Disconnect();
        Resolve<OpenVpnProcess>().Stop();
        processes.KillProcesses(config.ClientName);
    }

    private static void SetDllDirectories()
    {
        Kernel32.SetDefaultDllDirectories(Kernel32.SetDefaultDllDirectoriesFlags.LOAD_LIBRARY_SEARCH_DEFAULT_DIRS);
    }
}