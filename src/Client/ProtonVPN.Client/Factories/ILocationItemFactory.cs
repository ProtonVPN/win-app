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

using ProtonVPN.Client.Contracts.Enums;
using ProtonVPN.Client.Core.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.Client.Models.Connections;
using ProtonVPN.Client.Models.Connections.Countries;
using ProtonVPN.Client.Models.Connections.Gateways;

namespace ProtonVPN.Client.Factories;

public interface ILocationItemFactory
{
    GenericCountryLocationItem GetGenericCountry(CountriesConnectionType connectionType, SelectionStrategy intentKind, bool excludeMyCountry, bool isSearchItem = false);

    GenericFastestLocationItem GetGenericFastestLocation(ConnectionGroupType groupType, ILocationIntent locationIntent);

    CountryLocationItem GetCountry(Country country, bool isSearchItem = false);

    StateLocationItem GetState(State state, bool showBaseLocation = false, bool isSearchItem = false);

    CityLocationItem GetCity(City city, bool showBaseLocation = false, bool isSearchItem = false);

    ServerLocationItem GetServer(Server server, bool isSearchItem = false);

    SecureCoreCountryLocationItem GetSecureCoreCountry(Country country, bool isSearchItem = false);

    SecureCoreCountryPairLocationItem GetSecureCoreCountryPair(SecureCoreCountryPair countryPair, bool isSearchItem = false);

    P2PCountryLocationItem GetP2PCountry(Country country, bool isSearchItem = false);

    P2PStateLocationItem GetP2PState(State state, bool showBaseLocation = false, bool isSearchItem = false);

    P2PCityLocationItem GetP2PCity(City city, bool showBaseLocation = false, bool isSearchItem = false);

    P2PServerLocationItem GetP2PServer(Server server, bool isSearchItem = false);

    TorCountryLocationItem GetTorCountry(Country country, bool isSearchItem = false);

    TorServerLocationItem GetTorServer(Server server, bool isSearchItem = false);

    GenericGatewayLocationItem GetGenericGateway(SelectionStrategy intentKind);

    GatewayLocationItem GetGateway(Gateway gateway);

    GatewayServerLocationItem GetGatewayServer(Server server);
}