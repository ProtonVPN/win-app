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
using CommunityToolkit.WinUI.Collections;
using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Client.Common.UI.Assets.Icons.PathIcons;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Models.Activation;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.UI.Dialogs.Overlays;

namespace ProtonVPN.Client.UI.Countries.CountryFeatureTabs;

public partial class SecureCoreCountryPageViewModel : CountryTabViewModelBase
{
    private readonly IServersLoader _serversLoader;

    public override IconElement Icon => new LockLayers();

    public override string Title => Localizer.Get("Countries_SecureCore");

    protected override CountryFeature CountryFeature => CountryFeature.SecureCore;

    public SecureCoreCountryPageViewModel(
        IMainViewNavigator mainViewNavigator,
        IOverlayActivator overlayActivator,
        IServersLoader serversLoader,
        ICountryFeatureTabsViewNavigator viewNavigator,
        CountryViewModelsFactory countryViewModelsFactory,
        ILocalizationProvider localizationProvider) 
        : base(mainViewNavigator, 
               overlayActivator,
               countryViewModelsFactory, 
               viewNavigator,
               localizationProvider)
    {
        _serversLoader = serversLoader;
    }

    public override void OnNavigatedTo(object parameter)
    {
        CurrentCountryCode = parameter as string ?? string.Empty;

        base.OnNavigatedTo(parameter);
    }

    [RelayCommand]
    public async Task ShowInfoOverlayAsync()
    {
        await OverlayActivator.ShowOverlayAsync<SecureCoreOverlayViewModel>();
    }

    protected override List<Server> GetServers(City city)
    {
        return new List<Server>();
    }

    protected override IList GetItems()
    {
        return _serversLoader
            .GetServersByFeaturesAndCountryCode(ServerFeatures.SecureCore, CurrentCountryCode)
            .Select(CountryViewModelsFactory.GetServerViewModel)
            .ToList();
    }

    protected override IList<SortDescription> GetSortDescriptions()
    {
        return new List<SortDescription>
        {
            new(nameof(CountryViewModel.EntryCountryCode), SortDirection.Ascending)
        };
    }
}