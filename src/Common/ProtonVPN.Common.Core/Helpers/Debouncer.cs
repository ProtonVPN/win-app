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

namespace ProtonVPN.Common.Core.Helpers;

public class Debouncer<T>
{
    private readonly TimeSpan _delay;
    private readonly Func<T, Task> _function;
    private readonly object _lock = new();

    private CancellationTokenSource? _cancellationTokenSource = null;

    public Debouncer(TimeSpan delay, Func<T, Task> function)
    {
        _delay = delay;
        _function = function;
    }

    public Debouncer(TimeSpan delay, Action<T> action)
        : this(delay, o => { action(o); return Task.CompletedTask; })
    {
    }

    public void Call(T arg)
    {
        CancellationTokenSource cancellationTokenSource = new();
        CancellationToken token = cancellationTokenSource.Token;

        lock (_lock)
        {
            CancelPreviousCall();
            _cancellationTokenSource = cancellationTokenSource;
        }

        try
        {
            Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(_delay, token);
                    if (!token.IsCancellationRequested)
                    {
                        await _function(arg);
                    }
                }
                catch
                {
                }
            }, token);
        }
        catch
        {
        }
    }

    public void Cancel()
    {
        lock (_lock)
        {
            CancelPreviousCall();
        }
    }

    private void CancelPreviousCall()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;
    }
}