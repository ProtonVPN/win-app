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
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using ProtonVPN.Common.Abstract;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Helpers;
using ProtonVPN.Common.OS.Processes;

namespace ProtonVPN.Common.OS.Services
{
    public abstract class Service : IService
    {
        private readonly TimeSpan _waitForServiceStatusDuration = TimeSpan.FromSeconds(30);
        private readonly TimeSpan _waitForServiceEnableDuration = TimeSpan.FromSeconds(5);

        private readonly IOsProcesses _osProcesses;

        protected Service(string serviceName, IOsProcesses osProcesses)
        {
            Ensure.NotEmpty(serviceName, nameof(serviceName));

            Name = serviceName;
            _osProcesses = osProcesses;
        }

        public string Name { get; }

        public bool Exists()
        {
            return GetServices().Select(s => s.ServiceName).ContainsIgnoringCase(Name);
        }

        public void Create(string pathAndArgs, bool unrestricted)
        {
            IntPtr serviceManager = GetServiceManager();
            if (serviceManager == IntPtr.Zero)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            IntPtr service = Win32.CreateService(
                serviceManager,
                Name,
                Name,
                Win32.ServiceAccessRights.AllAccess,
                Win32.ServiceType.Win32OwnProcess,
                Win32.ServiceStartType.Demand,
                Win32.ServiceError.Normal,
                pathAndArgs,
                null,
                IntPtr.Zero,
                "Nsi\0TcpIp",
                null,
                null);

            if (service == IntPtr.Zero)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            if (unrestricted)
            {
                Win32.ServiceSidType sidType = Win32.ServiceSidType.Unrestricted;
                if (!Win32.ChangeServiceConfig2(service, Win32.ServiceConfigType.SidInfo, ref sidType))
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            }
        }

        public bool Running()
        {
            return GetServices()
                .Where(s => s.Status == ServiceControllerStatus.Running)
                .Select(s => s.ServiceName)
                .ContainsIgnoringCase(Name);
        }

        public bool IsStopped()
        {
            return GetServices()
                .Where(s => s.Status == ServiceControllerStatus.Stopped)
                .Select(s => s.ServiceName)
                .ContainsIgnoringCase(Name);
        }

        public bool Enabled()
        {
            //We first find the service and only then access StartType property, because if the service
            //is marked for removal, it will throw Win32Exception
            return GetServices()
                .Where(service => service.ServiceName.EqualsIgnoringCase(Name))
                .Any(service => service.StartType == ServiceStartMode.Manual);
        }

        public void Enable()
        {
            IOsProcess process = _osProcesses.ElevatedCommandLineProcess($"/c sc config \"{Name}\" start= demand");
            process.Start();
            process.WaitForExit(_waitForServiceEnableDuration);
        }

        public Task<Result> StartAsync(CancellationToken cancellationToken)
        {
            return ServiceControllerResult(async sc =>
            {
                sc.Start();
                await sc.WaitForStatusAsync(ServiceControllerStatus.Running, _waitForServiceStatusDuration,
                    cancellationToken);
            });
        }

        public Task<Result> StopAsync(CancellationToken cancellationToken)
        {
            return ServiceControllerResult(sc =>
            {
                sc.Stop();
                return Task.CompletedTask;
            });
        }

        protected abstract ServiceController[] GetServices();

        private async Task<Result> ServiceControllerResult(Func<ServiceController, Task> action)
        {
            using var sc = new ServiceController(Name);
            await action(sc);
            return Result.Ok();
        }

        private IntPtr GetServiceManager()
        {
            return Win32.OpenSCManager(null, null, Win32.ScmAccessRights.AllAccess);
        }
    }
}