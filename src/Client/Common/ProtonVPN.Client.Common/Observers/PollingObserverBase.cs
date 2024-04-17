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

public abstract class PollingObserverBase : ObserverBase
{
    private readonly Timer _timer;

    protected abstract TimeSpan PollingInterval { get; }

    protected bool IsTimerEnabled => _timer.Enabled;

    protected PollingObserverBase(ILogger logger, IIssueReporter issueReporter)
        : base(logger, issueReporter)
    {
        _timer = new Timer
        {
            AutoReset = true
        };
        _timer.Elapsed += OnTimerElapsed;
    }

    protected void StartTimerAndTriggerOnStart()
    {
        StartTimerWithTriggerOption(isToTriggerOnStart: true);
    }

    private void StartTimerWithTriggerOption(bool isToTriggerOnStart)
    {
        if (!_timer.Enabled)
        {
            _timer.Interval = PollingInterval.TotalMilliseconds;
            _timer.Start();
            if (isToTriggerOnStart)
            {
                TriggerAction.Run();
            }
        }
    }

    protected void StartTimer()
    {
        StartTimerWithTriggerOption(isToTriggerOnStart: false);
    }

    protected void StopTimer()
    {
        if (_timer.Enabled)
        {
            _timer.Stop();
        }
    }

    private void OnTimerElapsed(object? sender, EventArgs e)
    {
        TriggerAction.Run();
    }
}