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
using System.Threading.Tasks;

namespace ProtonVPN.Common.Threading
{
    public static class TaskQueueExtensions
    {
        public static Task Enqueue(this ITaskQueue queue, Action action)
        {
            return queue.Enqueue(() =>
            {
                action();
                return Task.FromResult<object>(null);
            });
        }

        public static Task<T> Enqueue<T>(this ITaskQueue queue, Func<T> function)
        {
            return queue.Enqueue(() => Task.FromResult(function()));
        }

        public static Task Enqueue(this ITaskQueue queue, Func<Task> action)
        {
            return queue.Enqueue<object>(async () =>
            {
                await action();
                return null;
            });
        }
    }
}
