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
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI.UI;
using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.UI.Dialogs.Overlays;

namespace ProtonVPN.Client.UI.Countries.CountryFeatureTabs;

public partial class TorServersPageViewModel : CountryTabViewModelBase
{
    private readonly IServerManager _serverManager;

    public override IconElement? Icon => null;

    public override string Title => Localizer.Get("Countries_Tor");

    public TorServersPageViewModel(
        IMainViewNavigator mainViewNavigator,
        IServerManager serverManager,
        CountriesViewModelsFactory countriesViewModelsFactory,
        ICountryFeatureTabsViewNavigator viewNavigator,
        ILocalizationProvider localizationProvider) : base(mainViewNavigator, countriesViewModelsFactory, viewNavigator,
        localizationProvider)
    {
        _serverManager = serverManager;
    }

    public override void OnNavigatedTo(object parameter)
    {
        CurrentCountryCode = parameter as string ?? string.Empty;

        base.OnNavigatedTo(parameter);
    }

    [RelayCommand]
    public async Task ShowInfoOverlayAsync()
    {
        await MainViewNavigator.ShowOverlayAsync<TorOverlayViewModel>();
    }

    protected override List<Server> GetServers(string city)
    {
        return new List<Server>();
    }

    protected override IList GetItems()
    {
        return _serverManager
            .GetTorServersByExitCountry(CurrentCountryCode)
            .Select(CountriesViewModelsFactory.GetServerViewModel)
            .ToList();
    }

    protected override IList<SortDescription> GetSortDescriptions()
    {
        return new List<SortDescription> { new(nameof(ServerViewModel.Load), SortDirection.Ascending) };
    }
}