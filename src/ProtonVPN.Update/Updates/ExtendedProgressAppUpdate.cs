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
using ProtonVPN.Common.Threading;

namespace ProtonVPN.Update.Updates
{
    /// <summary>
    /// Extends duration of the update state representing progress (checking and downloading) up to
    /// specified minimum progress duration. Required for short state change to be noticed in the UI.
    /// </summary>
    internal class ExtendedProgressAppUpdate : INotifyingAppUpdate
    {
        private readonly TimeSpan _minProgressDuration;
        private readonly INotifyingAppUpdate _origin;

        private readonly SerialTaskQueue _notifyQueue;
        private readonly CancellationHandle _cancellationHandle = new CancellationHandle();

        private DateTime _progressStartedAt;
        private IAppUpdateState _prevState;

        public ExtendedProgressAppUpdate(TimeSpan minProgressDuration, INotifyingAppUpdate origin)
        {
            _minProgressDuration = minProgressDuration;
            _origin = origin;

            _origin.StateChanged += AppUpdate_StateChanged;
            _notifyQueue = new SerialTaskQueue();
        }

        public event EventHandler<IAppUpdateState> StateChanged;

        public void StartCheckingForUpdate(bool earlyAccess) => _origin.StartCheckingForUpdate(earlyAccess);

        public Task StartUpdating(bool auto) => _origin.StartUpdating(auto);

        private void AppUpdate_StateChanged(object sender, IAppUpdateState state)
        {
            if (ProgressStarted(state))
            {
                HandleProgressStart();
            }
            else if (ProgressEnded(state))
            {
                HandleProgressEnd();
            }

            _prevState = state;
            _notifyQueue.Enqueue(() => OnStateChanged(state));
        }

        private bool ProgressStarted(IAppUpdateState state)
        {
            return _prevState?.Status.InProgress() != true && state.Status.InProgress();
        }

        private bool ProgressEnded(IAppUpdateState state)
        {
            return _prevState?.Status.InProgress() == true && !state.Status.InProgress();
        }

        private void HandleProgressStart()
        {
            _progressStartedAt = DateTime.UtcNow;
            _cancellationHandle.Cancel();
        }

        private void HandleProgressEnd()
        {
            TimeSpan requiredDelay = _progressStartedAt + _minProgressDuration - DateTime.UtcNow;
            if (requiredDelay > TimeSpan.Zero)
            {
                _notifyQueue.Enqueue(() => Delay(requiredDelay));
            }

            _progressStartedAt = DateTime.MinValue;
        }

        private async Task Delay(TimeSpan delay)
        {
            try
            {
                await Task.Delay(delay, _cancellationHandle.Token);
            }
            catch (TaskCanceledException)
            {
            }
        }

        private void OnStateChanged(IAppUpdateState state)
        {
            StateChanged?.Invoke(this, state);
        }
    }
}
