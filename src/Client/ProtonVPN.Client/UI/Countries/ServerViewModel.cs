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
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.UI.Home;
using ProtonVPN.Common.Core.Extensions;

namespace ProtonVPN.Client.UI.Countries;

public partial class ServerViewModel : ViewModelBase, ISearchableItem
{
    protected readonly IMainViewNavigator MainViewNavigator;
    protected readonly IConnectionManager ConnectionManager;

    public string Id { get; init; }
    public required string ExitCountryCode { get; init; }
    public required string ExitCountryName { get; init; }
    public required string EntryCountryCode { get; init; }
    public required string EntryCountryName { get; init; }
    public required string Name { get; init; }
    public required string City { get; init; }
    public double Load { get; init; }
    public string LoadPercent { get; init; }
    public bool IsVirtual { get; init; }
    public bool IsSecureCore { get; init; }
    public bool SupportsP2P { get; init; }
    public bool IsTor { get; init; }
    public bool IsUnderMaintenance { get; init; }
    public bool IsActiveConnection { get; init; }

    public ServerViewModel(ILocalizationProvider localizationProvider, IMainViewNavigator mainViewNavigator,
        IConnectionManager connectionManager) : base(localizationProvider)
    {
        MainViewNavigator = mainViewNavigator;
        ConnectionManager = connectionManager;
    }

    [RelayCommand]
    public virtual async Task ConnectAsync()
    {
        ILocationIntent locationIntent = new ServerLocationIntent(Id, Name, ExitCountryCode, City);
        IFeatureIntent? featureIntent = GetFeatureIntent();

        await NavigateToHomePageAsync();
        await ConnectionManager.ConnectAsync(new ConnectionIntent(locationIntent, featureIntent));
    }

    protected async Task NavigateToHomePageAsync()
    {
        await MainViewNavigator.NavigateToAsync<HomeViewModel>();
    }

    public bool MatchesSearchQuery(string query)
    {
        return Name.ContainsIgnoringCase(query);
    }

    private IFeatureIntent? GetFeatureIntent()
    {
        if (IsSecureCore)
        {
            return new SecureCoreFeatureIntent(EntryCountryCode);
        }

        if (SupportsP2P)
        {
            return new P2PFeatureIntent();
        }

        if (IsTor)
        {
            return new TorFeatureIntent();
        }

        return null;
    }
}