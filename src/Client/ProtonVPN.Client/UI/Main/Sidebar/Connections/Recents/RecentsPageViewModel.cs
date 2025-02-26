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

using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Client.Common.UI.Assets.Icons.Base;
using ProtonVPN.Client.Common.UI.Assets.Icons.PathIcons;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Services.Navigation;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Factories;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Recents.Contracts;
using ProtonVPN.Client.Logic.Recents.Contracts.Messages;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.Client.Logic.Users.Contracts.Messages;
using ProtonVPN.Client.Models.Connections;
using ProtonVPN.Client.Models.Connections.Recents;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.UI.Main.Sidebar.Connections.Bases.ViewModels;
using ProtonVPN.Common.Core.Geographical;

namespace ProtonVPN.Client.UI.Main.Sidebar.Connections.Recents;

public class RecentsPageViewModel : ConnectionPageViewModelBase,
    IEventMessageReceiver<RecentConnectionsChangedMessage>
{
    private readonly IConnectionItemFactory _connectionItemFactory;
    private readonly IRecentConnectionsManager _recentConnectionsManager;

    public override string Header => Localizer.Get("Home_Recents_Title");

    public override IconElement Icon => new ClockRotateLeft() { Size = PathIconSize.Pixels16 };

    public override int SortIndex { get; } = 1;

    public override bool IsAvailable => ParentViewNavigator.CanNavigateToRecentsView();

    public RecentsPageViewModel(
        IConnectionsViewNavigator parentViewNavigator,
        ISettings settings,
        IServersLoader serversLoader,
        IConnectionManager connectionManager,
        IConnectionGroupFactory connectionGroupFactory,
        IConnectionItemFactory connectionItemFactory, 
        IRecentConnectionsManager recentConnectionsManager,
        IViewModelHelper viewModelHelper)
        : base(parentViewNavigator,
               settings,
               serversLoader,
               connectionManager,
               connectionGroupFactory,
               viewModelHelper)
    {
        _connectionItemFactory = connectionItemFactory;
        _recentConnectionsManager = recentConnectionsManager;
    }

    public void Receive(RecentConnectionsChangedMessage message)
    {
        ExecuteOnUIThread(FetchItems);
    }

    protected override IEnumerable<ConnectionItemBase> GetItems()
    {
        return _recentConnectionsManager.GetRecentConnections()
                                        .Select(_connectionItemFactory.GetRecent);
    }

    protected override void InvalidateMaintenanceStates()
    {
        if (IsActive)
        {
            IEnumerable<Server> servers = ServersLoader.GetServers();
            DeviceLocation? deviceLocation = Settings.DeviceLocation;

            foreach (RecentConnectionItem item in Items.OfType<RecentConnectionItem>())
            {
                item.InvalidateIsUnderMaintenance(servers, deviceLocation);
            }
        }
    }

    protected override void OnVpnPlanChanged(VpnPlan oldPlan, VpnPlan newPlan)
    {
        base.OnVpnPlanChanged(oldPlan, newPlan);

        InvalidateIsAvailable();
    }

    protected override void OnLoggedIn()
    {
        base.OnLoggedIn();

        InvalidateIsAvailable();
    }

    protected override void OnItemsChanged()
    {
        base.OnItemsChanged();

        InvalidateIsAvailable();
    }

    private void InvalidateIsAvailable()
    {
        OnPropertyChanged(nameof(IsAvailable));
    }
}