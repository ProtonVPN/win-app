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
using ProtonVPN.Client.Models.Features.SplitTunneling;

namespace ProtonVPN.Client.Selectors;

public class SplitTunnelingItemTemplateSelector : DataTemplateSelector
{
    public DataTemplate? AppSplitTunnelingItemTemplate { get; set; }

    public DataTemplate? IpAddressSplitTunnelingItemTemplate { get; set; }

    protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
    {
        DataTemplate? template = item switch
        {
            AppSplitTunnelingItem => AppSplitTunnelingItemTemplate,
            IpAddressSplitTunnelingItem => IpAddressSplitTunnelingItemTemplate,

            _ => throw new NotSupportedException($"Split Tunneling item {item} is not recognized"),
        };

        return template ?? throw new NotSupportedException("Split Tunneling item data template is undefined");
    }
}