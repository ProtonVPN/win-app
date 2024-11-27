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

namespace ProtonVPN.Client.Contracts.Enums;

public enum ConnectionGroupType
{
    Profiles,

    Gateways,
    GatewayServers,

    SecureCoreCountries,
    SecureCoreCountryPairs,
    P2PCountries,
    TorCountries,
    Countries,

    P2PStates,
    States,

    P2PCities,
    Cities,

    SecureCoreServers,
    P2PServers,
    TorServers,
    Servers,
    FreeServers,

    PinnedRecents,
    Recents
}