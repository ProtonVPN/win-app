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
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.UI.Dialogs.Overlays;

namespace ProtonVPN.Client.UI.Countries.CountryFeatureTabs;

public partial class SecureCoreCountryPageViewModel : CountryTabViewModelBase
{
    private readonly IServerManager _serverManager;

    public override IconElement? Icon => null;

    public override string Title => Localizer.Get("Countries_SecureCore");

    public SecureCoreCountryPageViewModel(
        IMainViewNavigator mainViewNavigator,
        IServerManager serverManager,
        ICountryFeatureTabsViewNavigator viewNavigator,
        ILocalizationProvider localizationProvider) : base(mainViewNavigator, viewNavigator, localizationProvider)
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
        await MainViewNavigator.ShowOverlayAsync<SecureCoreOverlayViewModel>();
    }

    protected override IList GetItems()
    {
        return _serverManager.GetSecureCoreServersByExitCountry(CurrentCountryCode)
            .Select(s => new CountryViewModel(Localizer, MainViewNavigator)
            {
                EntryCountryCode = s.EntryCountry,
                EntryCountryName = Localizer.GetFormat("Countries_ViaCountry", Localizer.GetCountryName(s.EntryCountry)),
                ExitCountryCode = s.ExitCountry,
                ExitCountryName = Localizer.GetCountryName(s.ExitCountry),
                CountryFeature = CountryFeature.SecureCore,
                IsSecureCore = true,
            }).ToList();
    }

    protected override IList<SortDescription> GetSortDescriptions()
    {
        return new List<SortDescription>
        {
            new(nameof(CountryViewModel.EntryCountryCode), SortDirection.Ascending)
        };
    }
}