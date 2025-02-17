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

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Client.Models.Connections;
using ProtonVPN.Client.Models.Connections.Countries;
using ProtonVPN.Client.Models.Connections.Gateways;
using ProtonVPN.Client.Models.Connections.Profiles;
using ProtonVPN.Client.Models.Connections.Recents;

namespace ProtonVPN.Client.Selectors;

public class ConnectionItemTemplateSelector : DataTemplateSelector
{
    public DataTemplate? GenericCountryLocationItemTemplate { get; set; }

    public DataTemplate? CountryLocationWithExpanderItemTemplate { get; set; }

    public DataTemplate? CountryLocationWithFlyoutItemTemplate { get; set; }

    public DataTemplate? CountryPairLocationItemTemplate { get; set; }

    public DataTemplate? StateLocationItemTemplate { get; set; }

    public DataTemplate? CityLocationItemTemplate { get; set; }

    public DataTemplate? ServerLocationItemTemplate { get; set; }

    public DataTemplate? GenericGatewayLocationItemTemplate { get; set; }

    public DataTemplate? GatewayLocationItemTemplate { get; set; }

    public DataTemplate? GatewayServerLocationItemTemplate { get; set; }

    public DataTemplate? RecentConnectionItemTemplate { get; set; }

    public DataTemplate? ProfileConnectionItemTemplate { get; set; }

    protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
    {
        DataTemplate? template = item switch
        {
            ProfileConnectionItem => ProfileConnectionItemTemplate,

            RecentConnectionItem => RecentConnectionItemTemplate,

            GenericGatewayLocationItem => GenericGatewayLocationItemTemplate,
            GatewayLocationItem => GatewayLocationItemTemplate,
            GatewayServerLocationItem => GatewayServerLocationItemTemplate,

            GenericCountryLocationItem => GenericCountryLocationItemTemplate,
            CountryLocationItem or
            P2PCountryLocationItem or
            SecureCoreCountryLocationItem => CountryLocationWithExpanderItemTemplate,
            TorCountryLocationItem => CountryLocationWithFlyoutItemTemplate,
            SecureCoreCountryPairLocationItem => CountryPairLocationItemTemplate,
            StateLocationItemBase => StateLocationItemTemplate,
            CityLocationItemBase => CityLocationItemTemplate,
            ServerLocationItemBase => ServerLocationItemTemplate,

            _ => throw new NotSupportedException($"Connection item {item} is not recognized"),
        };

        return template ?? throw new NotSupportedException("Connection item data template is undefined");
    }
}