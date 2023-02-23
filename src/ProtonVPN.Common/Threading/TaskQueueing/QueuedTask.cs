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

namespace ProtonVPN.Common.Threading.TaskQueueing
{
    internal class QueuedTask<TArg, TResult> : IDisposable
    {
        private readonly Func<CancellationToken, Task<TResult>> _action;
        private readonly TaskCompletionSource<TResult> _completionSource;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public QueuedTask(Func<CancellationToken, Task<TResult>> action, TArg arg) :
            this(action, arg, new TaskCompletionSource<TResult>(), new CancellationTokenSource())
        { }

        private QueuedTask(
            Func<CancellationToken, Task<TResult>> action,
            TArg arg,
            TaskCompletionSource<TResult> completionSource,
            CancellationTokenSource cancellationTokenSource)
        {
            _action = action;
            Arg = arg;
            _completionSource = completionSource;
            _cancellationTokenSource = cancellationTokenSource;

            Task = _completionSource.Task;
        }

        public TArg Arg { get; }

        public Task<TResult> Task { get; }

        public bool CancellationRequested => _cancellationTokenSource.IsCancellationRequested;

        public void Cancel(bool running)
        {
            if (running)
            {
                _cancellationTokenSource.Cancel();
            }
            else
            {
                _completionSource.SetCanceled();
            }
        }

        public Task Run()
        {
            return _completionSource.Wrap(() =>
            {
                var cancellationToken = _cancellationTokenSource.Token;
                cancellationToken.ThrowIfCancellationRequested();
                return _action(cancellationToken);
            });
        }

        public void Dispose()
        {
            _cancellationTokenSource.Dispose();
        }
    }
}
