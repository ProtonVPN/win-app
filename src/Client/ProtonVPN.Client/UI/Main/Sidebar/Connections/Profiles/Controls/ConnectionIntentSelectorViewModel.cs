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

using CommunityToolkit.Mvvm.ComponentModel;
using ProtonVPN.Client.Common.Collections;
using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.Core.Enums;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Factories;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Enums;
using ProtonVPN.Client.Logic.Servers.Contracts.Messages;
using ProtonVPN.Client.Models;
using ProtonVPN.Client.Models.Connections;
using ProtonVPN.Client.Models.Connections.Countries;
using ProtonVPN.Client.Models.Connections.Gateways;
using ProtonVPN.Client.UI.Main.Sidebar.Connections.Profiles.Contracts;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Main.Sidebar.Connections.Profiles.Controls;

public partial class ConnectionIntentSelectorViewModel : ViewModelBase,
    IConnectionIntentSelector,
    IEventMessageReceiver<ServerListChangedMessage>
{
    private readonly IServersLoader _serversLoader;

    private readonly ILocationItemFactory _locationItemFactory;
    private readonly ICommonItemFactory _commonItemFactory;

    private ILocationIntent? _currentLocationIntent;
    private IFeatureIntent? _currentFeatureIntent;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FirstLevelHeader))]
    private FeatureItem? _selectedFeature;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSecondLevel))]
    [NotifyPropertyChangedFor(nameof(HasThirdLevel))]
    private ConnectionItemBase? _selectedFirstLevelLocationItem;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasThirdLevel))]
    private ConnectionItemBase? _selectedSecondLevelLocationItem;

    [ObservableProperty]
    private ConnectionItemBase? _selectedThirdLevelLocationItem;

    private IReadOnlyList<ConnectionItemBase> _gateways = [];
    private IReadOnlyList<ConnectionItemBase> _countries = [];
    private IReadOnlyList<ConnectionItemBase> _p2pCountries = [];
    private IReadOnlyList<ConnectionItemBase> _secureCoreCountries = [];

    private bool _isUpdatingFeatureSelection;
    private bool _isUpdatingLocationSelection;

    public SmartObservableCollection<ConnectionItemBase> FirstLevelLocationItems { get; } = [];

    public SmartObservableCollection<ConnectionItemBase> SecondLevelLocationItems { get; } = [];

    public SmartObservableCollection<ConnectionItemBase> ThirdLevelLocationItems { get; } = [];

    public SmartObservableCollection<FeatureItem> Features { get; } = [];

    public string FirstLevelHeader => SelectedFeature?.Feature switch
    {
        Feature.B2B => Localizer.Get("Connections_Gateway"),
        _ => Localizer.Get("Connections_Country"),
    };

    public string SecondLevelHeader => SecondLevelLocationItems.FirstOrDefault(i => i is not GenericFastestLocationItem) switch
    {
        SecureCoreCountryPairLocationItem => Localizer.Get("Connections_Country_Middle"),
        GatewayServerLocationItem => Localizer.Get("Connections_Gateways_Server"),
        StateLocationItem => Localizer.Get("Connections_State"),
        _ => Localizer.Get("Connections_City"),
    };

    public string ThirdLevelHeader => Localizer.Get("Server");

    public bool HasSecondLevel => SelectedFirstLevelLocationItem is IHostLocationItem;

    public bool HasThirdLevel => HasSecondLevel && SelectedSecondLevelLocationItem is IHostLocationItem;

    public ConnectionIntentSelectorViewModel(
        ILocalizationProvider localizationProvider,
        ILogger logger,
        IIssueReporter issueReporter,
        IServersLoader serversLoader,
        ILocationItemFactory locationItemFactory,
        ICommonItemFactory commonItemFactory)
        : base(localizationProvider,
               logger,
               issueReporter)
    {
        _serversLoader = serversLoader;
        _locationItemFactory = locationItemFactory;
        _commonItemFactory = commonItemFactory;

        InvalidateServers();
    }

    public IConnectionIntent GetConnectionIntent()
    {
        InvalidateCurrentFeatureIntent();
        InvalidateCurrentLocationIntent();

        return _currentLocationIntent != null
            ? new ConnectionIntent(_currentLocationIntent, _currentFeatureIntent)
            : ConnectionIntent.Default;
    }

    public void SetConnectionIntent(IConnectionIntent connectionIntent)
    {
        connectionIntent ??= ConnectionIntent.Default;

        _currentFeatureIntent = connectionIntent.Feature;

        InvalidateFeatureSelection();

        _currentLocationIntent = connectionIntent.Location;

        InvalidateLocationSelection();
    }

    public void Receive(ServerListChangedMessage message)
    {
        ExecuteOnUIThread(InvalidateServers);
    }

    private void InvalidateServers()
    {
        _gateways = GetGateways();
        _countries = GetCountries();
        _p2pCountries = GetP2PCountries();
        _secureCoreCountries = GetSecureCoreCountries();

        InvalidateFeatures();

        InvalidateFeatureSelection();
        InvalidateLocationSelection();
    }

    private IReadOnlyList<GatewayLocationItem> GetGateways()
    {
        return _serversLoader
            .GetGateways()
            .Select(_locationItemFactory.GetGateway)
            .ToList();
    }

    private IReadOnlyList<LocationItemBase> GetCountries()
    {
        IEnumerable<LocationItemBase> genericCountries =
        [
            _locationItemFactory.GetGenericCountry(CountriesConnectionType.All, ConnectionIntentKind.Fastest, false),
            _locationItemFactory.GetGenericCountry(CountriesConnectionType.All, ConnectionIntentKind.Fastest, true),
            _locationItemFactory.GetGenericCountry(CountriesConnectionType.All, ConnectionIntentKind.Random, false),
        ];

        IEnumerable<LocationItemBase> countries =
            _serversLoader.GetCountries()
                          .Select(_locationItemFactory.GetCountry);

        return genericCountries
            .Concat(countries)
            .ToList();
    }

    private IReadOnlyList<LocationItemBase> GetP2PCountries()
    {
        IEnumerable<LocationItemBase> genericCountries =
        [
            _locationItemFactory.GetGenericCountry(CountriesConnectionType.P2P, ConnectionIntentKind.Fastest, false),
            _locationItemFactory.GetGenericCountry(CountriesConnectionType.P2P, ConnectionIntentKind.Fastest, true),
            _locationItemFactory.GetGenericCountry(CountriesConnectionType.P2P, ConnectionIntentKind.Random, false),
        ];

        IEnumerable<LocationItemBase> countries =
            _serversLoader.GetCountriesByFeatures(ServerFeatures.P2P)
                          .Select(_locationItemFactory.GetP2PCountry);

        return genericCountries
            .Concat(countries)
            .ToList();
    }

    private IReadOnlyList<LocationItemBase> GetSecureCoreCountries()
    {
        IEnumerable<LocationItemBase> genericCountries =
        [
            _locationItemFactory.GetGenericCountry(CountriesConnectionType.SecureCore, ConnectionIntentKind.Fastest, false),
            _locationItemFactory.GetGenericCountry(CountriesConnectionType.SecureCore, ConnectionIntentKind.Fastest, true),
            _locationItemFactory.GetGenericCountry(CountriesConnectionType.SecureCore, ConnectionIntentKind.Random, false),
        ];

        IEnumerable<LocationItemBase> countries =
            _serversLoader.GetCountriesByFeatures(ServerFeatures.SecureCore)
                          .Select(_locationItemFactory.GetSecureCoreCountry);

        return genericCountries
            .Concat(countries)
            .ToList();
    }

    private void InvalidateFeatures()
    {
        List<Feature> features = new();

        if (_gateways.Any())
        {
            features.Add(Feature.B2B);
        }

        features.Add(Feature.None);

        if (_p2pCountries.Any())
        {
            features.Add(Feature.P2P);
        }

        if (_secureCoreCountries.Any())
        {
            features.Add(Feature.SecureCore);
        }

        Features.Reset(
            features.Select(_commonItemFactory.GetFeature));
    }

    private void InvalidateFeatureSelection()
    {
        try
        {
            _isUpdatingFeatureSelection = true;

            SelectedFeature = _currentFeatureIntent switch
            {
                B2BFeatureIntent when Features.Any(f => f.Feature == Feature.B2B) => Features.FirstOrDefault(f => f.Feature == Feature.B2B),
                P2PFeatureIntent when Features.Any(f => f.Feature == Feature.P2P) => Features.FirstOrDefault(f => f.Feature == Feature.P2P),
                SecureCoreFeatureIntent when Features.Any(f => f.Feature == Feature.SecureCore) => Features.FirstOrDefault(f => f.Feature == Feature.SecureCore),
                _ => Features.FirstOrDefault(f => f.Feature == Feature.None)
            };
        }
        finally
        {
            _isUpdatingFeatureSelection = false;
        }
    }

    private void InvalidateLocationSelection()
    {
        try
        {
            _isUpdatingLocationSelection = true;

            InvalidateFirstLevelLocations();

            SelectedFirstLevelLocationItem = _currentLocationIntent switch
            {
                CountryLocationIntent countryIntent when countryIntent.IsFastestCountry => FirstLevelLocationItems.OfType<GenericCountryLocationItem>().FirstOrDefault(c => c.IntentKind == ConnectionIntentKind.Fastest && !c.ExcludeMyCountry),
                CountryLocationIntent countryIntent when countryIntent.IsFastestCountryExcludingMine => FirstLevelLocationItems.OfType<GenericCountryLocationItem>().FirstOrDefault(c => c.IntentKind == ConnectionIntentKind.Fastest && c.ExcludeMyCountry),
                CountryLocationIntent countryIntent when countryIntent.IsRandomCountry => FirstLevelLocationItems.OfType<GenericCountryLocationItem>().FirstOrDefault(c => c.IntentKind == ConnectionIntentKind.Random && !c.ExcludeMyCountry),
                CountryLocationIntent countryIntent when countryIntent.IsRandomCountryExcludingMine => FirstLevelLocationItems.OfType<GenericCountryLocationItem>().FirstOrDefault(c => c.IntentKind == ConnectionIntentKind.Random && c.ExcludeMyCountry),
                CountryLocationIntent countryIntent => FirstLevelLocationItems.OfType<CountryLocationItemBase>().FirstOrDefault(c => c.ExitCountryCode == countryIntent.CountryCode),
                GatewayLocationIntent gatewayIntent => FirstLevelLocationItems.OfType<GatewayLocationItem>().FirstOrDefault(g => g.Gateway.Name == gatewayIntent.GatewayName),
                _ => null
            };
            SelectedFirstLevelLocationItem ??= FirstLevelLocationItems.FirstOrDefault();

            SelectedSecondLevelLocationItem = _currentLocationIntent switch
            {
                _ when _currentFeatureIntent is SecureCoreFeatureIntent secureCoreIntent => SecondLevelLocationItems.OfType<SecureCoreCountryPairLocationItem>().FirstOrDefault(cp => cp.CountryPair.EntryCountry == secureCoreIntent.EntryCountryCode),
                CityLocationIntent cityIntent => SecondLevelLocationItems.OfType<CityLocationItemBase>().FirstOrDefault(c => c.City.Name == cityIntent.City),
                StateLocationIntent stateIntent => SecondLevelLocationItems.OfType<StateLocationItemBase>().FirstOrDefault(s => s.State.Name == stateIntent.State),
                GatewayServerLocationIntent gatewayServerIntent => SecondLevelLocationItems.OfType<GatewayServerLocationItem>().FirstOrDefault(gs => gs.Server.Id == gatewayServerIntent.Id),
                _ => null
            };
            SelectedSecondLevelLocationItem ??= SecondLevelLocationItems.FirstOrDefault();

            SelectedThirdLevelLocationItem = _currentLocationIntent switch
            {
                ServerLocationIntent serverIntent => ThirdLevelLocationItems.OfType<ServerLocationItemBase>().FirstOrDefault(s => s.Server.Id == serverIntent.Id),
                _ => null
            };
            SelectedThirdLevelLocationItem ??= ThirdLevelLocationItems.FirstOrDefault();
        }
        finally
        {
            _isUpdatingLocationSelection = false;
        }
    }

    private void InvalidateFirstLevelLocations()
    {
        IEnumerable<ConnectionItemBase> items = SelectedFeature?.Feature switch
        {
            Feature.B2B => _gateways,
            Feature.P2P => _p2pCountries,
            Feature.SecureCore => _secureCoreCountries,
            _ => _countries,
        };

        FirstLevelLocationItems.Reset(
            items.OrderBy(g => g.FirstSortProperty)
                 .ThenBy(g => g.SecondSortProperty));
    }

    private void InvalidateSecondLevelLocations()
    {
        List<ConnectionItemBase> locations = [];

        if (SelectedFirstLevelLocationItem is IHostLocationItem hostLocationItem)
        {
            hostLocationItem.FetchSubItems();

            ConnectionItemBase fastestLocation =
                _locationItemFactory.GetGenericFastestLocation(hostLocationItem.GroupType, hostLocationItem.LocationIntent);

            locations.Add(fastestLocation);
            locations.AddRange(hostLocationItem.SubItems.ToList());
        }

        SecondLevelLocationItems.Reset(locations);

        OnPropertyChanged(nameof(SecondLevelHeader));
    }

    private void InvalidateThirdLevelLocations()
    {
        List<ConnectionItemBase> locations = [];

        if (SelectedSecondLevelLocationItem is IHostLocationItem hostLocationItem)
        {
            hostLocationItem.FetchSubItems();
            
            ConnectionItemBase fastestLocation =
                _locationItemFactory.GetGenericFastestLocation(hostLocationItem.GroupType, hostLocationItem.LocationIntent);

            locations.Add(fastestLocation);
            locations.AddRange(hostLocationItem.SubItems.OfType<ServerLocationItemBase>().ToList());
        }

        ThirdLevelLocationItems.Reset(locations);
    }

    private void InvalidateCurrentFeatureIntent()
    {
        _currentFeatureIntent = SelectedFeature?.Feature switch
        {
            // TODO: feature intent is not updated when country pair changes.
            Feature.SecureCore when SelectedSecondLevelLocationItem is SecureCoreCountryPairLocationItem countryPair => new SecureCoreFeatureIntent(countryPair.CountryPair.EntryCountry),
            Feature.SecureCore => new SecureCoreFeatureIntent(),
            Feature.Tor => new TorFeatureIntent(),
            Feature.P2P => new P2PFeatureIntent(),
            Feature.B2B => new B2BFeatureIntent(),
            _ => null,
        };
    }

    private void InvalidateCurrentLocationIntent()
    {
        _currentLocationIntent =
            HasThirdLevel && SelectedThirdLevelLocationItem is LocationItemBase thirdLevelLocationItem
                ? thirdLevelLocationItem.LocationIntent
                : HasSecondLevel && SelectedSecondLevelLocationItem is LocationItemBase secondLevelLocationItem
                    ? secondLevelLocationItem.LocationIntent
                    : SelectedFirstLevelLocationItem is LocationItemBase firstLevelLocationItem
                        ? firstLevelLocationItem.LocationIntent 
                        : CountryLocationIntent.Fastest;

    }

    partial void OnSelectedFeatureChanged(FeatureItem? value)
    {
        if (!_isUpdatingFeatureSelection)
        {
            InvalidateCurrentFeatureIntent();
            InvalidateCurrentLocationIntent();

            InvalidateLocationSelection();
        }
    }

    partial void OnSelectedFirstLevelLocationItemChanged(ConnectionItemBase? value)
    {
        InvalidateSecondLevelLocations();

        if (!_isUpdatingLocationSelection)
        {
            SelectedSecondLevelLocationItem = SecondLevelLocationItems.FirstOrDefault();
        }
    }

    partial void OnSelectedSecondLevelLocationItemChanged(ConnectionItemBase? value)
    {
        InvalidateThirdLevelLocations();

        if (!_isUpdatingLocationSelection)
        {
            SelectedThirdLevelLocationItem = ThirdLevelLocationItems.FirstOrDefault();
        }
    }
}