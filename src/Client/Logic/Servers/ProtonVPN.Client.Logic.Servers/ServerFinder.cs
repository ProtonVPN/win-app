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

using System.Collections.Concurrent;
using System.Net;
using System.Text.RegularExpressions;
using ProtonVPN.Api.Contracts;
using ProtonVPN.Api.Contracts.Servers;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Servers.Cache;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Messages;
using ProtonVPN.Client.Logic.Servers.Contracts.Searches;
using ProtonVPN.Common.Core.Extensions;
using ProtonVPN.Common.Core.Helpers;
using ProtonVPN.Configurations.Contracts;

namespace ProtonVPN.Client.Logic.Servers;

public partial class ServerFinder : IServerFinder
{
    private static readonly TimeSpan _searchResultExpirationInterval = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(5);

    private readonly IConfiguration _config;
    private readonly IServersCache _serversCache;
    private readonly IEventMessageSender _eventMessageSender;
    private readonly IFavoriteServersStorage _favoriteServersStorage;

    private readonly Lazy<Debouncer<string>> _serverSearchDebouncer;

    [GeneratedRegex("[a-zA-Z]{2,}(-[a-zA-Z]{2,})*(#)?[0-9]+(-[a-zA-Z0-9]{2,})*")]
    private static partial Regex ServerNameRegex();
    private readonly Regex _serverNameRegex = ServerNameRegex();

    private readonly ConcurrentDictionary<string, DateTime> _searchCache = new(StringComparer.InvariantCultureIgnoreCase);
    private Timer? _cleanupTimer;

    private readonly object _timerLock = new();
    private readonly object _searchBlockLock = new();

    private bool _isSearchBlocked;

    public ServerFinder(
        IConfiguration config,
        IServersCache serversCache,
        IEventMessageSender eventMessageSender,
        IFavoriteServersStorage favoriteServersStorage)
    {
        _config = config;
        _serversCache = serversCache;
        _eventMessageSender = eventMessageSender;
        _favoriteServersStorage = favoriteServersStorage;

        _serverSearchDebouncer = new(() => new(_config.ServerSearchDelay, input => TriggerServerSearchAsync(input)));
    }

    public void Search(string input)
    {
        lock (_timerLock)
        {
            _cleanupTimer ??= new Timer(CleanupSearchCache, null, _cleanupInterval, _cleanupInterval);
        }

        lock (_searchBlockLock)
        {
            bool isSearchBlocked = _isSearchBlocked;

            if (!isSearchBlocked && _serverNameRegex.IsMatch(input) && !IsCacheContaining(input))
            {
                input = AddMissingHashSymbolIfNeeded(input);
                _serverSearchDebouncer.Value.Call(input);
            }
        }
    }

    private bool IsCacheContaining(string input)
    {
        DateTime utcNow = DateTime.UtcNow;

        return _searchCache.TryGetValue(input, out DateTime expirationTime) &&
               expirationTime >= utcNow;
    }

    private string AddMissingHashSymbolIfNeeded(string input)
    {
        if (input.Contains('#'))
        {
            return input;
        }

        int indexOfFirstDigit = input.IndexOfFirstDigit();
        if (indexOfFirstDigit < 0)
        {
            return input;
        }

        return input.Insert(indexOfFirstDigit, "#");
    }

    private async Task TriggerServerSearchAsync(string input)
    {
        bool isSearchBlocked;
        lock (_searchBlockLock)
        {
            isSearchBlocked = _isSearchBlocked;
        }

        if (isSearchBlocked)
        {
            return;
        }

        ApiResponseResult<LookupServerResponse>? response = await _serversCache.LookupAsync(input);
        if (response is null)
        {
            return;
        }

        if (response.ResponseMessage.StatusCode == HttpStatusCode.TooManyRequests)
        {
            lock (_searchBlockLock)
            {
                _isSearchBlocked = true;
                Cancel();
            }
        }

        if (response.Success || response.Value.Code == ResponseCodes.SERVER_DOES_NOT_EXIST)
        {
            _searchCache.TryAdd(input, DateTime.UtcNow + _searchResultExpirationInterval);
        }

        if (response.Success)
        {
            _favoriteServersStorage.SetLastSearchedServerId(response.Value.LogicalServer.Id);
            _eventMessageSender.Send<NewServerFoundMessage>();
        }
    }

    public void Cancel()
    {
        _serverSearchDebouncer.Value.Cancel();
    }

    public void ClearSearchBlock()
    {
        lock (_searchBlockLock)
        {
            _isSearchBlocked = false;
        }
    }

    private void CleanupSearchCache(object? state)
    {
        DateTime utcNow = DateTime.UtcNow;
        List<string> expiredKeys = _searchCache
            .Where(kvp => kvp.Value <= utcNow)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (string key in expiredKeys)
        {
            _searchCache.TryRemove(key, out _);
        }

        if (_searchCache.IsEmpty)
        {
            lock (_timerLock)
            {
                _cleanupTimer?.Dispose();
                _cleanupTimer = null;
            }
        }
    }
}