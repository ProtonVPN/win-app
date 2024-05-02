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
using ProtonVPN.Client.UI.Connections.Common.Items;

namespace ProtonVPN.Client.UI.Connections.Common.Selectors;

public class LocationItemTemplateSelector : DataTemplateSelector
{
    public DataTemplate? CountryLocationWithNavigationItemTemplate { get; set; }

    public DataTemplate? CountryLocationWithFlyoutItemTemplate { get; set; }

    public DataTemplate? GatewayLocationItemTemplate { get; set; }

    public DataTemplate? GatewayServerLocationItemTemplate { get; set; }

    public DataTemplate? CountryPairLocationItemTemplate { get; set; }

    public DataTemplate? CityLocationItemTemplate { get; set; }

    public DataTemplate? ServerLocationItemTemplate { get; set; }

    protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
    {
        DataTemplate? template = item switch
        {
            CountryLocationItem or
            P2PCountryLocationItem => CountryLocationWithNavigationItemTemplate,
            SecureCoreCountryLocationItem or
            TorCountryLocationItem => CountryLocationWithFlyoutItemTemplate,
            SecureCoreCountryPairLocationItem => CountryPairLocationItemTemplate,
            GatewayLocationItem => GatewayLocationItemTemplate,
            GatewayServerLocationItem => GatewayServerLocationItemTemplate,
            CityLocationItemBase => CityLocationItemTemplate,
            ServerLocationItemBase => ServerLocationItemTemplate,
            _ => throw new NotSupportedException($"Location item {item} is not recognized"),
        };

        return template ?? throw new NotSupportedException("Location item data template is undefined");
    }
}