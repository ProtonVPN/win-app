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
using ProtonVPN.Common.Core.Contracts;
using ProtonVPN.Connection.Contracts;
using ProtonVPN.Connection.Contracts.Enums;
using ProtonVPN.Connection.Contracts.Messages;
using ProtonVPN.Connection.Contracts.Models;
using ProtonVPN.Recents.Contracts;
using ProtonVPN.Recents.Contracts.Messages;

namespace ProtonVPN.Recents;

public class RecentConnectionsProvider : ServiceRecipient, IRecentConnectionsProvider, IRecipient<ConnectionStatusChanged>
{
    private const int MAXIMUM_RECENT_CONNECTIONS = 5;

    private readonly IConnectionService _connectionService;
    private readonly object _lock = new();
    private List<IRecentConnection> _recentConnections;

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
        return _recentConnections.OrderByDescending(c => c.IsPinned);
    }

    public IRecentConnection? GetMostRecentConnection()
    {
        return _recentConnections.FirstOrDefault();
    }

    public void Receive(ConnectionStatusChanged message)
    {
        lock (_lock)
        {
            // Update the recent connections when a new connection is attempted

            if (message?.Value is null || message.Value != ConnectionStatus.Connecting)
            {
                return;
            }

            ConnectionDetails? connectionDetails = _connectionService.GetConnectionDetails();

            // TODO: Check if this intent is already in the recent connections.

            if (connectionDetails?.OriginalConnectionIntent is null)
            {
                throw new InvalidOperationException("The connection details cannot be null if the connection status is not disconnected");
            }

            _recentConnections.Insert(0, new RecentConnection(connectionDetails.OriginalConnectionIntent));

            while (_recentConnections.Count(c => !c.IsPinned) > MAXIMUM_RECENT_CONNECTIONS)
            {
                IRecentConnection lastRecentConnection = _recentConnections.Last(c => !c.IsPinned);
                _recentConnections.Remove(lastRecentConnection);
            }
        }

        BroadcastRecentConnectionsChanged();
    }

    public void Pin(IRecentConnection recentConnection)
    {
        lock (_lock)
        {
            _recentConnections.Remove(recentConnection);

            recentConnection.IsPinned = true;

            _recentConnections.Insert(0, recentConnection);
        }

        BroadcastRecentConnectionsChanged();
    }

    public void Unpin(IRecentConnection recentConnection)
    {
        lock (_lock)
        {
            _recentConnections.Remove(recentConnection);

            recentConnection.IsPinned = false;

            _recentConnections.Insert(0, recentConnection);
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

    private void BroadcastRecentConnectionsChanged()
    {
        Messenger.Send(new RecentConnectionsChanged());
    }
}