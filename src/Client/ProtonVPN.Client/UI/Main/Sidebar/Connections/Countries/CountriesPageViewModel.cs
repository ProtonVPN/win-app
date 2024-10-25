﻿/*
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

using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Client.Common.UI.Assets.Icons.Base;
using ProtonVPN.Client.Common.UI.Assets.Icons.PathIcons;
using ProtonVPN.Client.Contracts.Enums;
using ProtonVPN.Client.Contracts.Services.Navigation;
using ProtonVPN.Client.Factories;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Models.Connections;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.UI.Main.Sidebar.Connections.Bases.Contracts;
using ProtonVPN.Client.UI.Main.Sidebar.Connections.Bases.ViewModels;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Main.Sidebar.Connections.Countries;

public partial class CountriesPageViewModel : ConnectionPageViewModelBase
{
    [ObservableProperty]
    private ICountriesComponent _selectedCountriesComponent;

    public override string Header => Localizer.Get("Countries");

    public override IconElement Icon => new Earth() { Size = PathIconSize.Pixels16 };

    public override int SortIndex { get; } = 2;

    public List<ICountriesComponent> CountriesComponents { get; }

    public CountriesPageViewModel(
        IConnectionsViewNavigator parentViewNavigator,
        ILocalizationProvider localizer,
        ILogger logger,
        IIssueReporter issueReporter,
        ISettings settings,
        IServersLoader serversLoader,
        IConnectionManager connectionManager,
        IConnectionGroupFactory connectionGroupFactory,
        IEnumerable<ICountriesComponent> countriesComponents)
        : base(parentViewNavigator,
               localizer,
               logger,
               issueReporter,
               settings,
               serversLoader,
               connectionManager,
               connectionGroupFactory)
    {
        CountriesComponents = new(countriesComponents.OrderBy(p => p.SortIndex));

        _selectedCountriesComponent = CountriesComponents.First();
    }

    public override void OnNavigatedTo(object parameter, bool isBackNavigation)
    {
        base.OnNavigatedTo(parameter, isBackNavigation);

        if (parameter is CountriesConnectionType connectionType)
        {
            GoToCountryFeature(connectionType);
        }
    }

    protected override void OnLoggedIn()
    {
        base.OnLoggedIn();

        GoToCountryFeature(CountriesConnectionType.All);
    }

    protected override IEnumerable<ConnectionItemBase> GetItems()
    {
        return SelectedCountriesComponent.GetItems();
    }

    private void GoToCountryFeature(CountriesConnectionType connectionType)
    {
        SelectedCountriesComponent = CountriesComponents.FirstOrDefault(c => c.ConnectionType == connectionType)
                                  ?? CountriesComponents.First();
    }

    partial void OnSelectedCountriesComponentChanged(ICountriesComponent value)
    {
        FetchItems();
    }
}