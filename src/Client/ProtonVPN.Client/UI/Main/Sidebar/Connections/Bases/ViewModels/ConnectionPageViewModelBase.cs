/*
 * Copyright (c) 2024 Proton AG
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

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using ProtonVPN.Client.Common.Collections;
using ProtonVPN.Client.Contracts.Bases.ViewModels;
using ProtonVPN.Client.Contracts.Services.Navigation;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Factories;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Logic.Connection.Contracts.Models;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Messages;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.Client.Logic.Users.Contracts.Messages;
using ProtonVPN.Client.Models.Connections;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Models;
using ProtonVPN.Client.UI.Main.Sidebar.Connections.Bases.Contracts;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Main.Sidebar.Connections.Bases.ViewModels;

public abstract class ConnectionPageViewModelBase : PageViewModelBase<IConnectionsViewNavigator>, IConnectionPage,
    IEventMessageReceiver<ConnectionStatusChanged>,
    IEventMessageReceiver<VpnPlanChangedMessage>,
    IEventMessageReceiver<LoggedInMessage>,
    IEventMessageReceiver<ServerListChangedMessage>
{
    protected readonly ISettings Settings;
    protected readonly IServersLoader ServersLoader;
    protected readonly IConnectionGroupFactory ConnectionGroupFactory;
    protected readonly IConnectionManager ConnectionManager;

    private readonly SmartObservableCollection<ConnectionItemBase> _items = [];
    private readonly SmartObservableCollection<ConnectionGroup> _groups = [];

    public abstract int SortIndex { get; }

    public abstract string Header { get; }

    public string ShortcutText => $"ctrl + {SortIndex}";

    public abstract IconElement Icon { get; }

    public virtual bool IsAvailable => true;

    public CollectionViewSource GroupsCvs { get; }

    public bool HasItems => GroupsCvs.View.Any();

    protected ConnectionPageViewModelBase(
        IConnectionsViewNavigator parentViewNavigator,
        ILocalizationProvider localizer,
        ILogger logger,
        IIssueReporter issueReporter,
        ISettings settings,
        IServersLoader serversLoader,
        IConnectionManager connectionManager,
        IConnectionGroupFactory connectionGroupFactory)
        : base(parentViewNavigator, localizer, logger, issueReporter)
    {
        Settings = settings;
        ServersLoader = serversLoader;
        ConnectionManager = connectionManager;
        ConnectionGroupFactory = connectionGroupFactory;

        GroupsCvs = new()
        {
            Source = _groups,
            IsSourceGrouped = true
        };
    }

    public void Receive(ConnectionStatusChanged message)
    {
        ExecuteOnUIThread(() => OnConnectionStatusChanged(message.ConnectionStatus));
    }

    public void Receive(VpnPlanChangedMessage message)
    {
        ExecuteOnUIThread(() => OnVpnPlanChanged(message.OldPlan, message.NewPlan));
    }

    public void Receive(LoggedInMessage message)
    {
        ExecuteOnUIThread(OnLoggedIn);
    }

    public void Receive(ServerListChangedMessage message)
    {
        ExecuteOnUIThread(OnServerListChanged);
    }

    protected virtual void OnConnectionStatusChanged(ConnectionStatus connectionStatus)
    {
        InvalidateActiveConnection();
    }

    protected virtual void OnVpnPlanChanged(VpnPlan oldPlan, VpnPlan newPlan)
    {
        InvalidateRestrictions();
    }

    protected virtual void OnLoggedIn()
    {
        InvalidateRestrictions();
    }

    protected virtual void OnServerListChanged()
    {
        if (IsActive)
        {
            FetchItems();
        }
    }

    protected override void OnActivated()
    {
        FetchItems();

        InvalidateActiveConnection();
        InvalidateMaintenanceStates();
        InvalidateRestrictions();
    }

    protected void FetchItems()
    {
        _items.Reset(
            GetItems()
                .OrderBy(item => item.GroupType)
                .ThenBy(item => item.FirstSortProperty)
                .ThenBy(item => item.SecondSortProperty));

        GroupItems();

        InvalidateActiveConnection();
        InvalidateMaintenanceStates();
        InvalidateRestrictions();
    }

    protected abstract IEnumerable<ConnectionItemBase> GetItems();

    protected void GroupItems()
    {
        _groups.Reset(
            _items
                .GroupBy(item => item.GroupType)
                .OrderBy(group => group.Key)
                .Select(group => ConnectionGroupFactory.GetGroup(group.Key, group)));

        OnPropertyChanged(nameof(HasItems));
    }

    protected void InvalidateActiveConnection()
    {
        if (IsActive)
        {
            ConnectionDetails? connectionDetails = ConnectionManager.CurrentConnectionDetails;

            foreach (ConnectionItemBase item in _items)
            {
                item.InvalidateIsActiveConnection(connectionDetails);
            }
        }
    }

    protected void InvalidateMaintenanceStates()
    {
        if (IsActive)
        {
            IEnumerable<Server> servers = ServersLoader.GetServers();
            DeviceLocation? deviceLocation = Settings.DeviceLocation;

            foreach (ConnectionItemBase item in _items)
            {
                item.InvalidateIsUnderMaintenance(servers, deviceLocation);
            }
        }
    }

    protected virtual void InvalidateRestrictions()
    {
        if (IsActive)
        {
            bool isPaidUser = Settings.VpnPlan.IsPaid;

            foreach (ConnectionItemBase item in _items)
            {
                item.InvalidateIsRestricted(isPaidUser);
            }
        }
    }

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();

        OnPropertyChanged(nameof(Header));

        FetchItems();
    }
}