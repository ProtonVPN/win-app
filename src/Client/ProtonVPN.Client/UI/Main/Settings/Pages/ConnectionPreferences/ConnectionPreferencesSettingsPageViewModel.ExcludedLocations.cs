/*
 * Copyright (c) 2026 Proton AG
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

using System.Collections.Specialized;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProtonVPN.Client.Common.Collections;
using ProtonVPN.Client.Logic.Searches.Contracts;
using ProtonVPN.Client.Models.Features.LocationExclusion;
using ProtonVPN.Client.Settings.Contracts.Models;

namespace ProtonVPN.Client.UI.Main.Settings.Pages.ConnectionPreferences;

public partial class ConnectionPreferencesSettingsPageViewModel
{
    private string _normalizedSearchInput = string.Empty;

    [ObservableProperty]
    private bool _isLocationSelectorOpen;

    [ObservableProperty]
    private string _searchInput = string.Empty;

    public bool IsInSearchMode => !string.IsNullOrWhiteSpace(_normalizedSearchInput);

    protected readonly SmartNotifyObservableCollection<IExcludableLocation> Locations = [];

    protected IEnumerable<IExcludableLocation> AvailableLocations => Locations.Where(IsAvailable);

    public bool HasAvailableLocations => AvailableLocations.Any();

    protected IOrderedEnumerable<IExcludableLocation> AvailableCountries
        => AvailableLocations.Where(l => l is ExcludableCountryItem
                                      || l is IExcludableChildItem child && child.ParentCountry.HasMultipleAvailableChildren)
                             .OrderBy(l => l.AbsoluteSortOrder);

    protected IOrderedEnumerable<IExcludableLocation> AvailableSearchResults
        => AvailableLocations.Where(Matches)
                             .OrderBy(l => l.Type)
                             .ThenBy(l => l.RelativeSortOrder);

    public IOrderedEnumerable<IExcludableLocation> ExcludableLocations
        => IsInSearchMode
            ? AvailableSearchResults
            : AvailableCountries;

    public bool HasExcludableLocations => ExcludableLocations.Any();

    public IOrderedEnumerable<IExcludableLocation> ExcludedLocations
        => Locations.Where(IsExcluded)
                    .OrderBy(l => l.Type)
                    .ThenBy(l => l.RelativeSortOrder);

    public bool HasExcludedLocations => ExcludedLocations.Any();

    public string ExcludableLocationSelectorText
        => HasAvailableLocations
            ? Localizer.Get("Settings_Connection_ExcludedLocations_Search")
            : Localizer.Get("Settings_Connection_ExcludedLocations_NoLocations");

    private void InvalidateExcludableLocations()
    {
        Locations.Reset(
            _excludeLocationsManager.GetExcludableLocations());
    }

    private bool IsAvailable(IExcludableLocation location)
    {
        return !location.IsLocationExcluded;
    }

    private bool IsExcluded(IExcludableLocation location)
    {
        if (location is ExcludableLocationGroup group)
        {
            return group.Locations.Any(IsExcluded);
        }

        return location.IsExcluded;
    }

    private List<ExcludedLocation> GetExcludedLocations()
    {
        // Ordering the excluded locations by country code, then state, then city
        // to ensure a consistent order in the settings.
        return ExcludedLocations
            .Where(l => l is not ExcludableLocationGroup)
            .Select(l => l.ToExcludedLocation())
            .OrderBy(l => l.CountryCode)
            .ThenBy(l => l.StateName)
            .ThenBy(l => l.CityName)
            .ToList();
    }

    private void OnLocationItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        OnPropertyChanged(nameof(AvailableLocations));
        OnPropertyChanged(nameof(HasAvailableLocations));
        OnPropertyChanged(nameof(ExcludableLocations));
        OnPropertyChanged(nameof(HasExcludableLocations));
        OnPropertyChanged(nameof(ExcludedLocations));
        OnPropertyChanged(nameof(HasExcludedLocations));
        OnPropertyChanged(nameof(ExcludableLocationSelectorText));

        CloseLocationSelector();
    }

    private void OnLocationsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(AvailableLocations));
        OnPropertyChanged(nameof(HasAvailableLocations));
        OnPropertyChanged(nameof(ExcludableLocations));
        OnPropertyChanged(nameof(HasExcludableLocations));
        OnPropertyChanged(nameof(ExcludedLocations));
        OnPropertyChanged(nameof(HasExcludedLocations));
        OnPropertyChanged(nameof(ExcludableLocationSelectorText));
    }

    private bool Matches(IExcludableLocation location)
    {
        return location switch
        {
            ExcludableLocationGroup group => group.Locations.Any(l => IsAvailable(l) && Matches(l)),
            ExcludableCountryItem country => SearchMatcher.MatchesCountry(country.Location, country.DisplayName, _normalizedSearchInput),
            ExcludableStateItem state => SearchMatcher.MatchesState(state.DisplayName, _normalizedSearchInput),
            ExcludableCityItem city => SearchMatcher.MatchesCity(city.DisplayName, _normalizedSearchInput),
            _ => false
        };
    }

    [RelayCommand]
    private void OpenLocationSelector()
    {
        SearchInput = string.Empty;

        IsLocationSelectorOpen = true;
    }

    [RelayCommand]
    private void CloseLocationSelector()
    {
        IsLocationSelectorOpen = false;

        SearchInput = string.Empty;
    }

    partial void OnSearchInputChanged(string value)
    {
        _normalizedSearchInput = value.NormalizeInput() ?? string.Empty;

        OnPropertyChanged(nameof(IsInSearchMode));
        OnPropertyChanged(nameof(ExcludableLocations));
        OnPropertyChanged(nameof(HasExcludableLocations));
    }
}