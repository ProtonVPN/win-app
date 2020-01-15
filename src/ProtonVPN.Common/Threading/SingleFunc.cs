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

namespace ProtonVPN.Common.Threading
{
    public class SingleFunc<TResult>
    {
        private readonly Func<CancellationToken, Task<TResult>> _action;
        private readonly CancellationHandle _cancellationHandle;

        public SingleFunc(Func<Task<TResult>> action) : this(t => action())
        {
        }

        public SingleFunc(Func<CancellationToken, Task<TResult>> action)
        {
            _action = action;

            _cancellationHandle = new CancellationHandle();
        }

        public event EventHandler<TaskCompletedEventArgs<TResult>> Completed;

        private volatile Task<TResult> _task = System.Threading.Tasks.Task.FromResult<TResult>(default);
        public Task<TResult> Task => _task;

        public bool IsRunning => !_task.IsCompleted;

        public Task<TResult> Run()
        {
            var taskCompletion = new TaskCompletionSource<TResult>();
            var newTask = taskCompletion.Task;

            var task = Task;
            var previousTask = Interlocked.CompareExchange(ref _task, newTask, task);
            if (previousTask != task)
                return previousTask;

            var cancellationToken = _cancellationHandle.Token;
            // ReSharper disable once MethodSupportsCancellation
            System.Threading.Tasks.Task.Run(async () =>
            {
                await taskCompletion.Wrap(() => _action(cancellationToken));
                OnCompleted(taskCompletion.Task);
            });

            return newTask;
        }

        public void Cancel()
        {
            _cancellationHandle.Cancel();
        }

        private void OnCompleted(Task<TResult> task)
        {
            Completed?.Invoke(this, new TaskCompletedEventArgs<TResult>(task));
        }
    }
}
