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

using System.Diagnostics;

namespace ProtonVPN.Common.Core.Extensions;

public static class TaskExtensions
{
    public static Task<Task> Wrap(this Task task) => Task.FromResult(task);

    /// <summary>
    /// Waits for <paramref name="task"/> at most <paramref name="timeout"/> and returns whether it
    /// completed in time. On timeout the task is left running and observed in the background.
    /// A TimeoutException raised by the task itself is reported as a timeout too.
    /// Use <see cref="Task.WaitAsync(TimeSpan)"/> instead to throw rather than report.
    /// </summary>
    public static async Task<bool> WaitOrFireAndForgetAsync(this Task task, TimeSpan timeout)
    {
        try
        {
            await task.WaitAsync(timeout);
            return true;
        }
        catch (TimeoutException)
        {
            task.FireAndForget();
            return false;
        }
    }

    /// <summary>
    /// Runs <paramref name="action"/> with a token cancelled after <paramref name="timeout"/> and throws
    /// <see cref="TimeoutException"/> if it stops for that reason. The action has to honour the token.
    /// Unlike <see cref="Task.WaitAsync(TimeSpan)"/>, which only bounds the wait, this stops the work.
    /// </summary>
    public static async Task WaitOrCancelAsync(Func<CancellationToken, Task> action, TimeSpan timeout, CancellationToken cancellationToken)
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

    private static Action<Exception> _defaultExceptionHandler =
        ex => Debug.WriteLine($"[FireAndForget] Unhandled exception: {ex}");

    /// <summary>
    /// Set a global default exception handler invoked by all <see cref="FireAndForget"/> calls.
    /// </summary>
    public static void SetDefaultExceptionHandler(Action<Exception> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        _defaultExceptionHandler = handler;
    }

    /// <summary>
    /// Safely fire-and-forget a <see cref="Task"/>.
    /// Exceptions are routed to <paramref name="onException"/> and/or the global default handler.
    /// Cancellation exceptions are silently ignored.
    /// </summary>
    public static void FireAndForget(this Task task, Action<Exception>? onException = null)
    {
        HandleFireAndForgetAsync(task, onException);
    }

    private static async void HandleFireAndForgetAsync(Task task, Action<Exception>? onException)
    {
        try
        {
            await task.ConfigureAwait(false);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            onException?.Invoke(ex);
            _defaultExceptionHandler.Invoke(ex);
        }
    }

    public static Task NullSafe<T>(this Task<T>? task)
    {
        return task ?? Task.CompletedTask;
    }
}
