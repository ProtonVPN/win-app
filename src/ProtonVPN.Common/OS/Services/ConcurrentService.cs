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
using System.Threading;
using System.Threading.Tasks;
using ProtonVPN.Common.Abstract;
using ProtonVPN.Common.Threading;

namespace ProtonVPN.Common.OS.Services
{
    public class ConcurrentService : IConcurrentService
    {
        private readonly IService _origin;

        private readonly CoalescingTaskQueue<ServiceAction, Result> _taskQueue;

        /// <summary>
        /// Event invoked when service start action succeeds. Event is invoked on a task pool thread.
        /// </summary>
        public event EventHandler<string> ServiceStarted;

        public ConcurrentService(IService origin)
        {
            _origin = origin;

            _taskQueue = new CoalescingTaskQueue<ServiceAction, Result>(CoalesceDecisionCallback);
        }

        public string Name => _origin.Name;

        public bool Running() => _origin.Running();

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
            var result = await _origin.StartAsync(cancellationToken);

            if (result.Success)
            {
                ServiceStarted?.Invoke(this, _origin.Name);
            }

            return result;
        }


        private async Task<Result> StopAction(CancellationToken cancellationToken)
        {
            return await _origin.StopAsync(cancellationToken);
        }

        private enum ServiceAction
        {
            Start,
            Stop
        }
    }
}
