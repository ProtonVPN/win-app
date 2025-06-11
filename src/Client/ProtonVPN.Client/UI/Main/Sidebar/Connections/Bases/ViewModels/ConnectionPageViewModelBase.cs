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
using Microsoft.UI.Xaml.Navigation;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Services.Navigation;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Factories;
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Messages;
using ProtonVPN.Client.Logic.Users.Contracts.Messages;
using ProtonVPN.Client.Models.Connections;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.UI.Main.Sidebar.Bases;
using ProtonVPN.Client.UI.Main.Sidebar.Connections.Bases.Contracts;

namespace ProtonVPN.Client.UI.Main.Sidebar.Connections.Bases.ViewModels;

public abstract class ConnectionPageViewModelBase : ConnectionListViewModelBase<IConnectionsViewNavigator>,
    IConnectionPage,
    IEventMessageReceiver<ConnectionStatusChangedMessage>,
    IEventMessageReceiver<VpnPlanChangedMessage>,
    IEventMessageReceiver<LoggedInMessage>,
    IEventMessageReceiver<ServerListChangedMessage>
{
    protected bool WasInvalidatedWhileInactive { get; set; } = true;

    public abstract int SortIndex { get; }

    public abstract string Header { get; }

    public string ShortcutText => $"ctrl + {SortIndex}";

    public abstract IconElement Icon { get; }

    public virtual bool IsAvailable => true;

    public bool IsActivePage => ParentViewNavigator.GetCurrentPageContext() == this;

    protected ConnectionPageViewModelBase(
        IConnectionsViewNavigator parentViewNavigator,
        ISettings settings,
        IServersLoader serversLoader,
        IConnectionManager connectionManager,
        IConnectionGroupFactory connectionGroupFactory,
        IViewModelHelper viewModelHelper)
        : base(parentViewNavigator,
               settings,
               serversLoader,
               connectionManager,
               connectionGroupFactory,
               viewModelHelper)
    { }

    public void Receive(ConnectionStatusChangedMessage message)
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
        FetchItems();
    }

    protected override void OnNavigation(NavigationEventArgs e)
    {
        base.OnNavigation(e);

        OnPropertyChanged(nameof(IsActivePage));
    }

    protected override void OnActivated()
    {
        OnPropertyChanged(nameof(IsActive));

        if (WasInvalidatedWhileInactive)
        {
            FetchItems();
        }
        else
        {
            InvalidateActiveConnection();
            InvalidateMaintenanceStates();
            InvalidateRestrictions();
        }
    }

    protected void FetchItems()
    {
        if (!IsActive)
        {
            WasInvalidatedWhileInactive = true;
            return;
        }

        ResetItems(GetItems());

        GroupItems();

        InvalidateActiveConnection();
        InvalidateMaintenanceStates();
        InvalidateRestrictions();

        OnItemsChanged();

        WasInvalidatedWhileInactive = false;
    }

    protected virtual void OnItemsChanged()
    {
        OnPropertyChanged(nameof(HasItems));
    }

    protected abstract IEnumerable<ConnectionItemBase> GetItems();

    protected void GroupItems()
    {
        ResetGroups();
        OnPropertyChanged(nameof(HasItems));
    }

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();

        OnPropertyChanged(nameof(Header));

        FetchItems();
    }
}