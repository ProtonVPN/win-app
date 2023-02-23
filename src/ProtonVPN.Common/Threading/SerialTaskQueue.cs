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
    public class SerialTaskQueue : ITaskQueue
    {
        private volatile Task _currentTask = Task.CompletedTask;

        public async Task<T> Enqueue<T>(Func<Task<T>> function)
        {
            TaskCompletionSource<object> taskCompletion = new TaskCompletionSource<object>();
            Task<object> newTask = taskCompletion.Task;
            while (true)
            {
                Task task = _currentTask;
                Task previousTask = Interlocked.CompareExchange(ref _currentTask, newTask, task);
                if (previousTask != task)
                {
                    continue;
                }

                // ReSharper disable once PossibleNullReferenceException
                await previousTask;
                try
                {
                    return await function();
                }
                finally
                {
                    taskCompletion.SetResult(null);
                }
            }
        }

        public async Task<IDisposable> Lock()
        {
            TaskCompletionSource<object> taskCompletion = new TaskCompletionSource<object>();
            Task<object> newTask = taskCompletion.Task;
            while (true)
            {
                Task task = _currentTask;
                Task previousTask = Interlocked.CompareExchange(ref _currentTask, newTask, task);
                if (previousTask != task)
                {
                    continue;
                }

                // ReSharper disable once PossibleNullReferenceException
                await previousTask;
                return new AsyncLock(taskCompletion);
            }
        }

        private class AsyncLock : IDisposable
        {
            private readonly TaskCompletionSource<object> _taskCompletion;

            public AsyncLock(TaskCompletionSource<object> taskCompletion)
            {
                _taskCompletion = taskCompletion;
            }

            public void Dispose()
            {
                _taskCompletion.TrySetResult(null);
            }
        }
    }
}
