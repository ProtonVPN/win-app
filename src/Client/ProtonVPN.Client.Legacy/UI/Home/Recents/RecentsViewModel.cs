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

using System.Collections.ObjectModel;
using ProtonVPN.Client.Legacy.Contracts.ViewModels;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using ProtonVPN.Client.Logic.Profiles.Contracts;
using ProtonVPN.Client.Logic.Recents.Contracts;
using ProtonVPN.Client.Logic.Recents.Contracts.Messages;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Enums;
using ProtonVPN.Client.Settings.Contracts.Messages;
using ProtonVPN.Client.Settings.Contracts.Models;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.Legacy.UI.Home.Recents;

public partial class RecentsViewModel : ViewModelBase,
    IEventMessageReceiver<RecentConnectionsChanged>,
    IEventMessageReceiver<LoggedInMessage>,
    IEventMessageReceiver<SettingChangedMessage>
{
    private readonly IRecentConnectionsManager _recentConnectionsManager;
    private readonly IConnectionManager _connectionManager;
    private readonly ISettings _settings;
    private readonly IProfilesManager _profilesManager;

    public bool IsRecentsComponentOpened
    {
        get => _settings.IsRecentsPaneOpened;
        set => _settings.IsRecentsPaneOpened = value;
    }

    public ObservableCollection<RecentItem> RecentConnections { get; } = new();

    public bool HasRecentConnections => RecentConnections.Any();

    public RecentsViewModel(IRecentConnectionsManager recentConnectionsManager,
        IConnectionManager connectionManager,
        ILocalizationProvider localizationProvider,
        ILogger logger,
        IIssueReporter issueReporter,
        ISettings settings,
        IProfilesManager profilesManager)
        : base(localizationProvider, logger, issueReporter)
    {
        _recentConnectionsManager = recentConnectionsManager;
        _connectionManager = connectionManager;
        _settings = settings;
        _profilesManager = profilesManager;

        InvalidateRecentConnections();
    }

    public void Receive(RecentConnectionsChanged message)
    {
        ExecuteOnUIThread(InvalidateRecentConnections);
    }

    public void Receive(LoggedInMessage message)
    {
        ExecuteOnUIThread(() => OnPropertyChanged(nameof(IsRecentsComponentOpened)));
    }

    public void Receive(SettingChangedMessage message)
    {
        ExecuteOnUIThread(() =>
        {
            switch (message.PropertyName)
            {
                case nameof(ISettings.DefaultConnection):
                    InvalidateRecentConnections();
                    break;

                case nameof(ISettings.IsRecentsPaneOpened):
                    OnPropertyChanged(nameof(IsRecentsComponentOpened));
                    break;
            }
        });
    }

    public void InvalidateRecentConnections()
    {
        RecentConnections.Clear();

        IRecentConnection? mostRecentConnection = _recentConnectionsManager.GetMostRecentConnection();
        DefaultConnection defaultConnection = _settings.DefaultConnection;

        foreach (IRecentConnection recentConnection in _recentConnectionsManager.GetRecentConnections())
        {
            if (defaultConnection.Type == DefaultConnectionType.Last)
            {
                // Most recent connection will be displayed on the connection card instead (unless it is pinned)
                // We also want to show all recents in case the user upgraded to a paid plan and is still connected to a free server.
                // In this case the connection card displays free server, but we want to display all recents below.
                bool isMostRecentConnection = mostRecentConnection != null && recentConnection == mostRecentConnection;
                bool isFreeServerIntent = _connectionManager.CurrentConnectionDetails?.OriginalConnectionIntent.Location is FreeServerLocationIntent;
                if (isMostRecentConnection && !recentConnection.IsPinned && !isFreeServerIntent)
                {
                    continue;
                }
            }

            RecentConnections.Add(
                new RecentItem(
                    _connectionManager,
                    _recentConnectionsManager,
                    recentConnection,
                    Localizer,
                    _profilesManager));
        }

        OnPropertyChanged(nameof(HasRecentConnections));
    }
}