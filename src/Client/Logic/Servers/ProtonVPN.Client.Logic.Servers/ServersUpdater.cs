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

using ProtonVPN.Client.Logic.Servers.Cache;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Common.Core.Extensions;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;

namespace ProtonVPN.Client.Logic.Servers;

public class ServersUpdater : IServersUpdater
{
    private readonly ILogger _logger;
    private readonly IServersCache _serversCache;
    private readonly IServerCountCache _serverCountCache;
    private readonly ISettings _settings;

    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public ServersUpdater(
        ILogger logger,
        IServersCache serversCache,
        IServerCountCache serverCountCache,
        ISettings settings)
    {
        _logger = logger;
        _serversCache = serversCache;
        _serverCountCache = serverCountCache;
        _settings = settings;
    }

    public async Task UpdateAsync(CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken);

        try
        {
            _logger.Info<AppLog>("Server update requested");

            _serversCache.LoadFromFileIfEmpty();

            if (_serversCache.IsEmpty() || _serversCache.IsStale())
            {
                _logger.Info<AppLog>("Server cache is invalid, forcing full update");

                await ForceUpdateServersAsync(cancellationToken);
            }
            else if (_serversCache.IsOutdated())
            {
                _logger.Info<AppLog>("Server cache is outdated, updating");

                await UpdateServersAsync(cancellationToken);
            }
            else if (_serversCache.IsLoadOutdated())
            {
                _logger.Info<AppLog>("Load cache is outdated, updating");

                await UpdateLoadsAsync(cancellationToken);
            }
            else
            {
                _logger.Info<AppLog>("No server update needed, using cached servers");
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task ForceUpdateAsync(CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken);

        try
        {
            _logger.Info<AppLog>("Force server update requested");

            _serversCache.LoadFromFileIfEmpty();

            await ForceUpdateServersAsync(cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task ForceLoadsUpdateAsync(CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync();

        try
        {
            _logger.Info<AppLog>("Force load update requested");

            _serversCache.LoadFromFileIfEmpty();

            await UpdateLoadsAsync(cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task ClearCacheAsync(CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken);

        try
        {
            _logger.Info<AppLog>("Clear server cache requested");

            _serversCache.Clear();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private Task ForceUpdateServersAsync(CancellationToken cancellationToken)
    {
        _settings.LogicalsLastModifiedDate = DefaultSettings.LogicalsLastModifiedDate;

        return UpdateServersAsync(cancellationToken);
    }

    private Task UpdateServersAsync(CancellationToken cancellationToken)
    {
        // No need to await here, the server count is not critical for the servers cache update
        _serverCountCache.UpdateAsync().FireAndForget();

        return _serversCache.UpdateAsync(cancellationToken);
    }

    private Task UpdateLoadsAsync(CancellationToken cancellationToken)
    {
        return _serversCache.UpdateLoadsAsync(cancellationToken);
    }
}