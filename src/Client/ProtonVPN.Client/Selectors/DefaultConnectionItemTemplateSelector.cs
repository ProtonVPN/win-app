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

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Client.UI.Main.Settings.Pages.ConnectionPreferences;

namespace ProtonVPN.Client.Selectors;

public class DefaultConnectionItemTemplateSelector : DataTemplateSelector
{
    public DataTemplate? FastestConnectionItemTemplate { get; set; }
    public DataTemplate? RandomConnectionItemTemplate { get; set; }
    public DataTemplate? LastConnectionItemTemplate { get; set; }
    public DataTemplate? RecentConnectionItemTemplate { get; set; }
    public DataTemplate? SeparatorItemTemplate { get; set; }

    protected override DataTemplate? SelectTemplateCore(object item, DependencyObject container)
    {
        return item switch
        {
            DefaultConnectionItem observable => observable.ConnectionType switch
            {
                DefaultConnectionItemType.Fastest => FastestConnectionItemTemplate,
                DefaultConnectionItemType.Random => RandomConnectionItemTemplate,
                DefaultConnectionItemType.Last => LastConnectionItemTemplate,
                _ => null
            },
            RecentDefaultConnectionItem => RecentConnectionItemTemplate,
            DefaultConnectionSeparator => SeparatorItemTemplate,
            _ => null
        };
    }
}
