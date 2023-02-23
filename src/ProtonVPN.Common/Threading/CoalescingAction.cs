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

namespace ProtonVPN.Common.Threading
{
    /// <summary>
    /// Implements consumer in a simple producer/consumer pattern. Multiple producers are producing work,
    /// and a single consumer processes the work.
    /// </summary>
    public class CoalescingAction
    {
        private readonly Func<CancellationToken, Task> _action;
        private readonly CancellationHandle _cancellationHandle;

        private volatile Task _currentTask = Task.CompletedTask;
        private volatile int _workRequested;

        public CoalescingAction(Action action) : this(ct =>
        {
            action();
            return Task.CompletedTask;
        })
        { }

        public CoalescingAction(Func<Task> action) : this(ct => action())
        { }

        public CoalescingAction(Func<CancellationToken, Task> action)
        {
            _action = action;

            _cancellationHandle = new CancellationHandle();
        }

        public event EventHandler<TaskCompletedEventArgs> Completed;

        public bool Running { get; private set; }

        public void Run()
        {
            if (Interlocked.Exchange(ref _workRequested, 1) != 0)
                return;

            Running = true;

            var taskCompletion = new TaskCompletionSource<object>();
            var newTask = taskCompletion.Task;
            var cancellationToken = _cancellationHandle.Token;

            while (true)
            {
                var expectedTask = _currentTask;
                var previousTask = Interlocked.CompareExchange(ref _currentTask, newTask, expectedTask);
                if (previousTask != expectedTask)
                    continue;

                // ReSharper disable once PossibleNullReferenceException
                var task = previousTask.IsCompleted 
                    ? Task.Run(() => Run(cancellationToken), cancellationToken) 
                    : previousTask.ContinueWith(_ => Run(cancellationToken), cancellationToken, TaskContinuationOptions.LazyCancellation, TaskScheduler.Current).Unwrap();

                task.ContinueWith(t => OnCompleted(t, taskCompletion), TaskContinuationOptions.ExecuteSynchronously);

                return;
            }
        }

        public void Cancel()
        {
            _cancellationHandle.Cancel();
            _workRequested = 0;
        }

        private Task Run(CancellationToken cancellationToken)
        {
            Running = true;
            if (Interlocked.Exchange(ref _workRequested, 0) == 0)
                return Task.CompletedTask;

            return _action(cancellationToken);
        }

        private void OnCompleted(Task task, TaskCompletionSource<object> taskCompletion)
        {
            Running = _workRequested != 0;
            Completed?.Invoke(this, new TaskCompletedEventArgs(task));
            taskCompletion.SetResult(null);
        }
    }
}
