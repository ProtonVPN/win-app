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

using ProtonVPN.StatisticalEvents.Contracts.Dimensions;
using ProtonVPN.StatisticalEvents.Dimensions.Mappers.Bases;

namespace ProtonVPN.StatisticalEvents.Dimensions.Mappers;

public class VpnTriggerDimensionMapper : DimensionMapperBase, IVpnTriggerDimensionMapper
{
    private const string QUICK = "quick"; // Not used on Windows
    private const string CONNECTION_CARD = "connection_card";
    private const string CHANGE_SERVER = "change_server";
    private const string RECENT = "recent";
    private const string PIN = "pin";
    private const string COUNTRIES_COUNTRY = "countries_country";
    private const string COUNTRIES_STATE = "countries_state";
    private const string COUNTRIES_CITY = "countries_city";
    private const string COUNTRIES_SERVER = "countries_server";
    private const string SEARCH_COUNTRY = "search_country";
    private const string SEARCH_STATE = "search_state";
    private const string SEARCH_CITY = "search_city";
    private const string SEARCH_SERVER = "search_server";
    private const string GATEWAYS_GATEWAY = "gateways_gateway";
    private const string GATEWAYS_SERVER = "gateways_server";
    private const string COUNTRY = "country"; // Not used on Windows
    private const string SERVER = "server"; // Not used on Windows
    private const string PROFILE = "profile";
    private const string MAP = "map";
    private const string TRAY = "tray";
    private const string WIDGET = "widget"; // Not used on Windows
    private const string AUTO = "auto";
    private const string NEW_CONNECTION = "new_connection";
    private const string EXIT = "exit";
    private const string SIGNOUT = "signout";

    public string Map(VpnTriggerDimension? vpnTrigger)
    {
        return vpnTrigger switch
        {
            VpnTriggerDimension.ConnectionCard => CONNECTION_CARD,
            VpnTriggerDimension.ChangeServer => CHANGE_SERVER,
            VpnTriggerDimension.Recent => RECENT,
            VpnTriggerDimension.Pin => PIN,
            VpnTriggerDimension.CountriesCountry => COUNTRIES_COUNTRY,
            VpnTriggerDimension.CountriesState => COUNTRIES_STATE,
            VpnTriggerDimension.CountriesCity => COUNTRIES_CITY,
            VpnTriggerDimension.CountriesServer => COUNTRIES_SERVER,
            VpnTriggerDimension.SearchCountry => SEARCH_COUNTRY,
            VpnTriggerDimension.SearchState => SEARCH_STATE,
            VpnTriggerDimension.SearchCity => SEARCH_CITY,
            VpnTriggerDimension.SearchServer => SEARCH_SERVER,
            VpnTriggerDimension.GatewaysGateway => GATEWAYS_GATEWAY,
            VpnTriggerDimension.GatewaysServer => GATEWAYS_SERVER,
            VpnTriggerDimension.Profile => PROFILE,
            VpnTriggerDimension.Map => MAP,
            VpnTriggerDimension.Tray => TRAY,
            VpnTriggerDimension.Auto => AUTO,
            VpnTriggerDimension.NewConnection => NEW_CONNECTION,
            VpnTriggerDimension.Exit => EXIT,
            VpnTriggerDimension.Signout => SIGNOUT,
            VpnTriggerDimension.Undefined => NOT_AVAILABLE,
            _ => NOT_AVAILABLE
        };
    }
}