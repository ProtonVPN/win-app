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
    public class SingleAction : ISingleAction
    {
        private readonly Func<CancellationToken, Task> _action;
        private readonly CancellationHandle _cancellationHandle;

        public SingleAction(Action action) : this(ct =>
        {
            action();
            return Task.CompletedTask;
        })
        { }

        public SingleAction(Func<Task> action) : this(ct => action())
        { }

        public SingleAction(Func<CancellationToken, Task> action)
        {
            _action = action;

            _cancellationHandle = new CancellationHandle();
        }

        public event EventHandler<TaskCompletedEventArgs> Completed;

        private volatile Task _task = Task.CompletedTask;
        public Task Task => _task;

        public bool IsRunning => !Task.IsCompleted;

        public Task Run()
        {
            if (IsRunning)
                return Task;

            var taskCompletion = new TaskCompletionSource<object>();
            var newTask = taskCompletion.Task;
            var newExternalTask = newTask.ContinueWith(OnCompleted, TaskContinuationOptions.ExecuteSynchronously);

            var task = Task;
            var previousTask = Interlocked.CompareExchange(ref _task, newExternalTask, task);
            if (previousTask != task)
                return previousTask;

            var cancellationToken = _cancellationHandle.Token;
            // ReSharper disable once MethodSupportsCancellation
            Task.Run(async () =>
            {
                await taskCompletion.Wrap(() => _action(cancellationToken));
            });

            return newExternalTask;
        }

        public void Cancel()
        {
            _cancellationHandle.Cancel();
        }

        private void OnCompleted(Task task)
        {
            Completed?.Invoke(this, new TaskCompletedEventArgs(task));
        }
    }
}