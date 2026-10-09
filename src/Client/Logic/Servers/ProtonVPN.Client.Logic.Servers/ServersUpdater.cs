/*
 * Copyright (c) 2026 Proton AG
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
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;

namespace ProtonVPN.Client.Logic.Servers;

public class ServersUpdater : IServersUpdater
{
    private readonly ILogger _logger;
    private readonly IIssueReporter _issueReporter;
    private readonly IServersCache _serversCache;
    private readonly IServerCountCache _serverCountCache;
    private readonly ISettings _settings;
    private readonly IConfiguration _config;

    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public ServersUpdater(
        ILogger logger,
        IIssueReporter issueReporter,
        IServersCache serversCache,
        IServerCountCache serverCountCache,
        ISettings settings,
        IConfiguration config)
    {
        _logger = logger;
        _issueReporter = issueReporter;
        _serversCache = serversCache;
        _serverCountCache = serverCountCache;
        _settings = settings;
        _config = config;
    }

    public async Task UpdateAsync(CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken);

        try
        {
            _logger.Info<AppLog>("Server update requested");

            _serversCache.LoadFromFileIfEmpty();

            bool isServersCacheInvalid = IsServersCacheInvalid();
            bool isServersUpdateNeeded = IsServersUpdateNeeded();

            if (isServersCacheInvalid)
            {
                _logger.Info<AppLog>("Server cache is invalid, forcing full update");

                await ForceUpdateServersAsync(cancellationToken);
            }
            else if (isServersUpdateNeeded)
            {
                _logger.Info<AppLog>("Server cache is outdated, updating");

                await UpdateServersAsync(cancellationToken);
            }

            bool isLoadsUpdateNeeded = IsLoadsUpdateNeeded();
            if (isLoadsUpdateNeeded)
            {
                _logger.Info<AppLog>("Load cache is outdated, updating");

                await _serversCache.UpdateLoadsAsync(cancellationToken);
            }

            bool isLoadsRecomputeNeeded = IsLoadsRecomputeNeeded();
            if (isLoadsRecomputeNeeded)
            {
                _logger.Info<AppLog>("Device coordinates changed, recomputing the server scores");

                _serversCache.RecomputeLoads();
            }

            if (!isServersCacheInvalid && !isServersUpdateNeeded && !isLoadsUpdateNeeded && !isLoadsRecomputeNeeded)
            {
                _logger.Info<AppLog>("No server update needed, using cached servers");
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task UpdateOverdueLoadsAsync()
    {
        try
        {
            // Safety net for when the observer missed a refresh. Does nothing while the loads are recent.
            if (!IsLoadsOverdue())
            {
                return;
            }

            _logger.Info<AppLog>("Server loads are overdue, updating");
            _issueReporter.CaptureMessage("Overdue server loads detected");

            // Callers must not wait longer than the configured timeout.
            Task updateTask = UpdateLoadsIfOverdueAsync(CancellationToken.None);

            if (!await updateTask.WaitOrFireAndForgetAsync(_config.OverdueServerLoadUpdateTimeout))
            {
                _logger.Warn<AppLog>("Server loads are overdue but were not updated in time, using the cached ones.");
            }
        }
        catch (Exception e)
        {
            _logger.Error<AppLog>("Failed to update the overdue server loads, using the cached ones.", e);
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

    public async Task ForceUpdateLoadsAsync(CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken);

        try
        {
            _logger.Info<AppLog>("Force loads update requested");

            _serversCache.LoadFromFileIfEmpty();

            if (_serversCache.IsEmpty())
            {
                _logger.Info<AppLog>("Server cache is empty, skipping the loads update");
                return;
            }

            await _serversCache.UpdateLoadsAsync(cancellationToken);
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

    public bool HasServersRequestFailed()
    {
        return _serversCache.HasServersRequestFailed();
    }

    private bool IsLoadsOverdue()
    {
        return !_serversCache.IsEmpty() && _serversCache.IsLoadOverdue();
    }

    private async Task UpdateLoadsIfOverdueAsync(CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken);

        try
        {
            // Rechecked under the semaphore, an update may have completed while waiting.
            if (!IsLoadsOverdue())
            {
                return;
            }

            await _serversCache.UpdateLoadsAsync(cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private bool IsServersCacheInvalid()
    {
        return _serversCache.IsEmpty() || _serversCache.IsStale();
    }

    private bool IsServersUpdateNeeded()
    {
        return _serversCache.IsOutdated();
    }

    private bool IsLoadsUpdateNeeded()
    {
        return !_serversCache.IsEmpty() && _serversCache.IsLoadOutdated();
    }

    private bool IsLoadsRecomputeNeeded()
    {
        return !_serversCache.IsEmpty() && _serversCache.IsLoadStale();
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
}