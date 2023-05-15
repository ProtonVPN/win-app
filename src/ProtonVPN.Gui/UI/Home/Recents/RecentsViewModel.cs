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
using CommunityToolkit.Mvvm.Messaging;
using ProtonVPN.Connection.Contracts;
using ProtonVPN.Gui.Contracts.ViewModels;
using ProtonVPN.Recents.Contracts;
using ProtonVPN.Recents.Contracts.Messages;

namespace ProtonVPN.Gui.UI.Home.Recents;

public partial class RecentsViewModel : ViewModelBase, IRecipient<RecentConnectionsChanged>
{
    private readonly IRecentConnectionsProvider _recentConnectionsProvider;
    private readonly IConnectionService _connectionService;

    [ObservableProperty]
    private bool _isRecentsComponentOpened;

    public ObservableCollection<RecentItemViewModel> RecentConnections { get; } = new();

    public bool HasRecentConnections => RecentConnections.Any();

    public RecentsViewModel(IRecentConnectionsProvider recentConnectionsProvider, IConnectionService connectionService)
    {
        Messenger.RegisterAll(this);

        _recentConnectionsProvider = recentConnectionsProvider;
        _connectionService = connectionService;

        InvalidateRecentConnections();
    }

    public void Receive(RecentConnectionsChanged message)
    {
        InvalidateRecentConnections();
    }

    public void InvalidateRecentConnections()
    {
        RecentConnections.Clear();

        foreach (IRecentConnection recentConnection in _recentConnectionsProvider.GetRecentConnections())
        {
            RecentConnections.Add(new RecentItemViewModel(_connectionService, _recentConnectionsProvider, recentConnection));
        }

        OnPropertyChanged(nameof(HasRecentConnections));
    }
}