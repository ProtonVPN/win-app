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
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Common.Core.Extensions;

namespace ProtonVPN.Client.UI.Countries;

public partial class CountryViewModel : LocationViewModelBase, IComparable, ISearchableItem
{
    public required string ExitCountryCode { get; init; }

    public string ExitCountryName => Localizer.GetCountryName(ExitCountryCode);

    public required string EntryCountryCode { get; init; }

    public string EntryCountryName => Localizer.GetCountryName(EntryCountryCode);

    public string ViaEntryCountryLabel => Localizer.GetFormat("Countries_ViaCountry", EntryCountryName);

    public string ConnectButtonAutomationName => IsSecureCore && !string.IsNullOrEmpty(EntryCountryCode)
        ? $"{ExitCountryName} {ViaEntryCountryLabel}"
        : ExitCountryName;

    public required string SecondaryActionLabel { get; init; }
    public bool IsUnderMaintenance { get; init; }
    public CountryFeature CountryFeature { get; init; }
    public bool IsSecureCore => CountryFeature == CountryFeature.SecureCore;

    public string ConnectButtonAutomationId => $"Connect_to_{ExitCountryCode}";
    public string NavigateToCountryButtonAutomationId => $"Navigate_to_{ExitCountryCode}";
    public string ActiveConnectionAutomationId => $"Active_connection_{ExitCountryCode}";

    public override bool IsActiveConnection => ConnectionDetails is not null
                                            && !ConnectionDetails.IsGateway
                                            && ExitCountryCode == ConnectionDetails.ExitCountryCode 
                                            && (CountryFeature == CountryFeature.SecureCore) == (ConnectionDetails.OriginalConnectionIntent.Feature is SecureCoreFeatureIntent);

    protected override ConnectionIntent ConnectionIntent => new(new CountryLocationIntent(ExitCountryCode),
        CountryFeature.GetFeatureIntent(EntryCountryCode));

    public CountryViewModel(
        ILocalizationProvider localizationProvider,
        IMainViewNavigator mainViewNavigator,
        IConnectionManager connectionManager) :
        base(localizationProvider, mainViewNavigator, connectionManager)
    {
    }

    [RelayCommand]
    public async Task NavigateToCountryAsync()
    {
        await MainViewNavigator.NavigateToAsync<CountryTabViewModel>(this);
    }

    public int CompareTo(object? obj)
    {
        if (obj is not CountryViewModel country)
        {
            return 0;
        }

        if (string.IsNullOrEmpty(ExitCountryCode) && !string.IsNullOrEmpty(country.ExitCountryCode))
        {
            return -1;
        }

        if (string.IsNullOrEmpty(country.ExitCountryCode) && !string.IsNullOrEmpty(ExitCountryCode))
        {
            return 1;
        }

        return string.Compare(ExitCountryName, country.ExitCountryName, StringComparison.OrdinalIgnoreCase);
    }

    public bool MatchesSearchQuery(string query)
    {
        return !string.IsNullOrWhiteSpace(ExitCountryCode) && ExitCountryName.ContainsIgnoringCase(query)
            || !string.IsNullOrWhiteSpace(EntryCountryCode) && EntryCountryName.ContainsIgnoringCase(query);
    }
}