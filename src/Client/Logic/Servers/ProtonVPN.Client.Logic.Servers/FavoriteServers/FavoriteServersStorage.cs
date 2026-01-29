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

using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Settings.Contracts.Observers;
using ProtonVPN.Common.Core.Extensions;

namespace ProtonVPN.Client.Logic.Servers.FavoriteServers;

public class FavoriteServersStorage : IFavoriteServersStorage
{
    private const int MAX_TOTAL = 35;
    private const int MAX_RECENTS = 15;

    private readonly IFeatureFlagsObserver _featureFlagsObserver;

    private string? _currentServerId;
    private string? _lastSearchedServerId;
    private IList<string> _recentServerIds = [];
    private IList<string> _profileServerIds = [];

    public FavoriteServersStorage(IFeatureFlagsObserver featureFlagsObserver)
    {
        _featureFlagsObserver = featureFlagsObserver;
    }

    public IEnumerable<string>? Get()
    {
        if (!_featureFlagsObserver.IsServerListTruncationEnabled)
        {
            return null;
        }

        List<string> serverIds = [];

        serverIds.AddIfNotNull(_currentServerId);
        serverIds.AddIfNotNull(_lastSearchedServerId);
        serverIds.AddRange(_recentServerIds.Take(MAX_RECENTS));
        serverIds.AddRange(_profileServerIds.Where(s => !serverIds.Contains(s)));
        serverIds.AddRange(_recentServerIds.Skip(MAX_RECENTS));

        return serverIds.Distinct().Take(MAX_TOTAL);
    }

    public void SetLastSearchedServerId(string? serverId)
    {
        _lastSearchedServerId = serverId;
    }

    public void SetCurrentServerId(string? serverId)
    {
        _currentServerId = serverId;
    }

    public void SetRecentConnectionServerIds(IList<string> serverIds)
    {
        _recentServerIds = serverIds;
    }

    public void SetProfileServerIds(IList<string> serverIds)
    {
        _profileServerIds = serverIds;
    }
}