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
using System.Threading;
using System.Threading.Tasks;
using ProtonVPN.Common.Threading.TaskQueueing;

namespace ProtonVPN.Common.Threading
{
    /// <summary>
    /// Queues at most two tasks: one running and one pending. Next queued task will either
    /// join pending or running task or cancel them to be scheduled.
    /// </summary>
    /// <typeparam name="TArg">Type of action performed by the tasks</typeparam>
    /// <typeparam name="TResult">Type of the result of queued tasks</typeparam>
    public class CoalescingTaskQueue<TArg, TResult>
    {
        /// <summary>
        /// The callback delegate called during each call to <see cref="Enqueue"/> to make
        /// decision on joining or cancelling pending and running task.
        /// </summary>
        /// <param name="newArg">The new action being scheduled.</param>
        /// <param name="arg">The pending or running action.</param>
        /// <param name="running">True if action is running, false if pending</param>
        /// <returns></returns>
        public delegate CoalesceDecision CoalesceDecisionDelegate(TArg newArg, TArg arg, bool running);

        private readonly CoalesceDecisionDelegate _decisionCallback;

        private volatile CoalescingTaskQueueData<TArg, TResult> _work = CoalescingTaskQueueData<TArg, TResult>.Empty;

        /// <summary>
        /// Creates the <see cref="CoalescingTaskQueue&lt;TArg, TResult&gt;"/>.
        /// </summary>
        /// <param name="decisionCallback">
        /// The callback delegate called during each call to <see cref="Enqueue"/> to make
        /// decision on joining or cancelling pending and running task. The callback delegate might be called
        /// multiple times during one call to <see cref="Enqueue"/>.
        /// </param>
        public CoalescingTaskQueue(CoalesceDecisionDelegate decisionCallback)
        {
            _decisionCallback = decisionCallback;
        }

        /// <summary>
        /// Schedules new task. The coalesce decision callback will be called for pending and running tasks
        /// before returning from this method.
        /// </summary>
        /// <param name="action">The task to schedule.</param>
        /// <param name="arg">The action to schedule. Will be passed as an argument to the coalesce decision callback.</param>
        /// <returns>The result returned by the task.</returns>
        public Task<TResult> Enqueue(Func<CancellationToken, Task<TResult>> action, TArg arg)
        {
            CoalescingTaskQueueData<TArg, TResult> work;
            CoalescingTaskQueueData<TArg, TResult> newWork;
            CoalescingTaskQueueData<TArg, TResult> previousWork;
            bool cancelRunningTask;
            QueuedTask<TArg, TResult> newTask = null;

            do
            {
                work = _work;
                cancelRunningTask = false;
                newTask?.Dispose();

                if (work.PendingTask != null)
                {
                    // Makes decision on pending task
                    var decision = _decisionCallback(arg, work.PendingTask.Arg, false);
                    if (decision == CoalesceDecision.Join)
                    {
                        return _work.PendingTask.Task;
                    }
                }

                if (work.RunningTask != null && !work.RunningTask.CancellationRequested)
                {
                    // Makes decision on running task
                    var decision = _decisionCallback(arg, work.RunningTask.Arg, true);
                    if (decision == CoalesceDecision.Join)
                    {
                        return _work.RunningTask.Task;
                    }

                    // Doesn't cancel running task until successfully scheduling new task, will do that later.
                    cancelRunningTask = decision == CoalesceDecision.Cancel;
                }

                newTask = new QueuedTask<TArg, TResult>(action, arg);

                if (work.RunningTask == null)
                {
                    // No task is running. Schedule new running task, will start it later.
                    newWork = new CoalescingTaskQueueData<TArg, TResult>(
                        null,
                        newTask);
                }
                else
                {
                    // A task is running. Schedule new pending task.
                    newWork = new CoalescingTaskQueueData<TArg, TResult>(
                        newTask,
                        work.RunningTask);
                }

                previousWork = Interlocked.CompareExchange(ref _work, newWork, work);

            } while (previousWork != work);

            // Scheduling succeeded

            // Cancel previous pending task if any
            var pendingTask = work.PendingTask;
            if (pendingTask != null)
            {
                pendingTask.Cancel(false);
                pendingTask.Dispose();
            }

            // Cancel running task if requested by decision callback
            if (cancelRunningTask)
            {
                work.RunningTask.Cancel(true);
            }

            // Start new task if scheduled as running
            if (newWork.RunningTask == newTask)
            {
                Run(newTask);
            }

            return newTask.Task;
        }

        /// <summary>
        /// Cancels both pending and running tasks if any.
        /// </summary>
        public void Cancel()
        {
            CoalescingTaskQueueData<TArg, TResult> work;
            CoalescingTaskQueueData<TArg, TResult> previousWork;

            do
            {
                work = _work;

                var newWork = new CoalescingTaskQueueData<TArg, TResult>(
                    null,
                    work.RunningTask);

                previousWork = Interlocked.CompareExchange(ref _work, newWork, work);

            } while (previousWork != work);

            work.PendingTask?.Cancel(false);
            work.PendingTask?.Dispose();

            work.RunningTask?.Cancel(true);
        }

        private void Run(QueuedTask<TArg, TResult> queuedTask)
        {
            Task.Run(async () =>
            {
                await queuedTask.Run();
                RunPendingTask();
            });
        }

        /// <summary>
        /// Starts pending task if any.
        /// </summary>
        private void RunPendingTask()
        {
            CoalescingTaskQueueData<TArg, TResult> work;
            CoalescingTaskQueueData<TArg, TResult> newWork;
            CoalescingTaskQueueData<TArg, TResult> previousWork;

            do
            {
                work = _work;

                newWork = new CoalescingTaskQueueData<TArg, TResult>(
                    null,
                    work.PendingTask);

                previousWork = Interlocked.CompareExchange(ref _work, newWork, work);

            } while (previousWork != work);

            work.RunningTask?.Dispose();

            if (newWork.RunningTask != null)
            {
                Run(newWork.RunningTask);
            }
        }
    }

    public enum CoalesceDecision
    {
        None,
        Cancel,
        Join
    }
}
