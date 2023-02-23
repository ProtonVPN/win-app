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
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using TaskExtensions = ProtonVPN.Common.Extensions.TaskExtensions;

namespace ProtonVPN.Common.OS.Services
{
    public static class ServiceControllerExtensions
    {
        private static readonly Dictionary<ServiceControllerStatus, ServiceControllerStatus[]> PendingStatuses = new()
        {
            { ServiceControllerStatus.Running, new[] { ServiceControllerStatus.StartPending, ServiceControllerStatus.ContinuePending } },
            { ServiceControllerStatus.Stopped, new[] { ServiceControllerStatus.StopPending } },
            { ServiceControllerStatus.Paused, new[] { ServiceControllerStatus.PausePending } },
        };

        public static async Task WaitForStatusAsync(this ServiceController controller, ServiceControllerStatus desiredStatus, TimeSpan timeout, CancellationToken cancellationToken)
        {
            await TaskExtensions.TimeoutAfter(ct => WaitForStatusAsync(controller, desiredStatus, ct), timeout, cancellationToken);
        }

        public static async Task WaitForStatusAsync(this ServiceController controller, ServiceControllerStatus desiredStatus, CancellationToken cancellationToken)
        {
            PendingStatuses.TryGetValue(desiredStatus, out var pendingStatuses);

            controller.Refresh();
            while (pendingStatuses?.Contains(controller.Status) ?? controller.Status != desiredStatus)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Delay(250, cancellationToken).ConfigureAwait(false);
                controller.Refresh();
            }

            if (controller.Status != desiredStatus)
                throw new InvalidOperationException($"Service status changed to {controller.Status}");
        }
    }
}
