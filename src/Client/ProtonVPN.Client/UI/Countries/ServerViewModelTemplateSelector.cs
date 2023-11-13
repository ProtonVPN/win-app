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

namespace ProtonVPN.Client.UI.Countries;

public class ServerViewModelTemplateSelector : DataTemplateSelector
{
    public DataTemplate SecureCoreServerTemplate { get; set; }

    public DataTemplate ServerTemplate { get; set; }

    protected override DataTemplate SelectTemplateCore(object item)
    {
        if (item is SecureCoreServerViewModel)
        {
            return SecureCoreServerTemplate;
        }

        return item is ServerViewModel ? ServerTemplate : null;
    }
}