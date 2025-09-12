/*
 * Copyright (c) 2025 Proton AG
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

using ProtonVPN.Client.Core.Enums;
using ProtonVPN.Client.Contracts.Enums;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.Client.Models.Connections;
using ProtonVPN.Client.Models.Connections.Countries;
using ProtonVPN.Client.Models.Connections.Gateways;

namespace ProtonVPN.Client.Factories;

public class LocationItemFactory : ILocationItemFactory
{
    private readonly ILocalizationProvider _localizer;
    private readonly IServersLoader _serversLoader;
    private readonly IConnectionManager _connectionManager;
    private readonly IMainWindowOverlayActivator _overlayActivator;
    private readonly IUpsellCarouselWindowActivator _upsellCarouselWindowActivator;
    private readonly IConnectionGroupFactory _connectionGroupFactory;

    public LocationItemFactory(
        ILocalizationProvider localizer,
        IServersLoader serversLoader,
        IConnectionManager connectionManager,
        IMainWindowOverlayActivator overlayActivator,
        IUpsellCarouselWindowActivator upsellCarouselWindowActivator,
        IConnectionGroupFactory connectionGroupFactory)
    {
        _localizer = localizer;
        _serversLoader = serversLoader;
        _connectionManager = connectionManager;
        _overlayActivator = overlayActivator;
        _upsellCarouselWindowActivator = upsellCarouselWindowActivator;
        _connectionGroupFactory = connectionGroupFactory;
    }

    public GenericCountryLocationItem GetGenericCountry(
        CountriesConnectionType connectionType,
        SelectionStrategy intentKind,
        bool excludeMyCountry,
        bool isSearchItem = false)
    {
        return new GenericCountryLocationItem(
            _localizer,
            _serversLoader,
            _connectionManager,
            _upsellCarouselWindowActivator,
            connectionType,
            intentKind,
            excludeMyCountry,
            isSearchItem);
    }

    public GenericFastestLocationItem GetGenericFastestLocation(
        ConnectionGroupType groupType,
        ILocationIntent locationIntent)
    {
        return new GenericFastestLocationItem(
            _localizer,
            _serversLoader,
            _connectionManager,
            _upsellCarouselWindowActivator,
            groupType,
            locationIntent);
    }

    public CountryLocationItem GetCountry(Country country, bool isSearchItem = false)
    {
        return new CountryLocationItem(
            _localizer,
            _serversLoader,
            _connectionManager,
            _overlayActivator,
            _upsellCarouselWindowActivator,
            _connectionGroupFactory,
            this,
            country,
            isSearchItem);
    }

    public StateLocationItem GetState(State state, bool showBaseLocation = false, bool isSearchItem = false)
    {
        return new StateLocationItem(
            _localizer,
            _serversLoader,
            _connectionManager,
            _overlayActivator,
            _upsellCarouselWindowActivator,
            _connectionGroupFactory,
            this,
            state,
            showBaseLocation,
            isSearchItem);
    }

    public CityLocationItem GetCity(City city, bool showBaseLocation = false, bool isSearchItem = false)
    {
        return new CityLocationItem(
            _localizer,
            _serversLoader,
            _connectionManager,
            _overlayActivator,
            _upsellCarouselWindowActivator,
            _connectionGroupFactory,
            this,
            city,
            showBaseLocation,
            isSearchItem);
    }

    public ServerLocationItem GetServer(Server server, bool isSearchItem = false)
    {
        return new ServerLocationItem(
            _localizer,
            _serversLoader,
            _connectionManager,
            _upsellCarouselWindowActivator,
            server,
            isSearchItem);
    }

    public SecureCoreCountryLocationItem GetSecureCoreCountry(Country country, bool isSearchItem = false)
    {
        return new SecureCoreCountryLocationItem(
            _localizer,
            _serversLoader,
            _connectionManager,
            _overlayActivator,
            _upsellCarouselWindowActivator,
            _connectionGroupFactory,
            this,
            country,
            isSearchItem);
    }

    public SecureCoreCountryPairLocationItem GetSecureCoreCountryPair(SecureCoreCountryPair countryPair, bool isSearchItem = false)
    {
        return new SecureCoreCountryPairLocationItem(
            _localizer,
            _serversLoader,
            _connectionManager,
            _upsellCarouselWindowActivator,
            countryPair,
            isSearchItem);
    }

    public P2PCountryLocationItem GetP2PCountry(Country country, bool isSearchItem = false)
    {
        return new P2PCountryLocationItem(
            _localizer,
            _serversLoader,
            _connectionManager,
            _overlayActivator,
            _upsellCarouselWindowActivator,
            _connectionGroupFactory,
            this,
            country,
            isSearchItem);
    }

    public P2PStateLocationItem GetP2PState(State state, bool showBaseLocation = false, bool isSearchItem = false)
    {
        return new P2PStateLocationItem(
            _localizer,
            _serversLoader,
            _connectionManager,
            _overlayActivator,
            _upsellCarouselWindowActivator,
            _connectionGroupFactory,
            this,
            state,
            showBaseLocation,
            isSearchItem);
    }

    public P2PCityLocationItem GetP2PCity(City city, bool showBaseLocation = false, bool isSearchItem = false)
    {
        return new P2PCityLocationItem(
            _localizer,
            _serversLoader,
            _connectionManager,
            _overlayActivator,
            _upsellCarouselWindowActivator,
            _connectionGroupFactory,
            this,
            city,
            showBaseLocation,
            isSearchItem);
    }

    public P2PServerLocationItem GetP2PServer(Server server, bool isSearchItem = false)
    {
        return new P2PServerLocationItem(
            _localizer,
            _serversLoader,
            _connectionManager,
            _upsellCarouselWindowActivator,
            server,
            isSearchItem);
    }

    public TorCountryLocationItem GetTorCountry(Country country, bool isSearchItem = false)
    {
        return new TorCountryLocationItem(
            _localizer,
            _serversLoader,
            _connectionManager,
            _overlayActivator,
            _upsellCarouselWindowActivator,
            _connectionGroupFactory,
            this,
            country,
            isSearchItem);
    }

    public TorServerLocationItem GetTorServer(Server server, bool isSearchItem = false)
    {
        return new TorServerLocationItem(
            _localizer,
            _serversLoader,
            _connectionManager,
            _upsellCarouselWindowActivator,
            server,
            isSearchItem);
    }

    public GenericGatewayLocationItem GetGenericGateway(SelectionStrategy intentKind)
    {
        return new GenericGatewayLocationItem(
            _localizer, 
            _serversLoader,
            _connectionManager, 
            _upsellCarouselWindowActivator, 
            intentKind);
    }

    public GatewayLocationItem GetGateway(Gateway gateway)
    {
        return new GatewayLocationItem(
            _localizer,
            _serversLoader,
            _connectionManager,
            _overlayActivator,
            _upsellCarouselWindowActivator,
            _connectionGroupFactory,
            this,
            gateway);
    }

    public GatewayServerLocationItem GetGatewayServer(Server server)
    {
        return new GatewayServerLocationItem(
            _localizer,
            _serversLoader,
            _connectionManager,
            _upsellCarouselWindowActivator,
            server);
    }
}