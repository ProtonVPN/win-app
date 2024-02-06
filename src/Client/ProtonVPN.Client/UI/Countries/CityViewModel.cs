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
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.Client.Models.Activation;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.UI.Dialogs.Overlays;
using ProtonVPN.Common.Core.Extensions;

namespace ProtonVPN.Client.UI.Countries;

public partial class CityViewModel : LocationViewModelBase, ISearchableItem
{
    private readonly IOverlayActivator _overlayActivator;

    public required City City { get; init; }
    public string Name => City.Name;
    public string CountryCode => City.CountryCode;
    public CountryFeature CountryFeature { get; init; }
    public List<ServerViewModel> Servers { get; init; } = new();

    public string ConnectButtonAutomationId => $"Connect_to_{Name}";
    public string ShowServersButtonAutomationId => $"Show_servers_{Name}";
    public string ActiveConnectionAutomationId => $"Active_connection_{Name}";

    public override bool IsActiveConnection => ConnectionDetails != null 
                                            && !ConnectionDetails.IsGateway 
                                            && ConnectionDetails.ExitCountryCode == CountryCode 
                                            && ConnectionDetails.CityState == Name;

    protected override ConnectionIntent ConnectionIntent => new(new CityStateLocationIntent(CountryCode, Name),
        CountryFeature.GetFeatureIntent());

    public CityViewModel(
        ILocalizationProvider localizationProvider, 
        IMainViewNavigator mainViewNavigator,
        IOverlayActivator overlayActivator,
        IConnectionManager connectionManager) 
        : base(localizationProvider, mainViewNavigator, connectionManager)
    {
        _overlayActivator = overlayActivator;
    }

    [RelayCommand]
    public async Task ShowServerLoadOverlayAsync()
    {
        await _overlayActivator.ShowOverlayAsync<ServerLoadOverlayViewModel>();
    }

    public bool MatchesSearchQuery(string query)
    {
        return !string.IsNullOrWhiteSpace(Name) && Name.ContainsIgnoringCase(query);
    }
}