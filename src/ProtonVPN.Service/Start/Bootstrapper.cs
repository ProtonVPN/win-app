/*
 * Copyright (c) 2025 Proton AG
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
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Configurations.Installers;
using ProtonVPN.Crypto.Installers;
using ProtonVPN.IPv6.Installers;
using ProtonVPN.IssueReporting.Static;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppServiceLogs;
using ProtonVPN.Logging.Installers;
using ProtonVPN.Native.PInvoke;
using ProtonVPN.OperatingSystems.Network.Installers;
using ProtonVPN.Service.Settings;
using ProtonVPN.Service.StateMachine;
using ProtonVPN.Update.Installers;
using ProtonVPN.Vpn.OpenVpn;

namespace ProtonVPN.Service.Start;

internal class Bootstrapper
{
    private readonly ServiceGlobalExceptionHandler _globalExceptionHandler = new();

    private IContainer _container;
    private T Resolve<T>() => _container.Resolve<T>();

    public Bootstrapper()
    {
        _globalExceptionHandler.Initialize();
        _globalExceptionHandler.OnFatalException += OnFatalException;
        
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

        _globalExceptionHandler.SetLogger(Resolve<ILogger>());
    }

    private void PrepareDirectories()
    {
        IStaticConfiguration staticConfig = Resolve<IStaticConfiguration>();

        Directory.CreateDirectory(staticConfig.ServiceLogsFolder);
        Directory.CreateDirectory(staticConfig.OpenVpn.TlsExportCertFolder);
    }

    private void Start()
    {
        RegisterEvents();

        Resolve<ILogCleaner>().Clean(Resolve<IStaticConfiguration>().ServiceLogsFolder, 10);

        VpnService vpnService = Resolve<VpnService>();
        ServiceBase.Run(vpnService);
        vpnService.CancellationToken.WaitHandle.WaitOne();

        Resolve<ILogger>().Info<AppServiceStopLog>("=== Proton VPN Service has exited ===");
    }

    private void RegisterEvents()
    {
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

    private void OnFatalException(Exception exception)
    {
        if (_container is null)
        {
            return; // Fatal exception occurred before DI was built; nothing to clean up.
        }

        TryCleanup<ILogger>(logger =>
            logger.Info<AppServiceLog>("Fatal exception caught, attempting to clean up before crash"));
        TryCleanup<IVpnConnectionStateMachine>(sm => sm.Disconnect());
        TryCleanup<IOpenVpnProcess>(p => p.Stop());
        TryCleanup<IOsProcesses>(p => p.KillProcesses(Resolve<IStaticConfiguration>().ClientName));
    }

    private void TryCleanup<T>(Action<T> action)
    {
        try
        {
            action(Resolve<T>());
        }
        catch
        {
            // Best-effort cleanup during a fatal crash; swallow so remaining steps still run.
        }
    }

    private static void SetDllDirectories()
    {
        Kernel32.SetDefaultDllDirectories(Kernel32.SetDefaultDllDirectoriesFlags.LOAD_LIBRARY_SEARCH_DEFAULT_DIRS);
    }
}