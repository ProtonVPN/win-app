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

using ProtonVPN.StatisticalEvents.Contracts;

namespace ProtonVPN.StatisticalEvents;

public static class ModalSourceTranslator
{
    public static string Translate(ModalSource modalSource)
    {
        return modalSource switch
        {
            ModalSource.SecureCore => "secure_core",
            ModalSource.NetShield => "netshield",
            ModalSource.Countries => "countries",
            ModalSource.P2P => "p2p",
            ModalSource.Streaming => "streaming",
            ModalSource.PortForwarding => "port_forwarding",
            ModalSource.Profiles => "profiles",
            ModalSource.VpnAccelerator => "vpn_accelerator",
            ModalSource.SplitTunneling => "split_tunneling",
            ModalSource.CustomDns => "custom_dns",
            ModalSource.AllowLanConnections => "allow_lan",
            ModalSource.ModerateNat => "moderate_nat",
            ModalSource.ChangeServer => "change_server",
            ModalSource.PromoOffer => "promo_offer",
            ModalSource.Downgrade => "downgrade",
            ModalSource.MaxConnections => "max_connections",
            ModalSource.CarouselCountries => "home_carousel_countries",
            ModalSource.CarouselCustomization => "home_carousel_customization",
            ModalSource.CarouselMultipleDevices => "home_carousel_multiple_devices",
            ModalSource.CarouselNetShield => "home_carousel_netshield",
            ModalSource.CarouselP2P => "home_carousel_p2p",
            ModalSource.CarouselSecureCore => "home_carousel_secure_core",
            ModalSource.CarouselSpeed => "home_carousel_speed",
            ModalSource.CarouselSplitTunneling => "home_carousel_split_tunneling",
            ModalSource.CarouselStreaming => "home_carousel_streaming",
            ModalSource.CarouselTor => "home_carousel_tor",
            ModalSource.Account => "account",
            ModalSource.Tor => "tor",
            ModalSource.Tray => "tray",
            _ => "n/a",
        };
    }
}