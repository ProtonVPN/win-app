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

using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Logic.Connection.Contracts.Models;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using ProtonVPN.Client.Logic.Recents.Contracts;
using ProtonVPN.Client.Logic.Recents.Contracts.Messages;

namespace ProtonVPN.Client.Logic.Recents;

public class RecentConnectionsProvider : IRecentConnectionsProvider, IEventMessageReceiver<ConnectionStatusChanged>
{
    private const int MAXIMUM_RECENT_CONNECTIONS = 6;

    private readonly IConnectionManager _connectionManager;
    private readonly IEventMessageSender _eventMessageSender;
    private readonly object _lock = new();

    private List<IRecentConnection> _recentConnections;

    public RecentConnectionsProvider(IConnectionManager connectionManager,
        IEventMessageSender eventMessageSender)
    {
        _connectionManager = connectionManager;
        _eventMessageSender = eventMessageSender;
        _recentConnections = new List<IRecentConnection>();
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
        if (recentConnection == null || recentConnection.IsPinned)
        {
            return;
        }

        lock (_lock)
        {
            recentConnection.IsPinned = true;
            recentConnection.PinTime = DateTime.UtcNow;
        }

        BroadcastRecentConnectionsChanged();
    }

    public void Unpin(IRecentConnection recentConnection)
    {
        if (recentConnection == null || !recentConnection.IsPinned)
        {
            return;
        }

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
        if (recentConnection == null)
        {
            return;
        }

        ConnectionDetails? connectionDetails = _connectionManager.GetConnectionDetails();

        // The current connection cannot be removed, simply unpin it.
        if (connectionDetails != null && recentConnection.ConnectionIntent.IsSameAs(connectionDetails.OriginalConnectionIntent))
        {
            Unpin(recentConnection);
            return;
        }

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
            ConnectionDetails? connectionDetails = _connectionManager.GetConnectionDetails();

            try
            {
                if (message?.ConnectionStatus != ConnectionStatus.Connecting)
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
                SetActiveConnection(connectionDetails?.OriginalConnectionIntent, _connectionManager.ConnectionStatus);

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

        // TODO: TEMPORARY - Simulate server under maintenance when connecting to GB (remove once properly implemented)
        if (recentConnection.ConnectionIntent.Location is CountryLocationIntent countryIntent && countryIntent.CountryCode == "GB")
        {
            recentConnection.IsServerUnderMaintenance = true;
        }

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
        _eventMessageSender.Send(new RecentConnectionsChanged());
    }
}