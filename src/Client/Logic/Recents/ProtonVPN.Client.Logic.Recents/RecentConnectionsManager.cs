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

using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.GuestHole;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using ProtonVPN.Client.Logic.Profiles.Contracts;
using ProtonVPN.Client.Logic.Profiles.Contracts.Messages;
using ProtonVPN.Client.Logic.Profiles.Contracts.Models;
using ProtonVPN.Client.Logic.Recents.Contracts;
using ProtonVPN.Client.Logic.Recents.Contracts.Messages;
using ProtonVPN.Client.Logic.Recents.Files;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Messages;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Enums;
using ProtonVPN.Client.Settings.Contracts.Models;
using ProtonVPN.Common.Core.Geographical;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;

namespace ProtonVPN.Client.Logic.Recents;

public class RecentConnectionsManager : IRecentConnectionsManager,
    IEventMessageReceiver<ConnectionStatusChangedMessage>,
    IEventMessageReceiver<LoggedInMessage>,
    IEventMessageReceiver<LoggedOutMessage>,
    IEventMessageReceiver<ServerListChangedMessage>,
    IEventMessageReceiver<ProfilesChangedMessage>
{
    private const int MAXIMUM_RECENT_CONNECTIONS = 6;

    private readonly ILogger _logger;
    private readonly ISettings _settings;
    private readonly IServersLoader _serversLoader;
    private readonly IProfilesManager _profilesManager;
    private readonly IConnectionManager _connectionManager;
    private readonly IEventMessageSender _eventMessageSender;
    private readonly IGuestHoleManager _guestHoleManager;
    private readonly IRecentsFileReaderWriter _recentsFileReaderWriter;

    private readonly object _lock = new();

    private List<IRecentConnection> _recentConnections = [];

    private bool _areRecentsLoaded;

    public RecentConnectionsManager(
        ILogger logger,
        ISettings settings,
        IServersLoader serversLoader,
        IProfilesManager profilesManager,
        IConnectionManager connectionManager,
        IEventMessageSender eventMessageSender,
        IGuestHoleManager guestHoleManager,
        IRecentsFileReaderWriter recentsFileReaderWriter)
    {
        _logger = logger;
        _settings = settings;
        _serversLoader = serversLoader;
        _profilesManager = profilesManager;
        _connectionManager = connectionManager;
        _eventMessageSender = eventMessageSender;
        _guestHoleManager = guestHoleManager;
        _recentsFileReaderWriter = recentsFileReaderWriter;
    }

    public bool HasAnyRecentConnections()
    {
        return _recentConnections.Any();
    }

    public IOrderedEnumerable<IRecentConnection> GetRecentConnections()
    {
        return _recentConnections.OrderByDescending(c => c.IsPinned)
                                 .ThenBy(c => c.PinTime);
    }

    public IRecentConnection? GetMostRecentConnection()
    {
        List<Server> servers = _serversLoader.GetServers().ToList();
        DeviceLocation? deviceLocation = _settings.DeviceLocation;

        IRecentConnection? mostRecentConnection = _recentConnections
            .FirstOrDefault(c => !c.ConnectionIntent.AreAllServersUnderMaintenance(servers, deviceLocation));

        return mostRecentConnection;
    }

    public IConnectionIntent GetDefaultConnection()
    {
        if (!_settings.VpnPlan.IsPaid)
        {
            return ConnectionIntent.FreeDefault;
        }

        DefaultConnection defaultConnection = _settings.DefaultConnection;
        IConnectionIntent? mostRecentConnectionIntent = GetMostRecentConnection()?.ConnectionIntent;
        Gateway? gateway = _serversLoader.GetGateways().FirstOrDefault();

        if ((defaultConnection == DefaultConnection.Fastest || (defaultConnection == DefaultConnection.Last && mostRecentConnectionIntent is null)) &&
            gateway is not null && !_serversLoader.HasAnyCountries())
        {
            return new ConnectionIntent(new GatewayLocationIntent(gateway.Name), new B2BFeatureIntent());
        }

        return defaultConnection.Type switch
        {
            DefaultConnectionType.Recent => GetById(defaultConnection.RecentId)?.ConnectionIntent ?? ConnectionIntent.Default,
            DefaultConnectionType.Last => mostRecentConnectionIntent ?? ConnectionIntent.Default,
            DefaultConnectionType.Random => new ConnectionIntent(CountryLocationIntent.Random),
            _ => ConnectionIntent.Default
        };
    }

    public IRecentConnection? GetById(Guid id)
    {
        return _recentConnections.FirstOrDefault(c => c.Id == id);
    }

    public void OverrideRecentConnections(List<IConnectionIntent> connectionIntents, IConnectionIntent? quickConnectionIntent = null)
    {
        lock (_lock)
        {
            _recentConnections.Clear();

            foreach (IConnectionIntent connectionIntent in connectionIntents)
            {
                TryInsertRecentConnection(connectionIntent);
            }

            if (quickConnectionIntent is not null)
            {
                TryInsertRecentConnection(quickConnectionIntent);
                SetAsDefaultConnection(GetMostRecentConnection()?.Id);
            }

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

    public void SetAsDefaultConnection(Guid? recentConnectionId)
    {
        _settings.DefaultConnection = recentConnectionId != null 
            ? new DefaultConnection(recentConnectionId.Value)
            : DefaultSettings.DefaultConnection;
    }

    public void Receive(LoggedInMessage message)
    {
        lock (_lock)
        {
            LoadRecentConnections();

            UpdateCurrentConnectionIntent();
        }

        _areRecentsLoaded = true;

        BroadcastRecentConnectionsChanges();
    }

    public void Receive(LoggedOutMessage message)
    {
        _areRecentsLoaded = false;
    }

    public void Receive(ConnectionStatusChangedMessage message)
    {
        if (_areRecentsLoaded && !_guestHoleManager.IsActive && (message?.ConnectionStatus) == ConnectionStatus.Connecting)
        {
            lock (_lock)
            {
                IConnectionIntent? connectionIntent = _connectionManager.CurrentConnectionIntent;

                if (TryInsertRecentConnection(connectionIntent))
                {
                    TrimRecentConnections();
                    SaveAndBroadcastRecentConnectionsChanges();
                }
            }
        }
    }

    public void Receive(ServerListChangedMessage message)
    {
        if (_areRecentsLoaded)
        {
            lock (_lock)
            {
                if (TryInvalidateRetiredServers())
                {
                    SaveAndBroadcastRecentConnectionsChanges();
                }
            }
        }
    }

    public void Receive(ProfilesChangedMessage message)
    {
        if (_areRecentsLoaded)
        {
            lock (_lock)
            {
                if (TryInvalidateRecentProfiles() || TryInvalidateRetiredServers())
                {
                    SaveAndBroadcastRecentConnectionsChanges();
                }
            }
        }
    }

    private void UpdateCurrentConnectionIntent()
    {
        // If the client is launched after a crash and there was previously an active VPN connection,
        // we initialize the connection manager with the last connection intent from the recent active connection.
        // This ensures that when the connection manager receives the connected state, it knows the last user intent
        // and can therefore display the correct connection info in the connection details panel.
        IRecentConnection? recentConnection = GetMostRecentConnection();
        _connectionManager.InitializeAsync(recentConnection?.ConnectionIntent);
    }

    private bool TryInsertRecentConnection(IConnectionIntent? recentIntent)
    {
        if (recentIntent == null || recentIntent.Location is FreeServerLocationIntent)
        {
            return false;
        }

        List<IRecentConnection> duplicates = _recentConnections.Where(c => c.ConnectionIntent.IsSameAs(recentIntent)).ToList();

        foreach (IRecentConnection duplicate in duplicates)
        {
            // Remove duplicated intent, so it can be inserted at the top of the list
            _recentConnections.Remove(duplicate);
        }

        _recentConnections.Insert(0, duplicates.FirstOrDefault() ?? new RecentConnection(Guid.NewGuid(), recentIntent));

        return true;
    }

    private bool TryRemoveRecentConnection(IRecentConnection recentConnection)
    {
        if (recentConnection == null)
        {
            return false;
        }

        if (recentConnection.Id == _settings.DefaultConnection.RecentId)
        {
            _settings.DefaultConnection = DefaultSettings.DefaultConnection;
            _logger.Info<AppLog>("Removing recent which is set as a current default connection.");
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

    private bool TryInvalidateRecentProfiles()
    {
        bool hasRecentListBeenModified = false;

        List<IConnectionProfile> profiles = _profilesManager.GetAll().ToList();
        List<IRecentConnection> recentConnections = _recentConnections.ToList();

        foreach (IRecentConnection connection in recentConnections.Where(c => c.ConnectionIntent is IConnectionProfile))
        {
            IConnectionProfile? profile = profiles.FirstOrDefault(p => p.IsSameAs(connection.ConnectionIntent));
            if (profile != null)
            {
                // Profile may have changed, update the recent connection item
                connection.ConnectionIntent = profile;
            }
            else
            {
                // Profile no longer exists, remove it from recents
                _logger.Info<AppLog>($"Recent connection {connection.ConnectionIntent} has been removed because the profile has been deleted");
                _recentConnections.Remove(connection);
            }
            hasRecentListBeenModified = true;
        }

        return hasRecentListBeenModified;
    }

    private bool TryInvalidateRetiredServers()
    {
        bool hasRecentListBeenModified = false;
        bool hasGatewaysOnly = _serversLoader.HasAnyGateways() && !_serversLoader.HasAnyCountries();

        List<Server> servers = _serversLoader.GetServers().ToList();
        List<IRecentConnection> recentConnections = _recentConnections.ToList();

        foreach (IRecentConnection connection in recentConnections)
        {
            bool hasNoServers = connection.ConnectionIntent is not IConnectionProfile &&
                connection.ConnectionIntent.HasNoServers(servers, _settings.DeviceLocation);
            bool isFastestCountryIntent = connection.ConnectionIntent.Location is CountryLocationIntent cli && cli.IsFastestCountry;

            if (hasNoServers || (hasGatewaysOnly && isFastestCountryIntent))
            {
                _logger.Info<AppLog>($"Recent connection {connection.ConnectionIntent} has been removed. All servers for this intent have been retired.");
                _recentConnections.Remove(connection);

                hasRecentListBeenModified = true;
            }
        }

        return hasRecentListBeenModified;
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
        _eventMessageSender.Send(new RecentConnectionsChangedMessage());
    }

    private void TrimRecentConnections()
    {
        while (_recentConnections.Count(c => !IsRecentInUseOrPinned(c)) > MAXIMUM_RECENT_CONNECTIONS)
        {
            _recentConnections.Remove(_recentConnections.Last(c => !IsRecentInUseOrPinned(c)));
        }
    }

    private bool IsRecentInUseOrPinned(IRecentConnection recent)
    {
        return recent.IsPinned || recent.Id == _settings.DefaultConnection.RecentId;
    }
}