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

using System.Collections;
using CommunityToolkit.WinUI.UI;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Common.Core.Extensions;

namespace ProtonVPN.Client.UI.Countries;

public abstract class CountriesTabViewModelBase : CountryTabViewModelBase, ICountryTabViewModelBase
{
    protected readonly IServerManager ServerManager;

    protected string LastSearchQuery = string.Empty;

    protected CountriesTabViewModelBase(
        IMainViewNavigator mainViewNavigator,
        IServerManager serverManager,
        ICountriesFeatureTabsViewNavigator viewNavigator,
        ILocalizationProvider localizationProvider) : base(mainViewNavigator, viewNavigator, localizationProvider)
    {
        MainViewNavigator = mainViewNavigator;
        ServerManager = serverManager;
    }

    protected abstract CountryFeature CountryFeature { get; }

    protected abstract IList<string> GetCountryCodes();

    protected abstract int GetItemCountByCountry(string exitCountryCode);

    public virtual void FilterItems(string query)
    {
        LastSearchQuery = query;
        Items.Filter = string.IsNullOrWhiteSpace(LastSearchQuery) ? null : o => MatchesSearchQuery(o, query);
        OnPropertyChanged(nameof(HasItems));
        OnPropertyChanged(nameof(TotalItems));
    }

    protected bool MatchesSearchQuery(object item, string query)
    {
        return item is CountryViewModel country &&
               !string.IsNullOrWhiteSpace(country.ExitCountryCode) &&
               country.ExitCountryName.ContainsIgnoringCase(query);
    }

    public override void OnNavigatedTo(object parameter)
    {
        base.OnNavigatedTo(parameter);

        if (!string.IsNullOrEmpty(LastSearchQuery))
        {
            FilterItems(LastSearchQuery);
        }
    }
    
    protected override IList GetItems()
    {
        return GetCountryCodes()
            .Select(CreateCountry)
            .Prepend(GetFastestCountry())
            .ToList();
    }

    protected CountryViewModel GetFastestCountry()
    {
        string fastestCountryLabel = Localizer.Get("Countries_Fastest");

        return new CountryViewModel(Localizer, MainViewNavigator)
        {
            ExitCountryName = fastestCountryLabel,
            CountryFeature = CountryFeature,
            IsSecureCore = CountryFeature == CountryFeature.SecureCore,
        };
    }

    protected override IList<SortDescription> GetSortDescriptions()
    {
        return new List<SortDescription> { new(SortDirection.Ascending) };
    }

    private CountryViewModel CreateCountry(string exitCountryCode)
    {
        return new CountryViewModel(Localizer, MainViewNavigator)
        {
            ExitCountryCode = exitCountryCode,
            ExitCountryName = Localizer.GetCountryName(exitCountryCode),
            IsUnderMaintenance = false,
            SecondaryActionLabel = Localizer.GetPluralFormat(GetCountrySecondaryActionLabel(), GetItemCountByCountry(exitCountryCode)),
            CountryFeature = CountryFeature,
            IsSecureCore = CountryFeature == CountryFeature.SecureCore,
        };
    }

    private string GetCountrySecondaryActionLabel()
    {
        return CountryFeature == CountryFeature.Cities ? "Countries_City" : "Countries_Server";
    }
}