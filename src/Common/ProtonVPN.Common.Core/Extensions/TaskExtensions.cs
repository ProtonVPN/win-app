/*
 * Copyright (c) 2024 Proton AG
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

namespace ProtonVPN.Common.Core.Extensions;

public static class TaskExtensions
{
    public static Task<Task> Wrap(this Task task) => Task.FromResult(task);

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

    /// <summary>Run this task in parallel without awaiting, and ignore any exceptions</summary>
    public static void FireAndForget(this Task task)
    {
        task.ContinueWith(c => { AggregateException? ignored = c.Exception; },
            TaskContinuationOptions.OnlyOnFaulted |
            TaskContinuationOptions.ExecuteSynchronously);
    }
}