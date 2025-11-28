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

namespace ProtonVPN.Common.Core.Threading;

public class AsyncManualResetEventSlim
{
    private readonly ManualResetEventSlim _event;

    public bool IsSet => _event.IsSet;

    public AsyncManualResetEventSlim(bool initialState = false)
    {
        _event = new(initialState: initialState);
    }

    public Task WaitAsync(CancellationToken cancellationToken = default)
    {
        CancellationTokenRegistration cancellationRegistration = default;
        WaitHandle waitHandle = _event.WaitHandle;

        TaskCompletionSource tcs = new();
        RegisteredWaitHandle handle = ThreadPool.RegisterWaitForSingleObject(
            waitObject: waitHandle,
            callBack: (o, timeout) =>
            {
                cancellationRegistration.Unregister();
                tcs.TrySetResult();
            },
            state: null,
            timeout: Timeout.InfiniteTimeSpan,
            executeOnlyOnce: true);

        if (cancellationToken.CanBeCanceled)
        {
            cancellationRegistration = cancellationToken.Register(() =>
            {
                handle.Unregister(waitHandle);
                tcs.TrySetCanceled(cancellationToken);
            });
        }

        return tcs.Task;
    }

    public void Set()
    {
        _event.Set();
    }

    public void Reset()
    {
        _event.Reset();
    }
}
