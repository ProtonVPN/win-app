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

using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Client.Common.UI.Assets.Icons.Base;
using ProtonVPN.Client.Common.UI.Assets.Icons.PathIcons;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Services.Navigation;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Factories;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Models.Connections;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Messages;
using ProtonVPN.Client.UI.Main.Sidebar.Connections.Bases.ViewModels;

namespace ProtonVPN.Client.UI.Main.Sidebar.Connections.Gateways;

public partial class GatewaysPageViewModel : ConnectionPageViewModelBase,
    IEventMessageReceiver<SettingChangedMessage>
{
    private readonly ILocationItemFactory _locationItemFactory;

    public override string Header => Localizer.Get("Gateways_Page_Title");

    public override IconElement Icon => new Buildings() { Size = PathIconSize.Pixels16 };

    public override int SortIndex { get; } = 4;

    public override bool IsAvailable => ParentViewNavigator.CanNavigateToGatewaysView();

    public string BannerDescription => Localizer.Get("Gateways_Page_Description");

    public bool IsInfoBannerVisible => !Settings.IsGatewayInfoBannerDismissed;

    public GatewaysPageViewModel(
        IConnectionsViewNavigator parentViewNavigator,
        ISettings settings,
        IServersLoader serversLoader,
        IConnectionManager connectionManager,
        IConnectionGroupFactory connectionGroupFactory,
        ILocationItemFactory locationItemFactory,
        IViewModelHelper viewModelHelper)
        : base(parentViewNavigator,
               settings,
               serversLoader,
               connectionManager,
               connectionGroupFactory,
               viewModelHelper)
    {
        _locationItemFactory = locationItemFactory;
    }

    protected override IEnumerable<ConnectionItemBase> GetItems()
    {
        IEnumerable<ConnectionItemBase> gateways = 
            ServersLoader.GetGateways()
                         .Select(_locationItemFactory.GetGateway);

        return gateways;
    }

    [RelayCommand]
    private void DismissInfoBanner()
    {
        Settings.IsGatewayInfoBannerDismissed = true;
    }

    public void Receive(SettingChangedMessage message)
    {
        if (message.PropertyName == nameof(ISettings.IsGatewayInfoBannerDismissed))
        {
            ExecuteOnUIThread(() => OnPropertyChanged(nameof(IsInfoBannerVisible)));
        }
    }

    protected override void OnServerListChanged()
    {
        base.OnServerListChanged();

        OnPropertyChanged(nameof(IsAvailable));
    }
}