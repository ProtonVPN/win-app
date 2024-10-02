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

using ProtonVPN.Client.Logic.Auth.Contracts.Enums;
using ProtonVPN.Client.Legacy.UI.Connections.Common.Enums;

namespace ProtonVPN.Client.Legacy.UI.Connections.Common.Extensions;

public static class GroupLocationTypeExtensions
{
    public static ModalSources GetUpsellModalSources(this GroupLocationType groupType)
    {
        return groupType switch
        {
            GroupLocationType.Countries or
            GroupLocationType.Cities or
            GroupLocationType.States or
            GroupLocationType.Servers or
            GroupLocationType.FreeServers => ModalSources.Countries,

            GroupLocationType.SecureCoreCountries or
            GroupLocationType.SecureCoreCountryPairs or
            GroupLocationType.SecureCoreServers => ModalSources.SecureCore,

            GroupLocationType.P2PCountries or
            GroupLocationType.P2PCities or
            GroupLocationType.P2PStates or
            GroupLocationType.P2PServers => ModalSources.P2P,

            GroupLocationType.TorCountries or
            GroupLocationType.TorServers => ModalSources.Countries,

            _ => ModalSources.Undefined
        };
    }

    public static bool IsInfoButtonVisible(this GroupLocationType groupType)
    {
        return groupType switch
        {
            GroupLocationType.SecureCoreCountries or
            GroupLocationType.P2PCountries or
            GroupLocationType.P2PCities or
            GroupLocationType.P2PStates or
            GroupLocationType.TorCountries => true,

            _ => false
        };
    }

    public static bool IsServerLoadInfoButtonVisible(this GroupLocationType groupType)
    {
        return groupType switch
        {
            GroupLocationType.Servers or
            GroupLocationType.FreeServers or
            GroupLocationType.P2PServers or
            GroupLocationType.TorServers or
            GroupLocationType.SecureCoreServers or
            GroupLocationType.GatewayServers => true,

            _ => false
        };
    }
}