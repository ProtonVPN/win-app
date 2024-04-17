/*
 * Copyright (c) 2024 Proton AG
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

using ProtonVPN.Client.Logic.Servers.Contracts.Updaters;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;

namespace ProtonVPN.Client.Logic.Servers;

public class ServersUpdater : IServersUpdater
{
    private readonly ILogger _logger;
    private readonly IServersCache _serversCache;
    private readonly IConfiguration _config;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    private DateTime _lastFullUpdateUtc = DateTime.MinValue;
    private DateTime _lastLoadsUpdateUtc = DateTime.MinValue;

    public ServersUpdater(ILogger logger,
        IServersCache serversCache,
        IConfiguration config)
    {
        _logger = logger;
        _serversCache = serversCache;
        _config = config;
    }

    public async Task UpdateAsync(ServersRequestParameter parameter, bool isToReprocessServers = false)
    {
        await _semaphore.WaitAsync();

        try
        {
            if (isToReprocessServers)
            {
                // Because the API requests can fail or take a long time, the current servers are reprocessed
                // beforehand to ensure the user is seeing or not seeing the correct servers already
                _serversCache.ReprocessServers();
            }

            DateTime utcNow = DateTime.UtcNow;

            if (parameter == ServersRequestParameter.ForceFullUpdate ||
                utcNow - _lastFullUpdateUtc >= _config.ServerUpdateInterval)
            {
                _lastFullUpdateUtc = utcNow;
                _lastLoadsUpdateUtc = utcNow;
                await _serversCache.UpdateAsync();
            }
            else if (parameter == ServersRequestParameter.ForceLoadsUpdate ||
                utcNow - _lastLoadsUpdateUtc >= _config.MinimumServerLoadUpdateInterval)
            {
                _lastLoadsUpdateUtc = utcNow;
                await _serversCache.UpdateLoadsAsync();
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task ForceFullUpdateIfEmptyAsync()
    {
        await _semaphore.WaitAsync();
        bool hasServers;

        try
        {
            _serversCache.LoadFromFileIfEmpty();
            hasServers = _serversCache.HasAnyServers();
        }
        finally
        {
            _semaphore.Release();
        }

        if (!hasServers)
        {
            _logger.Info<AppLog>("Fetching servers as the user has none.");
            await UpdateAsync(ServersRequestParameter.ForceFullUpdate);
        }
    }

    public async Task ClearCacheAsync()
    {
        await _semaphore.WaitAsync();

        try
        {
            _lastFullUpdateUtc = DateTime.MinValue;
            _lastLoadsUpdateUtc = DateTime.MinValue;
            _serversCache.Clear();
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
