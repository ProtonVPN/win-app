/*
 * Copyright (c) 2025 Proton AG
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

using System.Collections.Concurrent;

namespace ProtonVPN.Common.Core.Helpers;

/// <summary>
/// Runs only once at any time even if called multiple times during that [execution time + timeout interval].
/// Tasks called while a execution is running are released when execution is finished.
/// Tasks called in the timeout interval wait for that interval to end before executing (and then only one is ran as expected).
/// </summary>
public class SingleFunctionExecutor
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly ConcurrentQueue<TaskCompletionSource> _queue = new();
    private readonly Func<Task> _function;
    private readonly TimeSpan _retryTimeoutInterval;
    private DateTime _retryDateUtc = DateTime.MinValue;

    public SingleFunctionExecutor(Func<Task> function, TimeSpan? retryTimeoutInterval = null)
    {
        _function = function;
        _retryTimeoutInterval = retryTimeoutInterval ?? TimeSpan.Zero;
    }

    public async Task ExecuteAsync()
    {
        TaskCompletionSource tcs = new();

        bool isFirst;
        TimeSpan? delay = null;
        await _semaphore.WaitAsync();
        try
        {
            _queue.Enqueue(tcs);
            isFirst = _queue.Count == 1;

            DateTime utcNow = DateTime.UtcNow;
            if (utcNow < _retryDateUtc)
            {
                delay = _retryDateUtc - utcNow;
            }
        }
        finally
        {
            _semaphore.Release();
        }

        if (delay is not null &&
            delay.Value > TimeSpan.Zero &&
            delay.Value.TotalMilliseconds < int.MaxValue)
        {
            await Task.Delay(delay.Value);
        }

        if (isFirst)
        {
            try
            {
                await _function();
            }
            finally
            {
                await _semaphore.WaitAsync();
                try
                {
                    while (_queue.TryDequeue(out TaskCompletionSource? queuedTcs))
                    {
                        queuedTcs.TrySetResult();
                    }
                    _queue.Clear();
                    _retryDateUtc = DateTime.UtcNow + _retryTimeoutInterval;
                }
                finally
                {
                    _semaphore.Release();
                }
            }
        }
        else
        {
            await tcs.Task;
        }
    }
}