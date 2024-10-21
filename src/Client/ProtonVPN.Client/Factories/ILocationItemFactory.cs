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

using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.Client.Contracts.Enums;
using ProtonVPN.Client.Models.Connections.Countries;
using ProtonVPN.Client.Models.Connections.Gateways;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using ProtonVPN.Client.Models.Connections;

namespace ProtonVPN.Client.Factories;

public interface ILocationItemFactory
{
    GenericCountryLocationItem GetGenericCountry(CountriesConnectionType connectionType, ConnectionIntentKind intentKind, bool excludeMyCountry);

    GenericFastestLocationItem GetGenericFastestLocation(ConnectionGroupType groupType, ILocationIntent locationIntent);

    CountryLocationItem GetCountry(string exitCountryCode);

    StateLocationItem GetState(State state, bool showBaseLocation = false);

    CityLocationItem GetCity(City city, bool showBaseLocation = false);

    ServerLocationItem GetServer(Server server);

    SecureCoreCountryLocationItem GetSecureCoreCountry(string exitCountryCode);

    SecureCoreCountryPairLocationItem GetSecureCoreCountryPair(SecureCoreCountryPair countryPair);

    P2PCountryLocationItem GetP2PCountry(string exitCountryCode);

    P2PStateLocationItem GetP2PState(State state, bool showBaseLocation = false);

    P2PCityLocationItem GetP2PCity(City city, bool showBaseLocation = false);

    P2PServerLocationItem GetP2PServer(Server server);

    TorCountryLocationItem GetTorCountry(string exitCountryCode);

    TorServerLocationItem GetTorServer(Server server);

    GatewayLocationItem GetGateway(string gateway);

    GatewayServerLocationItem GetGatewayServer(Server server);
}