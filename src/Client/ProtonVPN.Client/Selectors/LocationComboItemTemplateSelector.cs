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

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Client.Models.Connections;
using ProtonVPN.Client.Models.Connections.Countries;
using ProtonVPN.Client.Models.Connections.Gateways;

namespace ProtonVPN.Client.Selectors;

public class LocationComboItemTemplateSelector : DataTemplateSelector
{
    public DataTemplate? GenericCountryLocationItemTemplate { get; set; }

    public DataTemplate? CountryLocationItemTemplate { get; set; }

    public DataTemplate? StateOrCityLocationItemTemplate { get; set; }

    public DataTemplate? ServerLocationItemTemplate { get; set; }

    public DataTemplate? GenericGatewayLocationItemTemplate { get; set; }

    public DataTemplate? GatewayLocationItemTemplate { get; set; }

    public DataTemplate? GatewayServerLocationItemTemplate { get; set; }

    public DataTemplate? CountryPairLocationItemTemplate { get; set; }

    public DataTemplate? GenericFastestLocationItemTemplate { get; set; }

    protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
    {
        DataTemplate? template = item switch
        {
            GenericGatewayLocationItem => GenericGatewayLocationItemTemplate,
            GatewayLocationItem => GatewayLocationItemTemplate,
            GatewayServerLocationItem => GatewayServerLocationItemTemplate,

            GenericCountryLocationItem => GenericCountryLocationItemTemplate,
            CountryLocationItemBase => CountryLocationItemTemplate,
            SecureCoreCountryPairLocationItem => CountryPairLocationItemTemplate,
            StateLocationItemBase or
            CityLocationItemBase => StateOrCityLocationItemTemplate,
            ServerLocationItemBase => ServerLocationItemTemplate,
            GenericFastestLocationItem => GenericFastestLocationItemTemplate,
            _ => throw new NotSupportedException($"Location item {item} is not recognized"),
        };

        return template ?? throw new NotSupportedException("Location item data template is undefined");
    }
}