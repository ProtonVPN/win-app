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

namespace ProtonVPN.Client.UI.Home.ConnectionCard.Selectors;

public class ConnectionCardTemplateSelector : DataTemplateSelector
{
    public DataTemplate? ConnectionCardTemplate { get; set; }

    public DataTemplate? FreeConnectionCardTemplate { get; set; }

    protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
    {
        bool isPaidUser = Convert.ToBoolean(item);

        DataTemplate? template = isPaidUser
            ? ConnectionCardTemplate
            : FreeConnectionCardTemplate;

        return template ?? throw new InvalidOperationException("Connection card data template is undefined");
    }
}