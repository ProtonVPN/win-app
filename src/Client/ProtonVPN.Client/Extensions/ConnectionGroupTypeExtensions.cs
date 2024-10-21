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
using ProtonVPN.Client.Logic.Auth.Contracts.Enums;

namespace ProtonVPN.Client.Extensions;

public static class ConnectionGroupTypeExtensions
{
    public static ModalSources GetUpsellModalSources(this ConnectionGroupType groupType)
    {
        return groupType switch
        {
            ConnectionGroupType.Countries or
            ConnectionGroupType.Cities or
            ConnectionGroupType.States or
            ConnectionGroupType.Servers or
            ConnectionGroupType.FreeServers => ModalSources.Countries,

            ConnectionGroupType.SecureCoreCountries or
            ConnectionGroupType.SecureCoreCountryPairs or
            ConnectionGroupType.SecureCoreServers => ModalSources.SecureCore,

            ConnectionGroupType.P2PCountries or
            ConnectionGroupType.P2PCities or
            ConnectionGroupType.P2PStates or
            ConnectionGroupType.P2PServers => ModalSources.P2P,

            ConnectionGroupType.TorCountries or
            ConnectionGroupType.TorServers => ModalSources.Countries, // TODO: Should we create a Tor modal source?

            ConnectionGroupType.Gateways or
            ConnectionGroupType.GatewayServers => ModalSources.Undefined, // Should never be triggered, user who can see gateways have access to them.

            ConnectionGroupType.Recents or
            ConnectionGroupType.PinnedRecents => ModalSources.Countries, // TODO: Should we create a Recents modal source?

            ConnectionGroupType.Profiles => ModalSources.Profiles,

            _ => ModalSources.Undefined
        };
    }

    public static bool IsInfoButtonVisible(this ConnectionGroupType groupType)
    {
        return groupType switch
        {
            ConnectionGroupType.SecureCoreCountries or
            ConnectionGroupType.P2PCountries or
            ConnectionGroupType.TorCountries => true,
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

    public static string GetLocalizationKey(this ConnectionGroupType groupType)
    {
        return groupType switch
        {
            ConnectionGroupType.Countries => "Connections_Countries",
            ConnectionGroupType.States => "Connections_States",
            ConnectionGroupType.Cities => "Connections_Cities",
            ConnectionGroupType.Servers => "Connections_Servers",
            ConnectionGroupType.FreeServers => "Connections_Free_Servers",
            ConnectionGroupType.SecureCoreCountries => "Connections_SecureCore_Countries",
            ConnectionGroupType.SecureCoreCountryPairs or
            ConnectionGroupType.SecureCoreServers => "Connections_SecureCore_Servers",
            ConnectionGroupType.P2PCountries => "Connections_P2P_Countries",
            ConnectionGroupType.P2PStates => "Connections_P2P_States",
            ConnectionGroupType.P2PCities => "Connections_P2P_Cities",
            ConnectionGroupType.P2PServers => "Connections_P2P_Servers",
            ConnectionGroupType.TorCountries => "Connections_Tor_Countries",
            ConnectionGroupType.TorServers => "Connections_Tor_Servers",
            ConnectionGroupType.Gateways => "Connections_Gateways",
            ConnectionGroupType.GatewayServers => "Connections_Gateways_Servers",
            ConnectionGroupType.PinnedRecents => "Connections_Recents_Pinned",
            ConnectionGroupType.Recents => "Connections_Recents",
            ConnectionGroupType.Profiles => "Connections_Profiles",
            _ => throw new NotSupportedException($"Group type '{groupType}' is not supported.")
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