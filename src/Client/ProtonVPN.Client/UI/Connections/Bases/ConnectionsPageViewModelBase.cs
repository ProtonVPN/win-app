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

using Microsoft.UI.Xaml.Data;
using ProtonVPN.Client.Common.Collections;
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Logic.Connection.Contracts.Models;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Messages;
using ProtonVPN.Client.Logic.Users.Contracts.Messages;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.UI.Connections.Common.Factories;
using ProtonVPN.Client.UI.Connections.Common.Items;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Connections.Bases;

public abstract class ConnectionsPageViewModelBase : PageViewModelBase<IMainViewNavigator>,
    IEventMessageReceiver<ConnectionStatusChanged>,
    IEventMessageReceiver<ServerListChangedMessage>,
    IEventMessageReceiver<VpnPlanChangedMessage>,
    IEventMessageReceiver<LoggedInMessage>
{
    protected readonly IServersLoader ServersLoader;
    protected readonly IConnectionManager ConnectionManager;
    protected readonly ISettings Settings;

    protected readonly LocationItemFactory LocationItemFactory;

    private readonly SmartObservableCollection<LocationItemBase> _locationItems = [];

    private readonly SmartObservableCollection<LocationGroup> _locationGroups = [];

    public CollectionViewSource LocationGroupsCvs { get; }

    public bool HasItems => LocationGroupsCvs.View.Any();

    protected ConnectionsPageViewModelBase(
        IMainViewNavigator viewNavigator,
        ILocalizationProvider localizationProvider,
        ILogger logger,
        IIssueReporter issueReporter,
        LocationItemFactory locationItemFactory,
        IServersLoader serversLoader,
        IConnectionManager connectionManager,
        ISettings settings)
        : base(viewNavigator, localizationProvider, logger, issueReporter)
    {
        LocationItemFactory = locationItemFactory;
        ServersLoader = serversLoader;
        ConnectionManager = connectionManager;
        Settings = settings;

        LocationGroupsCvs = new()
        {
            Source = _locationGroups,
            IsSourceGrouped = true
        };

        FetchItems();
    }

    protected override void OnActivated()
    {
        InvalidateActiveConnection();
        InvalidateRestrictions();
    }

    public void Receive(ConnectionStatusChanged message)
    {
        ExecuteOnUIThread(InvalidateActiveConnection);
    }

    public void Receive(ServerListChangedMessage message)
    {
        ExecuteOnUIThread(FetchItems);
    }

    public void Receive(VpnPlanChangedMessage message)
    {
        ExecuteOnUIThread(InvalidateRestrictions);
    }

    public void Receive(LoggedInMessage message)
    {
        ExecuteOnUIThread(InvalidateRestrictions);
    }

    protected abstract IEnumerable<LocationItemBase> GetItems();

    protected virtual IEnumerable<LocationItemBase> FilterItems(IEnumerable<LocationItemBase> items)
    {
        // By default, no filters applied
        return items;
    }

    protected void FetchItems()
    {
        _locationItems.Reset(
            GetItems()
                .OrderBy(item => item.GroupType)
                .ThenBy(item => item.FirstSortProperty)
                .ThenBy(item => item.SecondSortProperty));

        GroupItems();

        InvalidateActiveConnection();
        InvalidateRestrictions();
    }

    protected void GroupItems()
    {
        _locationGroups.Reset(
            FilterItems(_locationItems)
                .GroupBy(item => item.GroupType)
                .Select(group => LocationItemFactory.GetGroup(group.Key, group)));

        OnPropertyChanged(nameof(HasItems));
    }

    protected void InvalidateActiveConnection()
    {
        if (IsActive)
        {
            ConnectionDetails? connectionDetails = ConnectionManager.CurrentConnectionDetails;

            foreach (LocationItemBase item in _locationItems)
            {
                item.InvalidateIsActiveConnection(connectionDetails);
            }
        }
    }

    protected virtual void InvalidateRestrictions()
    {
        if (IsActive)
        {
            bool isPaidUser = Settings.VpnPlan.IsPaid;

            foreach (LocationItemBase item in _locationItems)
            {
                item.InvalidateIsRestricted(isPaidUser);
            }
        }
    }
}