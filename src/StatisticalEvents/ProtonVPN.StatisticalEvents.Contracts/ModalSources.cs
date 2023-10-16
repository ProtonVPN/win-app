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

namespace ProtonVPN.StatisticalEvents.Contracts
{
    public enum ModalSources
    {
        Undefined,
        SecureCore,
        NetShield,
        Countries,
        P2P,
        Streaming,
        PortForwarding,
        Profiles,
        VpnAccelerator,
        SplitTunneling,
        CustomDns,
        ModerateNat,
        NonStandardPorts,
        ChangeServer,
        PromoOffer,
        Downgrade,
        MaxConnections
    }
}