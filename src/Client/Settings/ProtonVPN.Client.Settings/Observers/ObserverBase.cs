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

using ProtonVPN.Api.Contracts;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Observers;
using ProtonVPN.Common.Legacy.Threading;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Logging.Contracts;
using Timer = System.Timers.Timer;

namespace ProtonVPN.Client.Settings.Observers;

public abstract class ObserverBase : IObserver
{
    protected readonly ISettings Settings;
    protected readonly IApiClient ApiClient;
    protected readonly IConfiguration Config;
    protected readonly ILogger Logger;

    private readonly Timer _timer;
    private readonly SingleAction _updateAction;

    protected abstract TimeSpan PollingInterval { get; }

    public ObserverBase(
        ISettings settings,
        IApiClient apiClient,
        IConfiguration config,
        ILogger logger)
    {
        Settings = settings;
        ApiClient = apiClient;
        Config = config;
        Logger = logger;

        _updateAction = new SingleAction(UpdateAsync);

        _timer = new Timer
        {
            Interval = PollingInterval.TotalMilliseconds
        };
        _timer.Elapsed += OnTimerElapsed;
    }

    protected abstract Task UpdateAsync();

    protected void StartTimer()
    {
        if (!_timer.Enabled)
        {
            _timer.Start();
            _updateAction.Run();
        }
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
        _updateAction.Run();
    }
}