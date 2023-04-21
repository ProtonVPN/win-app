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

using System.Collections.ObjectModel;
using Microsoft.UI.Xaml.Controls;

namespace ProtonVPN.Gui.Models;

public class NavigationPage
{
    public NavigationPage(string name, string iconCode, Type pageKeyType)
    {
        Name = name;
        PageKeyType = pageKeyType;

        if (!string.IsNullOrEmpty(iconCode))
        {
            Icon = new FontIcon() { Glyph = char.ConvertFromUtf32(Convert.ToInt32(iconCode, 16)) };
        }

        Children = new ObservableCollection<NavigationPage>();
    }

    public ObservableCollection<NavigationPage> Children { get; set; }

    public IconElement Icon { get; set; }

    public string Name { get; set; }

    public Type PageKeyType { get; set; }
}