/*
 * Copyright (c) 2024 Proton AG
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
using ProtonVPN.Client.Contracts.Enums;
using ProtonVPN.Client.Contracts.Services.Navigation;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Factories;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Logic.Searches.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Enums;
using ProtonVPN.Client.Logic.Servers.Contracts.Messages;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.Client.Models.Connections;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.UI.Main.Sidebar.Bases;
using ProtonVPN.Client.UI.Main.Sidebar.Connections.Bases.Contracts;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Main.Sidebar.Search;

public partial class SearchResultsPageViewModel : ConnectionListViewModelBase<ISidebarViewNavigator>,
    ISearchInputReceiver, IEventMessageReceiver<ConnectionStatusChangedMessage>,
    IEventMessageReceiver<ServerListChangedMessage>
{
    private readonly IGlobalSearch _globalSearch;
    private readonly ILocationItemFactory _locationItemFactory; 
    
    private string _input = string.Empty;

    [ObservableProperty]
    private bool _hasSearchInput;

    [ObservableProperty]
    private ICountriesComponent _selectedCountriesComponent;

    public List<ICountriesComponent> CountriesComponents { get; }

    public string ExampleCountries => $"{Localizer.Get("Country_val_JP")}, {Localizer.Get("Country_val_US")}";
    public string ExampleCities => "Tokyo, Los Angeles";
    public string ExampleServers => "JP#75, US-NY#166";

    public SearchResultsPageViewModel(
        ISettings settings,
        IConnectionManager connectionManager,
        IServersLoader serversLoader,
        ISidebarViewNavigator parentViewNavigator,
        ILocalizationProvider localizer,
        ILogger logger,
        IIssueReporter issueReporter,
        IGlobalSearch globalSearch,
        ILocationItemFactory locationItemFactory,
        IConnectionGroupFactory connectionGroupFactory,
        IEnumerable<ICountriesComponent> countriesComponents)
        : base(parentViewNavigator, localizer, logger, issueReporter, settings,
            serversLoader, connectionManager, connectionGroupFactory)
    {
        _globalSearch = globalSearch;
        _locationItemFactory = locationItemFactory;

        CountriesComponents = new(countriesComponents.OrderBy(p => p.SortIndex));
        _selectedCountriesComponent = CountriesComponents.First();
    }

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();
        OnPropertyChanged(nameof(ExampleCountries));
        SearchAsync().Wait();
    }

    partial void OnSelectedCountriesComponentChanged(ICountriesComponent value)
    {
        SearchAsync().Wait();
    }

    public async Task SearchAsync(string input)
    {
        _input = input;
        await SearchAsync();
    }

    private async Task SearchAsync()
    {
        string input = _input;
        if (string.IsNullOrWhiteSpace(input))
        {
            HasSearchInput = false;
            SetSearchResult([]);
        }
        else
        {
            HasSearchInput = true;
            Func<ILocation, ConnectionItemBase?> connectionItemCreationFunction = GetConnectionItemCreationFunction();
            IEnumerable<ConnectionItemBase> result = (await _globalSearch.SearchAsync(input, GetServerFeatures()))
                .Select(connectionItemCreationFunction)
                .Where(ci => ci is not null)
                .Cast<ConnectionItemBase>();
            SetSearchResult(result);
        }
    }

    private ServerFeatures? GetServerFeatures()
    {
        return SelectedCountriesComponent.ConnectionType switch
        {
            CountriesConnectionType.SecureCore => ServerFeatures.SecureCore,
            CountriesConnectionType.P2P => ServerFeatures.P2P,
            CountriesConnectionType.Tor => ServerFeatures.Tor,
            _ => null,
        };
    }

    private void SetSearchResult(IEnumerable<ConnectionItemBase> result)
    {
        ResetItems(result);
        ResetGroups();

        InvalidateActiveConnection();
        InvalidateMaintenanceStates();
        InvalidateRestrictions();
    }

    private Func<ILocation, ConnectionItemBase?> GetConnectionItemCreationFunction()
    {
        return SelectedCountriesComponent.ConnectionType switch
        {
            CountriesConnectionType.SecureCore => CreateSecureCoreConnectionItem,
            CountriesConnectionType.P2P => CreateP2PConnectionItem,
            CountriesConnectionType.Tor => CreateTorConnectionItem,
            _ => CreateStandardConnectionItem,
        };
    }

    private ConnectionItemBase? CreateSecureCoreConnectionItem(ILocation location)
    {
        if (location is Server server)
        {
            return _locationItemFactory.GetServer(server);
        }
        else if (location is Country country)
        {
            return _locationItemFactory.GetSecureCoreCountry(country);
        }

        return null;
    }

    private ConnectionItemBase? CreateP2PConnectionItem(ILocation location)
    {
        if (location is Server server)
        {
            return _locationItemFactory.GetP2PServer(server);
        }
        else if (location is City city)
        {
            return _locationItemFactory.GetP2PCity(city);
        }
        else if (location is State state)
        {
            return _locationItemFactory.GetP2PState(state);
        }
        else if (location is Country country)
        {
            return _locationItemFactory.GetP2PCountry(country);
        }

        return null;
    }

    private ConnectionItemBase? CreateTorConnectionItem(ILocation location)
    {
        if (location is Server server)
        {
            return _locationItemFactory.GetTorServer(server);
        }
        else if (location is Country country)
        {
            return _locationItemFactory.GetTorCountry(country);
        }

        return null;
    }

    private ConnectionItemBase? CreateStandardConnectionItem(ILocation location)
    {
        if (location is Server server)
        {
            return _locationItemFactory.GetServer(server);
        }
        else if (location is City city)
        {
            return _locationItemFactory.GetCity(city);
        }
        else if (location is State state)
        {
            return _locationItemFactory.GetState(state);
        }
        else if (location is Country country)
        {
            return _locationItemFactory.GetCountry(country);
        }

        return null;
    }

    public void Receive(ConnectionStatusChangedMessage message)
    {
        InvalidateActiveConnection();
    }

    public void Receive(ServerListChangedMessage message)
    {
        InvalidateActiveConnection();
        InvalidateMaintenanceStates();
        InvalidateRestrictions();
    }
}