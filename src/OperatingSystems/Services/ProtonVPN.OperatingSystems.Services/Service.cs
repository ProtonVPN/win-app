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

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.ServiceProcess;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.OperatingSystemLogs;
using ProtonVPN.OperatingSystems.Processes.Contracts;
using ProtonVPN.OperatingSystems.Services.Contracts;

namespace ProtonVPN.OperatingSystems.Services;

public class Service : IService
{
    private const int ERROR_SERVICE_ALREADY_RUNNING = 1056;
    private readonly TimeSpan _timeoutInterval = TimeSpan.FromSeconds(10);

    private readonly ILogger _logger;
    private readonly ICommandLineCaller _commandLineCaller;

    public string Name { get; }

    public Service(string name, ILogger logger, ICommandLineCaller commandLineCaller)
    {
        Name = name;
        _logger = logger;
        _commandLineCaller = commandLineCaller;
    }

    public bool IsCreated()
    {
        return GetService() is not null;
    }

    private ServiceController? GetService()
    {
        ServiceController? serviceController = GetServices().FirstOrDefault(s => s.ServiceName == Name);
        if (serviceController is null)
        {
            _logger.Error<OperatingSystemLog>($"The service '{Name}' does not exist.");
        }
        return serviceController;
    }

    private IEnumerable<ServiceController> GetServices()
    {
        try
        {
            return ServiceController.GetServices();
        }
        catch (Exception ex)
        {
            _logger.Error<OperatingSystemLog>("Failed to fetch the services running in the Operating System.", ex);
            return new List<ServiceController>();
        }
    }

    public bool IsEnabled()
    {
        return GetService()?.StartType is not ServiceStartMode.Disabled;
    }

    public void Enable()
    {
        _logger.Info<OperatingSystemLog>($"Enabling the Windows service '{Name}'.");
        _commandLineCaller.ExecuteElevated($"/c sc config \"{Name}\" start= demand");
    }

    public bool IsRunning()
    {
        return GetService()?.Status == ServiceControllerStatus.Running;
    }

    public bool? IsStopped()
    {
        ServiceController? service = GetService();
        return service is null ? null : service.Status == ServiceControllerStatus.Stopped;
    }

    public bool Start()
    {
        _logger.Info<OperatingSystemLog>($"Starting the Windows service '{Name}'.");
        return ExecuteSafeServiceAction(StartServiceAndAwaitStartCompletion);
    }

    private void StartServiceAndAwaitStartCompletion(ServiceController service)
    {
        try
        {
            service.Start();
        }
        catch (Exception ex)
        {
            if (ex.InnerException is null || 
                ex.InnerException is not Win32Exception win32Exception || 
                win32Exception.NativeErrorCode != ERROR_SERVICE_ALREADY_RUNNING)
            {
                throw;
            }
        }
        service.WaitForStatus(ServiceControllerStatus.Running, _timeoutInterval);
    }

    private bool ExecuteSafeServiceAction(Action<ServiceController> action, [CallerMemberName] string sourceMemberName = "")
    {
        ServiceController? service = GetService();
        if (service is null)
        {
            return false;
        }

        try
        {
            action(service);
        }
        catch (Exception ex)
        {
            _logger.Warn<OperatingSystemLog>($"Failed to execute Service '{sourceMemberName}' action.", ex);
            return false;
        }

        return true;
    }

    public bool Stop()
    {
        _logger.Info<OperatingSystemLog>($"Stopping the Windows service '{Name}'.");
        return ExecuteSafeServiceAction(service => service.Stop());
    }

    public ServiceControllerStatus? GetStatus()
    {
        return GetService()?.Status;
    }
}