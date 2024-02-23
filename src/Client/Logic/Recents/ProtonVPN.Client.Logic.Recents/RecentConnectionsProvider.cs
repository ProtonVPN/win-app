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
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Logic.Connection.Contracts.Models;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using ProtonVPN.Client.Logic.Recents.Contracts;
using ProtonVPN.Client.Logic.Recents.Contracts.Messages;
using ProtonVPN.Client.Logic.Recents.Files;

namespace ProtonVPN.Client.Logic.Recents;

public class RecentConnectionsProvider : IRecentConnectionsProvider,
    IEventMessageReceiver<ConnectionStatusChanged>,
    IEventMessageReceiver<LoggedInMessage>
{
    private const int MAXIMUM_RECENT_CONNECTIONS = 6;

    private readonly IConnectionManager _connectionManager;
    private readonly IEventMessageSender _eventMessageSender;
    private readonly IRecentsFileReaderWriter _recentsFileReaderWriter;

    private readonly object _lock = new();

    private List<IRecentConnection> _recentConnections = new();

    public RecentConnectionsProvider(
        IConnectionManager connectionManager,
        IEventMessageSender eventMessageSender,
        IRecentsFileReaderWriter recentsFileReaderWriter)
    {
        _connectionManager = connectionManager;
        _eventMessageSender = eventMessageSender;
        _recentsFileReaderWriter = recentsFileReaderWriter;
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

        SaveRecentsAndBroadcastChanges();
    }

    private void SaveRecentsAndBroadcastChanges()
    {
        SaveRecentsToFile();
        BroadcastRecentConnectionsChanged();
    }

    private void SaveRecentsToFile()
    {
        List<IRecentConnection> recentConnections = _recentConnections.ToList();
        Task.Run(() => { _recentsFileReaderWriter.Save(recentConnections); }).ConfigureAwait(false);
    }

    private void BroadcastRecentConnectionsChanged()
    {
        _eventMessageSender.Send(new RecentConnectionsChanged());
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

        SaveRecentsAndBroadcastChanges();
    }

    public void Remove(IRecentConnection recentConnection)
    {
        if (recentConnection == null)
        {
            return;
        }

        ConnectionDetails? connectionDetails = _connectionManager.CurrentConnectionDetails;

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

        SaveRecentsAndBroadcastChanges();
    }

    public void Receive(ConnectionStatusChanged message)
    {
        lock (_lock)
        {
            IConnectionIntent connectionIntent = _connectionManager.CurrentConnectionIntent;

            try
            {
                if (message?.ConnectionStatus != ConnectionStatus.Connecting)
                {
                    return;
                }

                if (!TryInsertRecentConnection(connectionIntent))
                {
                    return;
                }

                TrimRecentConnections();
            }
            finally
            {
                SetActiveConnection(connectionIntent, _connectionManager.ConnectionStatus);

                SaveRecentsAndBroadcastChanges();
            }
        }
    }

    private bool TryInsertRecentConnection(IConnectionIntent recentIntent)
    {
        if (recentIntent == null || recentIntent.Location is FreeServerLocationIntent)
        {
            return false;
        }

        IRecentConnection? duplicate = _recentConnections.SingleOrDefault(c => c.ConnectionIntent.IsSameAs(recentIntent));
        if (duplicate != null)
        {
            // Remove duplicated intent, so it can be inserted at the top of the list
            _recentConnections.Remove(duplicate);
        }

        _recentConnections.Insert(0, duplicate ?? new RecentConnection(recentIntent));

        return true;
    }

    private void TrimRecentConnections()
    {
        while (_recentConnections.Count(c => !c.IsPinned) > MAXIMUM_RECENT_CONNECTIONS)
        {
            _recentConnections.Remove(_recentConnections.Last(c => !c.IsPinned));
        }
    }

    private void SetActiveConnection(IConnectionIntent activeIntent, ConnectionStatus connectionStatus)
    {
        foreach (IRecentConnection connection in _recentConnections)
        {
            connection.IsActiveConnection = activeIntent != null
                                            && connectionStatus == ConnectionStatus.Connected
                                            && activeIntent.IsSameAs(connection.ConnectionIntent);
        }
    }

    public void Receive(LoggedInMessage message)
    {
        lock (_lock)
        {
            _recentConnections = _recentsFileReaderWriter.Read();
        }

        BroadcastRecentConnectionsChanged();
    }

    public void SaveRecentConnections(List<IConnectionIntent> connectionIntents, IConnectionIntent? recentConnectionIntent = null)
    {
        List<IRecentConnection> recentConnections = [];
        foreach (IConnectionIntent connectionIntent in connectionIntents)
        {
            if (!recentConnections.Any(c => c.ConnectionIntent.IsSameAs(connectionIntent)))
            {
                InsertRecentConnection(recentConnections, connectionIntent);
            }
        }

        if (recentConnectionIntent is not null)
        {
            IRecentConnection? duplicate = recentConnections.SingleOrDefault(c => c.ConnectionIntent.IsSameAs(recentConnectionIntent));
            if (duplicate != null)
            {
                recentConnections.Remove(duplicate);
            }

            InsertRecentConnection(recentConnections, recentConnectionIntent);
        }

        if (recentConnections.Count > 0)
        {
            lock (_lock)
            {
                _recentConnections = recentConnections;
            }
            SaveRecentsAndBroadcastChanges();
        }
    }

    private void InsertRecentConnection(List<IRecentConnection> recentConnections, IConnectionIntent connectionIntent)
    {
        recentConnections.Insert(0, new RecentConnection(connectionIntent) { IsPinned = true });
    }
}