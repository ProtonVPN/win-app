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
using ProtonVPN.Client.UI.Sidebar.Bases;

namespace ProtonVPN.Client.UI.Sidebar.Selectors;

public class SidebarItemTemplateSelector : DataTemplateSelector
{
    public DataTemplate? SeparatorItemTemplate { get; set; }

    public DataTemplate? AccountItemTemplate { get; set; }

    public DataTemplate? PortForwardingItemTemplate { get; set; }

    public DataTemplate? FeatureNavigationItemTemplate { get; set; }

    public DataTemplate? NavigationItemTemplate { get; set; }

    public DataTemplate? InteractiveItemTemplate { get; set; }

    public DataTemplate? HeaderItemTemplate { get; set; }

    protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
    {
        DataTemplate? template = item switch
        {
            SidebarSeparatorViewModel => SeparatorItemTemplate,
            SidebarAccountViewModel => AccountItemTemplate,
            SidebarPortForwardingViewModel => PortForwardingItemTemplate,
            SidebarFeatureNavigationItemViewModelBase => FeatureNavigationItemTemplate,
            SidebarNavigationItemViewModelBase => NavigationItemTemplate,
            SidebarInteractiveItemViewModelBase => InteractiveItemTemplate,
            SidebarHeaderViewModelBase => HeaderItemTemplate,
            _ => throw new NotImplementedException($"Sidebar item {item} is not recognized"),
        };

        return template ?? throw new InvalidOperationException("Sidebar item data template is undefined");
    }
}