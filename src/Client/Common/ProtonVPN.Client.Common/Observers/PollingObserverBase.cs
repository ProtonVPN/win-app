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

using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;
using Timer = System.Timers.Timer;

namespace ProtonVPN.Client.Common.Observers;

public abstract class PollingObserverBase : ObserverBase, IDisposable
{
    private readonly Timer _timer;

    protected abstract TimeSpan PollingInterval { get; }
    protected virtual TimeSpan? InitialDelay => null;

    protected bool IsTimerEnabled => _timer.Enabled;

    protected PollingObserverBase(ILogger logger, IIssueReporter issueReporter)
        : base(logger, issueReporter)
    {
        _timer = new Timer();
    }

    protected void TriggerAndStartTimer()
    {
        TriggerAction.Run();
        StartTimerWithDelay(TimeSpan.Zero);
    }

    private void StartTimerWithDelay(TimeSpan initialDelay)
    {
        if (_timer.Enabled)
        {
            return;
        }

        if (initialDelay > TimeSpan.Zero)
        {
            StartDelayTimer(initialDelay);
        }
        else
        {
            StartPollingTimer();
        }
    }

    private void StartDelayTimer(TimeSpan delay)
    {
        _timer.Elapsed -= OnInitialDelayElapsed;
        _timer.Elapsed -= OnTimerElapsed;

        _timer.Interval = delay.TotalMilliseconds;
        _timer.AutoReset = false;
        _timer.Elapsed += OnInitialDelayElapsed;
        _timer.Start();
    }

    private void OnInitialDelayElapsed(object? sender, EventArgs e)
    {
        StopDelayTimer();
        StartPollingTimer();
        TriggerAction.Run();
    }

    private void StartPollingTimer()
    {
        _timer.Elapsed -= OnInitialDelayElapsed;
        _timer.Elapsed -= OnTimerElapsed;

        _timer.Interval = PollingInterval.TotalMilliseconds;
        _timer.AutoReset = true;
        _timer.Elapsed += OnTimerElapsed;
        _timer.Start();
    }

    private void StopDelayTimer()
    {
        _timer.Stop();
        _timer.Elapsed -= OnInitialDelayElapsed;
    }

    protected void StartTimer()
    {
        TimeSpan initialDelay = InitialDelay ?? PollingInterval;
        StartTimerWithDelay(initialDelay);
    }

    protected void StopTimer()
    {
        if (_timer.Enabled)
        {
            _timer.Stop();
        }

        _timer.Elapsed -= OnInitialDelayElapsed;
        _timer.Elapsed -= OnTimerElapsed;
    }

    private void OnTimerElapsed(object? sender, EventArgs e)
    {
        TriggerAction.Run();
    }

    public void Dispose()
    {
        StopTimer();
        _timer.Dispose();
    }
}