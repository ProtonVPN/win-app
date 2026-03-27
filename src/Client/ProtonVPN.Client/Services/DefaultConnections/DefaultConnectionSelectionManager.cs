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

using ProtonVPN.Client.Common.Collections;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.Countries;
using ProtonVPN.Client.Logic.Profiles.Contracts.Models;
using ProtonVPN.Client.Logic.Recents.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Settings.Contracts.Enums;
using ProtonVPN.Client.Settings.Contracts.Models;
using ProtonVPN.Client.UI.Main.Settings.Pages.ConnectionPreferences;

namespace ProtonVPN.Client.Services.DefaultConnections;

public class DefaultConnectionSelectionManager : IDefaultConnectionSelectionManager
{
    private readonly IServersLoader _serversLoader;
    private readonly IRecentConnectionsManager _recentConnectionsManager;
    private readonly ILocalizationProvider _localizer;

    public SmartObservableCollection<object> Connections { get; } = [];

    public DefaultConnectionSelectionManager(
        IServersLoader serversLoader,
        IRecentConnectionsManager recentConnectionsManager,
        ILocalizationProvider localizer)
    {
        _serversLoader = serversLoader;
        _recentConnectionsManager = recentConnectionsManager;
        _localizer = localizer;
    }

    public void Refresh()
    {
        List<object> items = [];

        if (_serversLoader.HasAnyCountries())
        {
            items.Add(new DefaultConnectionItem(
                DefaultConnectionItemType.Fastest,
                _localizer.Get("Settings_Connection_Default_Fastest")));
            items.Add(new DefaultConnectionItem(
                DefaultConnectionItemType.Random,
                _localizer.Get("Settings_Connection_Default_Random")));
        }

        items.Add(new DefaultConnectionItem(
            DefaultConnectionItemType.Last,
            _localizer.Get("Settings_Connection_Default_Last")));

        List<IRecentConnection> filteredRecents = _recentConnectionsManager
            .GetRecentConnections()
            .Where(r => !IsGenericFastestOrRandomConnection(r))
            .ToList();

        if (filteredRecents.Any())
        {
            items.Add(new DefaultConnectionSeparator());
        }

        items.AddRange(filteredRecents.Select(r => new RecentDefaultConnectionItem(_localizer, r)));

        Connections.Reset(items);
    }

    public object? FindSelectedItem(DefaultConnection defaultConnection)
    {
        if (defaultConnection.Type == DefaultConnectionType.Recent)
        {
            return Connections
                .OfType<RecentDefaultConnectionItem>()
                .FirstOrDefault(r => r.Recent.Id == defaultConnection.RecentId);
        }

        DefaultConnectionItemType? itemType = defaultConnection.Type switch
        {
            DefaultConnectionType.Fastest => DefaultConnectionItemType.Fastest,
            DefaultConnectionType.Random => DefaultConnectionItemType.Random,
            DefaultConnectionType.Last => DefaultConnectionItemType.Last,
            _ => null
        };

        if (itemType is null)
        {
            return null;
        }

        return Connections
            .OfType<DefaultConnectionItem>()
            .FirstOrDefault(item => item.ConnectionType == itemType);
    }

    public DefaultConnection? TryCreateDefaultConnection(object? selection)
    {
        if (selection is DefaultConnectionItem item)
        {
            return item.ConnectionType switch
            {
                DefaultConnectionItemType.Fastest => DefaultConnection.Fastest,
                DefaultConnectionItemType.Random => DefaultConnection.Random,
                DefaultConnectionItemType.Last => DefaultConnection.Last,
                _ => null
            };
        }

        if (selection is RecentDefaultConnectionItem recent)
        {
            return new DefaultConnection(recent.Recent.Id);
        }

        return null;
    }

    private static bool IsGenericFastestOrRandomConnection(IRecentConnection recent)
    {
        if (recent.ConnectionIntent is IConnectionProfile)
        {
            return false;
        }

        if (recent.ConnectionIntent.Location is not MultiCountryLocationIntent multiCountryIntent)
        {
            return false;
        }

        return (multiCountryIntent.Strategy is SelectionStrategy.Fastest or SelectionStrategy.Random)
               && recent.ConnectionIntent.Feature is null
               && multiCountryIntent is { IsToExcludeMyCountry: false, IsSelectionEmpty: true };
    }
}