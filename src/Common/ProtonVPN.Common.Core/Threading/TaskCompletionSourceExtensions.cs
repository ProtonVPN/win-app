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

namespace ProtonVPN.Common.Core.Threading;

public static class TaskCompletionSourceExtensions
{
    public static Task Wrap(this TaskCompletionSource<object?> source, Func<Task> action)
    {
        return source.WrapAsync(async () =>
        {
            await action();
            return null;
        });
    }

    public static async Task WrapAsync<T>(this TaskCompletionSource<T?> source, Func<Task<T?>> function)
    {
        try
        {
            T? result = await function();
            source.TrySetResult(result);
        }
        catch (OperationCanceledException)
        {
            source.TrySetCanceled();
        }
        catch (Exception e)
        {
            source.TrySetException(e);
        }
    }
}