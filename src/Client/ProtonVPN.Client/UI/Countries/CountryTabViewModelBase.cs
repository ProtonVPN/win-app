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
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI.Collections;
using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.Client.Models.Activation;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Common.Legacy.Extensions;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Countries;

public abstract partial class CountryTabViewModelBase : PageViewModelBase<IViewNavigator>
{
    protected readonly IMainViewNavigator MainViewNavigator;
    protected readonly IOverlayActivator OverlayActivator;
    public readonly CountryViewModelsFactory CountryViewModelsFactory;

    public abstract IconElement? Icon { get; }

    public virtual bool HasItems => Items.Count > 0;

    public virtual int TotalItems => Items.Count;

    protected string CurrentCountryCode = string.Empty;
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasItems))]
    private AdvancedCollectionView _items = new();

    protected CountryTabViewModelBase(
        IMainViewNavigator mainViewNavigator,
        IOverlayActivator overlayActivator,
        CountryViewModelsFactory countryViewModelsFactory,
        IViewNavigator viewNavigator,
        ILocalizationProvider localizationProvider,
        ILogger logger,
        IIssueReporter issueReporter) 
        : base(viewNavigator, localizationProvider, logger, issueReporter)
    {
        MainViewNavigator = mainViewNavigator;
        OverlayActivator = overlayActivator;
        CountryViewModelsFactory = countryViewModelsFactory;
    }

    public virtual void LoadItems(string? country = null)
    {
        CurrentCountryCode = country ?? string.Empty;
        Items = new AdvancedCollectionView(GetItems(), true);
        Items.SortDescriptions.AddRange(GetSortDescriptions());
    }

    public override void OnNavigatedTo(object parameter, bool isBackNavigation)
    {
        base.OnNavigatedTo(parameter, isBackNavigation);

        LoadItems(CurrentCountryCode);
    }

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();

        LoadItems(CurrentCountryCode);
    }

    protected CityViewModel GetCity(City city)
    {
        List<ServerViewModel> servers = GetServersByCity(city);
        return CountryViewModelsFactory.GetCityViewModel(city, servers, CountryFeature);
    }

    private List<ServerViewModel> GetServersByCity(City city)
    {
        return GetServers(city)
            .Select(CountryViewModelsFactory.GetServerViewModel)
            .OrderBy(s => s.IsUnderMaintenance)
            .ThenBy(s => s.Load)
            .ToList();
    }

    protected abstract IEnumerable<Server> GetServers(City city);

    protected abstract IList GetItems();

    protected abstract IList<SortDescription> GetSortDescriptions();

    protected abstract CountryFeature CountryFeature { get; }
}