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

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI.Collections;
using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Client.Common.UI.Assets.Icons.PathIcons;
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Messages;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.UI.Gateways.Factories;
using ProtonVPN.Client.UI.Gateways.Items;

namespace ProtonVPN.Client.UI.Gateways;

public partial class GatewaysViewModel :
    NavigationPageViewModelBase,
    IEventMessageReceiver<ServerListChangedMessage>
{
    private readonly IServersLoader _serversLoader;
    private readonly GatewayViewModelsFactory _gatewayViewModelsFactory;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasGateways))]
    private AdvancedCollectionView _gateways = new();

    public override IconElement Icon { get; } = new Servers();

    public override bool IsBackEnabled => false;

    public override bool IsPageEnabled => HasGateways;

    public override string? Title => Localizer.Get("Gateways_Page_Title");

    public bool HasGateways => Gateways.Count > 0;

    public GatewaysViewModel(
        IMainViewNavigator viewNavigator,
        ILocalizationProvider localizationProvider,
        IServersLoader serversLoader,
        GatewayViewModelsFactory gatewayViewModelsFactory)
        : base(viewNavigator, localizationProvider)
    {
        _serversLoader = serversLoader;
        _gatewayViewModelsFactory = gatewayViewModelsFactory;
    }

    public override void OnNavigatedTo(object parameter)
    {
        base.OnNavigatedTo(parameter);

        LoadItems();
    }

    public void Receive(ServerListChangedMessage message)
    {
        ExecuteOnUIThread(() =>
        {
            LoadItems();
        });
    }

    private void LoadItems()
    {
        Gateways = new AdvancedCollectionView(GetGatewayViewModels(), true);
        Gateways.SortDescriptions.Add(new(SortDirection.Ascending));

        OnPropertyChanged(nameof(HasGateways));
        OnPropertyChanged(nameof(IsPageEnabled));
    }

    private List<GatewayViewModel> GetGatewayViewModels()
    {
        return _serversLoader.GetGateways()
            .Select(GetGatewayViewModel)
            .ToList();
    }

    private GatewayViewModel GetGatewayViewModel(string gatewayName)
    {
        return _gatewayViewModelsFactory.GetGatewayViewModel(gatewayName, _serversLoader.GetServersByGateway(gatewayName).ToList());
    }
}