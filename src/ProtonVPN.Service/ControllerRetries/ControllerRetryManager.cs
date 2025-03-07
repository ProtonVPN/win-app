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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.ConnectLogs;
using ProtonVPN.ProcessCommunication.Contracts.Entities;
using Timer = System.Timers.Timer;

namespace ProtonVPN.Service.ControllerRetries;

public class ControllerRetryManager : IControllerRetryManager
{
    private readonly TimeSpan _cleanupTimerInterval = TimeSpan.FromMinutes(10);
    private readonly TimeSpan _expirationInterval = TimeSpan.FromHours(1);

    private readonly ILogger _logger;
    private readonly Timer _timer;
    private readonly object _lock = new object();
    private readonly ConcurrentDictionary<Guid, DateTimeOffset> _receivedRetryIds = [];

    public ControllerRetryManager(ILogger logger)
    {
        _logger = logger;

        _timer = new(_cleanupTimerInterval);
        _timer.Elapsed += OnTimedEvent;
        _timer.AutoReset = true;
        _timer.Enabled = true;
    }

    private void OnTimedEvent(object sender, EventArgs e)
    {
        lock (_lock)
        {
            List<KeyValuePair<Guid, DateTimeOffset>> list = _receivedRetryIds.ToList();
            foreach (KeyValuePair<Guid, DateTimeOffset> pair in list)
            {
                if (pair.Value <= DateTimeOffset.UtcNow)
                {
                    _receivedRetryIds.TryRemove(pair.Key, out _);
                }
            }
        }
    }

    public void EnforceRetryId(IRetryableEntity retryableEntity, [CallerMemberName] string sourceMemberName = "")
    {
        ThrowIfRetryIdIsEmpty(retryableEntity, sourceMemberName);

        lock (_lock)
        {
            ThrowIfRetryIdWasAlreadyUsed(retryableEntity, sourceMemberName);

            DateTimeOffset expirationDateUtc = DateTimeOffset.UtcNow + _expirationInterval;
            _receivedRetryIds.AddOrUpdate(retryableEntity.RetryId, _ => expirationDateUtc, (_, _) => expirationDateUtc);
        }
    }

    private void ThrowIfRetryIdIsEmpty(IRetryableEntity retryableEntity, string sourceMemberName)
    {
        if (retryableEntity.RetryId == Guid.Empty)
        {
            string message = $"{sourceMemberName} request dropped because the retry ID is empty.";
            _logger.Info<ConnectLog>(message);
            throw new ArgumentException(message);
        }
    }

    private void ThrowIfRetryIdWasAlreadyUsed(IRetryableEntity retryableEntity, string sourceMemberName)
    {
        if (_receivedRetryIds.ContainsKey(retryableEntity.RetryId))
        {
            string message = $"{sourceMemberName} request dropped because the retry ID is repeated.";
            _logger.Info<ConnectLog>(message);
            throw new ArgumentException(message);
        }
    }
}
