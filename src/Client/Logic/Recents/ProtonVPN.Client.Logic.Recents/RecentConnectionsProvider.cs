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
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using ProtonVPN.Client.Logic.Recents.Contracts;
using ProtonVPN.Client.Logic.Recents.Contracts.Messages;
using ProtonVPN.Client.Logic.Recents.Files;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Messages;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;

namespace ProtonVPN.Client.Logic.Recents;

public class RecentConnectionsProvider : IRecentConnectionsProvider,
    IEventMessageReceiver<ConnectionStatusChanged>,
    IEventMessageReceiver<LoggedInMessage>,
    IEventMessageReceiver<LoggedOutMessage>,
    IEventMessageReceiver<ServerListChangedMessage>
{
    private const int MAXIMUM_RECENT_CONNECTIONS = 6;

    private readonly ILogger _logger;
    private readonly ISettings _settings;
    private readonly IServersLoader _serversLoader;
    private readonly IConnectionManager _connectionManager;
    private readonly IEventMessageSender _eventMessageSender;
    private readonly IRecentsFileReaderWriter _recentsFileReaderWriter;

    private readonly object _lock = new();
    private bool _areRecentsLoaded;

    private List<IRecentConnection> _recentConnections = new();

    public RecentConnectionsProvider(
        ILogger logger,
        ISettings settings,
        IServersLoader serversLoader,
        IConnectionManager connectionManager,
        IEventMessageSender eventMessageSender,
        IRecentsFileReaderWriter recentsFileReaderWriter)
    {
        _logger = logger;
        _settings = settings;
        _serversLoader = serversLoader;
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
        IRecentConnection mostRecentConnection = 
            _recentConnections.FirstOrDefault(c => c.IsActiveConnection) ?? 
            _recentConnections.FirstOrDefault();

        return mostRecentConnection == null || mostRecentConnection.IsServerUnderMaintenance 
            ? null 
            : mostRecentConnection;
    }

    public void OverrideRecentConnections(List<IConnectionIntent> connectionIntents, IConnectionIntent? mostRecentConnectionIntent = null)
    {
        lock (_lock)
        {
            _recentConnections.Clear();

            foreach (IConnectionIntent connectionIntent in connectionIntents)
            {
                TryInsertRecentConnection(connectionIntent);
            }

            foreach (IRecentConnection recentConnection in _recentConnections)
            {
                TryPinRecentConnection(recentConnection);
            }

            TryInsertRecentConnection(mostRecentConnectionIntent ?? ConnectionIntent.Default);

            SaveRecentConnections();
        }
    }

    public void Pin(IRecentConnection recentConnection)
    {
        lock (_lock)
        {
            if (TryPinRecentConnection(recentConnection))
            {
                SaveAndBroadcastRecentConnectionsChanges();
            }
        }
    }

    public void Unpin(IRecentConnection recentConnection)
    {
        lock (_lock)
        {
            if (TryUnpinRecentConnection(recentConnection))
            {
                TrimRecentConnections();
                SaveAndBroadcastRecentConnectionsChanges();
            }
        }
    }

    public void Remove(IRecentConnection recentConnection)
    {
        lock (_lock)
        {
            if (TryRemoveRecentConnection(recentConnection))
            {
                SaveAndBroadcastRecentConnectionsChanges();
            }
        }
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

                if (TryInsertRecentConnection(connectionIntent))
                {
                    TrimRecentConnections();
                }
            }
            finally
            {
                InvalidateActiveConnection();

                SaveAndBroadcastRecentConnectionsChanges();
            }
        }
    }

    public void Receive(LoggedInMessage message)
    {
        lock (_lock)
        {
            LoadRecentConnections();

            InvalidateActiveConnection();
        }

        _areRecentsLoaded = true;

        BroadcastRecentConnectionsChanges();
    }

    public void Receive(LoggedOutMessage message)
    {
        _areRecentsLoaded = false;
    }

    public void Receive(ServerListChangedMessage message)
    {
        if (_areRecentsLoaded)
        {
            lock (_lock)
            {
                InvalidateRetiredAndUnderMaintenanceServers();
            }

            SaveAndBroadcastRecentConnectionsChanges();
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

    private bool TryRemoveRecentConnection(IRecentConnection recentConnection)
    {
        if (recentConnection == null)
        {
            return false;
        }

        // The current connection cannot be removed, simply unpin it.
        if (recentConnection.IsActiveConnection)
        {
            return TryUnpinRecentConnection(recentConnection);
        }

        _recentConnections.Remove(recentConnection);

        return true;
    }

    private bool TryPinRecentConnection(IRecentConnection recentConnection)
    {
        if (recentConnection == null || recentConnection.IsPinned)
        {
            return false;
        }

        recentConnection.IsPinned = true;
        recentConnection.PinTime = DateTime.UtcNow;

        return true;
    }

    private bool TryUnpinRecentConnection(IRecentConnection recentConnection)
    {
        if (recentConnection == null || !recentConnection.IsPinned)
        {
            return false;
        }

        recentConnection.IsPinned = false;
        recentConnection.PinTime = null;

        return true;
    }

    private void InvalidateActiveConnection()
    {
        ConnectionStatus currentConnectionStatus = _connectionManager.ConnectionStatus;
        IConnectionIntent currentConnectionIntent = _connectionManager.CurrentConnectionIntent;

        foreach (IRecentConnection connection in _recentConnections)
        {
            connection.IsActiveConnection = currentConnectionStatus == ConnectionStatus.Connected
                                         && currentConnectionIntent != null
                                         && currentConnectionIntent.IsSameAs(connection.ConnectionIntent);
        }
    }

    private void InvalidateRetiredAndUnderMaintenanceServers()
    {
        List<Server> servers = _serversLoader.GetServers().ToList();
        List<IRecentConnection> recentConnections = _recentConnections.ToList();

        foreach (IRecentConnection connection in recentConnections)
        {
            if (connection.ConnectionIntent.HasNoServers(servers))
            {
                _logger.Info<AppLog>($"Recent connection {connection.ConnectionIntent} has been removed. All servers for this intent have been retired.");
                _recentConnections.Remove(connection);
                continue;
            }

            connection.IsServerUnderMaintenance = connection.ConnectionIntent.AreAllServersUnderMaintenance(servers);
        }
    }

    private void SaveAndBroadcastRecentConnectionsChanges()
    {
        SaveRecentConnections();
        BroadcastRecentConnectionsChanges();
    }

    private void LoadRecentConnections()
    {
        _recentConnections = _recentsFileReaderWriter.Read();
    }

    private void SaveRecentConnections()
    {
        _recentsFileReaderWriter.Save(_recentConnections.ToList());
    }

    private void BroadcastRecentConnectionsChanges()
    {
        _eventMessageSender.Send(new RecentConnectionsChanged());
    }

    private void TrimRecentConnections()
    {
        while (_recentConnections.Count(c => !c.IsPinned) > MAXIMUM_RECENT_CONNECTIONS)
        {
            _recentConnections.Remove(_recentConnections.Last(c => !c.IsPinned));
        }
    }
}