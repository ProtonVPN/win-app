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

using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Client.Common.UI.Assets.Icons.Base;
using ProtonVPN.Client.Common.UI.Assets.Icons.PathIcons;
using ProtonVPN.Client.Contracts.Enums;

namespace ProtonVPN.Client.Extensions;

public static class ConnectionGroupTypeExtensions
{
    public static bool IsInfoButtonVisible(this ConnectionGroupType groupType)
    {
        return groupType switch
        {
            ConnectionGroupType.SecureCoreCountries or
            ConnectionGroupType.P2PCountries or
            ConnectionGroupType.TorCountries or 
            ConnectionGroupType.Profiles => true,

            _ => false
        };
    }

    public static bool IsServerLoadInfoButtonVisible(this ConnectionGroupType groupType)
    {
        return groupType switch
        {
            ConnectionGroupType.Servers or
            ConnectionGroupType.FreeServers or
            ConnectionGroupType.P2PServers or
            ConnectionGroupType.TorServers or
            ConnectionGroupType.SecureCoreServers or
            ConnectionGroupType.GatewayServers => true,
            _ => false
        };
    }

    public static IconElement? GetIcon(this ConnectionGroupType groupType)
    {
        CustomPathIcon? icon = groupType switch
        {
            ConnectionGroupType.PinnedRecents => new PinFilled(),
            ConnectionGroupType.Recents => new ClockRotateLeft(),
            _ => null
        };

        if (icon != null)
        {
            icon.Size = PathIconSize.Pixels16;
        }

        return icon;
    }
}