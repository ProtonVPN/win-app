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
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Models.Navigation;

namespace ProtonVPN.Client.UI.Countries.CountriesFeatureTabs;

public class AllCountriesPageViewModel : CountriesTabViewModelBase
{
    public override IconElement? Icon => null;

    public override string Title => Localizer.Get("Countries_All");

    protected override CountryFeature CountryFeature => CountryFeature.Cities;

    public override bool HasItems => string.IsNullOrWhiteSpace(LastSearchQuery) ? Items.Count > 1 : Items.Count > 0;

    public override int TotalItems => string.IsNullOrWhiteSpace(LastSearchQuery) ? Items.Count - 1 : Items.Count;

    public AllCountriesPageViewModel(
        IMainViewNavigator mainViewNavigator,
        ICountriesFeatureTabsViewNavigator viewNavigator,
        IServerManager serverManager,
        ILocalizationProvider localizationProvider) : base(mainViewNavigator, serverManager, viewNavigator, localizationProvider)
    {
    }

    protected override IList<string> GetCountryCodes()
    {
        return ServerManager.GetCountryCodes();
    }

    protected override int GetItemCountByCountry(string exitCountryCode)
    {
        return ServerManager.GetCitiesByCountry(exitCountryCode).Count;
    }
}