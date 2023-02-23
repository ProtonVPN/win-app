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
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ProtonVPN.Common.Abstract;
using ProtonVPN.Common.Threading;

namespace ProtonVPN.Common.OS.Services
{
    public class ConcurrentService : IConcurrentService
    {
        protected readonly IService Service;
        private readonly IServiceEnabler _serviceEnabler;

        private readonly CoalescingTaskQueue<ServiceAction, Result> _taskQueue;

        public ConcurrentService(IService service, IServiceEnabler serviceEnabler)
        {
            Service = service;
            _serviceEnabler = serviceEnabler;

            _taskQueue = new CoalescingTaskQueue<ServiceAction, Result>(CoalesceDecisionCallback);
        }

        public string Name => Service.Name;

        public bool Running() => Service.Running();

        public bool Enabled() => Service.Enabled();

        public void Enable() => Service.Enable();

        public async Task<Result> StartAsync()
        {
            try
            {
                return await _taskQueue.Enqueue(StartAction, ServiceAction.Start);
            }
            catch (OperationCanceledException)
            {
                return Result.Fail();
            }
        }

        public async Task<Result> StopAsync()
        {
            try
            {
                return await _taskQueue.Enqueue(StopAction, ServiceAction.Stop);
            }
            catch (OperationCanceledException)
            {
                return Result.Fail();
            }
        }

        private CoalesceDecision CoalesceDecisionCallback(ServiceAction newAction, ServiceAction action, bool running)
        {
            // Joins scheduled or running task if it performs the same action, cancels otherwise.
            return newAction == action ? CoalesceDecision.Join : CoalesceDecision.Cancel;
        }

        private async Task<Result> StartAction(CancellationToken cancellationToken)
        {
            if (!Service.Exists())
            {
                return Result.Fail(new FileNotFoundException($"Service {Service.Name} is not installed."));
            }

            Result result = _serviceEnabler.GetServiceEnabledResult(Service);
            if (result.Failure)
            {
                return result;
            }

            return await Service.StartAsync(cancellationToken);
        }

        private async Task<Result> StopAction(CancellationToken cancellationToken)
        {
            return await Service.StopAsync(cancellationToken);
        }

        private enum ServiceAction
        {
            Start,
            Stop
        }
    }
}