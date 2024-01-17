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

using Timer = System.Timers.Timer;

namespace ProtonVPN.Client.Common.Observers;

public abstract class PollingObserverBase : ObserverBase, IObserver
{
    private readonly Timer _timer;

    protected abstract TimeSpan PollingInterval { get; }

    public PollingObserverBase()
        : base()
    {
        _timer = new Timer();
        _timer.Elapsed += OnTimerElapsed;
    }

    protected void UpdateAndStartTimer()
    {
        if (!_timer.Enabled)
        {
            _timer.Interval = PollingInterval.TotalMilliseconds;

            _timer.Start();
            UpdateAction.Run();
        }
    }

    protected void StopTimer()
    {
        if (_timer.Enabled)
        {
            _timer.Stop();
        }
    }

    protected void UpdateAndRestartTimer()
    {
        StopTimer();
        UpdateAndStartTimer();
    }

    private void OnTimerElapsed(object? sender, EventArgs e)
    {
        UpdateAction.Run();
    }
}