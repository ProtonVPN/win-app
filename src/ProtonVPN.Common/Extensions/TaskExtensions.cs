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

namespace ProtonVPN.Common.Extensions
{
    public static class TaskExtensions
    {
        public static Task<Task> Wrap(this Task task) => Task.FromResult(task);

        public static Task<T> AsTask<T>(this T value) => Task.FromResult(value);

        public static Task CompletedTask => Task.FromResult(true);

        public static Task<T> WithCancellation<T>(this Task<T> task, CancellationToken cancellationToken)
        {
            if (task.IsCompleted)
            {
                return task;
            }

            return task.ContinueWith(
                completedTask => completedTask.GetAwaiter().GetResult(),
                cancellationToken,
                TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);
        }

        public static async Task TimeoutAfter(this Task task, TimeSpan timeout)
        {
            using CancellationTokenSource cancellationTokenSource = new();

            Task completedTask = await Task.WhenAny(task, Task.Delay(timeout, cancellationTokenSource.Token));
            if (completedTask != task)
            {
                throw new TimeoutException();
            }

            cancellationTokenSource.Cancel();

            // Task completed within timeout. The task may have faulted or been canceled.
            // Await the task so that any exceptions/cancellation is rethrown.
            await task;
        }

        public static async Task<TResult> TimeoutAfter<TResult>(this Task<TResult> task, TimeSpan timeout)
        {
            using CancellationTokenSource cancellationTokenSource = new();

            Task completedTask = await Task.WhenAny(task, Task.Delay(timeout, cancellationTokenSource.Token));
            if (completedTask != task)
            {
                throw new TimeoutException();
            }

            cancellationTokenSource.Cancel();

            // Task completed within timeout. The task may have faulted or been canceled.
            // Await the task so that any exceptions/cancellation is rethrown.
            return await task;
        }

        public static async Task WithTimeout(this Task task, Task timeoutTask)
        {
            if (await Task.WhenAny(task, timeoutTask) != task)
            {
                throw new TimeoutException();
            }

            // Task completed within timeout. The task may have faulted or been canceled.
            // Await the task so that any exceptions/cancellation is rethrown.
            await task;
        }

        public static async Task<TResult> WithTimeout<TResult>(this Task<TResult> task, Task timeoutTask)
        {
            if (await Task.WhenAny(task, timeoutTask) != task)
            {
                throw new TimeoutException();
            }

            // Task completed within timeout. The task may have faulted or been canceled.
            // Await the task so that any exceptions/cancellation is rethrown.
            return await task;
        }

        public static async Task TimeoutAfter(Func<CancellationToken, Task> action, TimeSpan timeout, CancellationToken cancellationToken)
        {
            using CancellationTokenSource timeoutSource = new(timeout);
            using CancellationTokenSource linkedCancellationSource =
                CancellationTokenSource.CreateLinkedTokenSource(new[] { cancellationToken, timeoutSource.Token });

            try
            {
                await action(linkedCancellationSource.Token);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested && timeoutSource.IsCancellationRequested)
            {
                throw new TimeoutException();
            }
        }
        
        public static void IgnoreExceptions(this Task task)
        {
            task.ContinueWith(c => { AggregateException ignored = c.Exception; },
                TaskContinuationOptions.OnlyOnFaulted |
                TaskContinuationOptions.ExecuteSynchronously);
        }
    }
}