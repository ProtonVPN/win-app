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

using CommunityToolkit.Mvvm.Messaging;
using ProtonVPN.Client.Common.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Logic.Connection.Contracts.Models;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents;
using ProtonVPN.Client.Logic.Recents.Contracts;
using ProtonVPN.Client.Logic.Recents.Contracts.Messages;

namespace ProtonVPN.Client.Logic.Recents;

public class RecentConnectionsProvider : ServiceRecipient, IRecentConnectionsProvider, IRecipient<ConnectionStatusChanged>
{
    private const int MAXIMUM_RECENT_CONNECTIONS = 6;

    private readonly IConnectionService _connectionService;
    private readonly object _lock = new();
    private List<IRecentConnection> _recentConnections;

    private Queue<IRecentConnection> _recentConnectionsQueue;

    public RecentConnectionsProvider(IConnectionService connectionService)
    {
        _connectionService = connectionService;

        _recentConnections = new List<IRecentConnection>();
        //{
        //    new RecentConnection(new ConnectionIntent(new CityStateLocationIntent("AU", "Sydney"))) { IsPinned = true },
        //    new RecentConnection(new ConnectionIntent(new ServerLocationIntent("CH", "Zurich", 30))),
        //    new RecentConnection(new ConnectionIntent(new CountryLocationIntent("LT"), new SecureCoreFeatureIntent("SE"))),
        //    new RecentConnection(new ConnectionIntent(new CountryLocationIntent("CA")))  { IsPinned = true },
        //    new RecentConnection(new ConnectionIntent(new CityStateLocationIntent("US", "Los Angeles"))) ,
        //};
    }

    public IOrderedEnumerable<IRecentConnection> GetRecentConnections()
    {
        return _recentConnections.OrderByDescending(c => c.IsPinned)
                                 .ThenBy(c => c.PinTime);
    }

    public IRecentConnection? GetMostRecentConnection()
    {
        return _recentConnections.FirstOrDefault(c => c.IsActiveConnection)
            ?? _recentConnections.FirstOrDefault();
    }

    public void Pin(IRecentConnection recentConnection)
    {
        lock (_lock)
        {
            recentConnection.IsPinned = true;
            recentConnection.PinTime = DateTime.UtcNow;
        }

        BroadcastRecentConnectionsChanged();
    }

    public void Unpin(IRecentConnection recentConnection)
    {
        lock (_lock)
        {
            recentConnection.IsPinned = false;
            recentConnection.PinTime = null;

            TrimRecentConnections();
        }

        BroadcastRecentConnectionsChanged();
    }

    public void Remove(IRecentConnection recentConnection)
    {
        lock (_lock)
        {
            _recentConnections.Remove(recentConnection);
        }

        BroadcastRecentConnectionsChanged();
    }

    public void Receive(ConnectionStatusChanged message)
    {
        lock (_lock)
        {
            ConnectionDetails? connectionDetails = _connectionService.GetConnectionDetails();

            try
            {
                if (message?.Value != ConnectionStatus.Connecting)
                {
                    return;
                }

                if (!TryInsertRecentConnection(connectionDetails?.OriginalConnectionIntent))
                {
                    return;
                }

                TrimRecentConnections();
            }
            finally
            {
                SetActiveConnection(connectionDetails?.OriginalConnectionIntent, _connectionService.ConnectionStatus);

                BroadcastRecentConnectionsChanged();
            }
        }
    }

    private bool TryInsertRecentConnection(IConnectionIntent? recentIntent)
    {
        if (recentIntent == null)
        {
            return false;
        }

        IRecentConnection? duplicate = _recentConnections.SingleOrDefault(c => c.ConnectionIntent.IsSameAs(recentIntent));
        if (duplicate != null)
        {
            // Remove duplicated intent so it can be inserted at the top of the list
            _recentConnections.Remove(duplicate);
        }

        IRecentConnection recentConnection = duplicate ?? new RecentConnection(recentIntent);

        _recentConnections.Insert(0, recentConnection);

        return true;
    }

    private void TrimRecentConnections()
    {
        while (_recentConnections.Count(c => !c.IsPinned) > MAXIMUM_RECENT_CONNECTIONS)
        {
            _recentConnections.Remove(_recentConnections.Last(c => !c.IsPinned));
        }
    }

    private void SetActiveConnection(IConnectionIntent? activeIntent, ConnectionStatus connectionStatus)
    {
        foreach (IRecentConnection connection in _recentConnections)
        {
            connection.IsActiveConnection = activeIntent != null
                                         && connectionStatus == ConnectionStatus.Connected
                                         && activeIntent.IsSameAs(connection.ConnectionIntent);
        }
    }

    private void BroadcastRecentConnectionsChanged()
    {
        Messenger.Send(new RecentConnectionsChanged());
    }
}