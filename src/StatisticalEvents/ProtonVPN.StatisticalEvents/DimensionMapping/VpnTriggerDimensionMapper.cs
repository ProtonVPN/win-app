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

namespace ProtonVPN.StatisticalEvents.DimensionMapping;

public class VpnTriggerDimensionMapper : DimensionMapperBase, IDimensionMapper<VpnTriggerDimension?>
{
    public string Map(VpnTriggerDimension? vpnTrigger)
    {
        return vpnTrigger switch
        {
            VpnTriggerDimension.ConnectionCard => "connection_card",
            VpnTriggerDimension.ChangeServer => "change_server",
            VpnTriggerDimension.Recent => "recent",
            VpnTriggerDimension.Pin => "pin",
            VpnTriggerDimension.CountriesCountry => "countries_country",
            VpnTriggerDimension.CountriesState => "countries_state",
            VpnTriggerDimension.CountriesCity => "countries_city",
            VpnTriggerDimension.CountriesServer => "countries_server",
            VpnTriggerDimension.SearchCountry => "search_country",
            VpnTriggerDimension.SearchState => "search_state",
            VpnTriggerDimension.SearchCity => "search_city",
            VpnTriggerDimension.SearchServer => "search_server",
            VpnTriggerDimension.GatewaysGateway => "gateways_gateway",
            VpnTriggerDimension.GatewaysServer => "gateways_server",
            VpnTriggerDimension.Profile => "profile",
            VpnTriggerDimension.Map => "map",
            VpnTriggerDimension.Tray => "tray",
            VpnTriggerDimension.Auto => "auto",
            VpnTriggerDimension.NewConnection => "new_connection",
            VpnTriggerDimension.Exit => "exit",
            VpnTriggerDimension.Signout => "signout",
            _ => NOT_AVAILABLE
        };
    }
}