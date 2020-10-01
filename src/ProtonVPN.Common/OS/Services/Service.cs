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
using System.Linq;
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
        private static readonly TimeSpan WaitForServiceStatusDuration = TimeSpan.FromSeconds(30);
        private readonly IOsProcesses _osProcesses;

        protected Service(string serviceName, IOsProcesses osProcesses)
        {
            Ensure.NotEmpty(serviceName, nameof(serviceName));

            Name = serviceName;
            _osProcesses = osProcesses;
        }

        public string Name { get; }

        public bool Running()
        {
            return GetServices()
                .Where(s => s.Status == ServiceControllerStatus.Running)
                .Select(s => s.ServiceName)
                .ContainsIgnoringCase(Name);
        }

        public bool Enabled()
        {
            return GetServices()
                .Where(s => s.StartType == ServiceStartMode.Manual)
                .Select(s => s.ServiceName)
                .ContainsIgnoringCase(Name);
        }

        public void Enable()
        {
            var process = _osProcesses.ElevatedCommandLineProcess($"/c sc config \"{Name}\" start= demand");
            process.Start();
        }

        public Task<Result> StartAsync(CancellationToken cancellationToken)
        {
            return ServiceControllerResult(async sc =>
            {
                sc.Start();
                await sc.WaitForStatusAsync(ServiceControllerStatus.Running, WaitForServiceStatusDuration, cancellationToken);
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
    }
}
