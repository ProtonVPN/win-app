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
using CommunityToolkit.Mvvm.ComponentModel;
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Recents.Contracts;
using ProtonVPN.Client.Logic.Recents.Contracts.Messages;

namespace ProtonVPN.Client.UI.Home.Recents;

public partial class RecentsViewModel : ViewModelBase, IEventMessageReceiver<RecentConnectionsChanged>
{
    private readonly IRecentConnectionsProvider _recentConnectionsProvider;
    private readonly IConnectionManager _connectionManager;

    [ObservableProperty]
    private bool _isRecentsComponentOpened;

    public ObservableCollection<RecentItemViewModel> RecentConnections { get; } = new();

    public bool HasRecentConnections => RecentConnections.Any();

    public RecentsViewModel(IRecentConnectionsProvider recentConnectionsProvider,
        IConnectionManager connectionManager,
        ILocalizationProvider localizationProvider)
        : base(localizationProvider)
    {
        _recentConnectionsProvider = recentConnectionsProvider;
        _connectionManager = connectionManager;

        InvalidateRecentConnections();
    }

    public void Receive(RecentConnectionsChanged message)
    {
        InvalidateRecentConnections();
    }

    public void InvalidateRecentConnections()
    {
        RecentConnections.Clear();

        IRecentConnection? mostRecentConnection = _recentConnectionsProvider.GetMostRecentConnection();

        foreach (IRecentConnection recentConnection in _recentConnectionsProvider.GetRecentConnections())
        {
            // Most recent connection will be displayed on the connection card instead (unless it is pinned)
            if (mostRecentConnection != null && !mostRecentConnection.IsPinned && recentConnection == mostRecentConnection)
            {
                continue;
            }

            RecentConnections.Add(new RecentItemViewModel(_connectionManager, _recentConnectionsProvider, recentConnection, Localizer));
        }

        OnPropertyChanged(nameof(HasRecentConnections));
    }
}