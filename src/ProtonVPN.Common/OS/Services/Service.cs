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
using ProtonVPN.Common.Abstract;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Helpers;

namespace ProtonVPN.Common.OS.Services
{
    public abstract class Service : IService
    {
        private static readonly TimeSpan WaitForServiceStatusDuration = TimeSpan.FromSeconds(30);

        protected Service(string serviceName)
        {
            Ensure.NotEmpty(serviceName, nameof(serviceName));

            Name = serviceName;
        }

        public string Name { get; }

        public event EventHandler<string> ServiceStartedHandler;

        public bool IsRunning()
        {
            return GetServices()
                .Where(s => s.Status == ServiceControllerStatus.Running)
                .Select(s => s.ServiceName)
                .ContainsIgnoringCase(Name);
        }

        public Result Start()
        {
            return ServiceControllerResult(sc =>
            {
                sc.Start();
                sc.WaitForStatus(ServiceControllerStatus.Running, WaitForServiceStatusDuration);
            });
        }

        public Result Stop()
        {
            return ServiceControllerResult(sc =>
            {
                sc.Stop();
            });
        }

        protected abstract ServiceController[] GetServices();

        private Result ServiceControllerResult(Action<ServiceController> action)
        {
            using var sc = new ServiceController(Name);
            action(sc);
            return Result.Ok();
        }
    }
}
