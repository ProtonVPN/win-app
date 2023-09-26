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

using ProtonVPN.StatisticalEvents.Contracts;

namespace ProtonVPN.StatisticalEvents
{
    public static class ModalSourceTranslator
    {
        public static string Translate(ModalSources modalSource)
        {
            return modalSource switch
            {
                ModalSources.SecureCore => "secure_core",
                ModalSources.NetShield => "netshield",
                ModalSources.Countries => "countries",
                ModalSources.P2P => "p2p",
                ModalSources.Streaming => "streaming",
                ModalSources.PortForwarding => "port_forwarding",
                ModalSources.Profiles => "profiles",
                ModalSources.VpnAccelerator => "vpn_accelerator",
                ModalSources.SplitTunneling => "split_tunneling",
                ModalSources.CustomDns => "custom_dns",
                ModalSources.ModerateNat => "moderate_nat",
                ModalSources.NonStandardPorts => "safe_mode",
                ModalSources.ChangeServer => "change_server",
                ModalSources.PromoOffer => "promo_offer",
                ModalSources.Downgrade => "downgrade",
                ModalSources.MaxConnections => "max_connections",
                _ => "n/a",
            };
        }
    }
}