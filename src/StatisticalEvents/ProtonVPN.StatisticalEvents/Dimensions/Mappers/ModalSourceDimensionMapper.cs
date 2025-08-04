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
using ProtonVPN.StatisticalEvents.Dimensions.Mappers.Bases;

namespace ProtonVPN.StatisticalEvents.Dimensions.Mappers;

public class ModalSourceDimensionMapper : DimensionMapperBase, IModalSourceDimensionMapper
{
    private const string SECURE_CORE = "secure_core";
    private const string NETSHIELD = "netshield";
    private const string COUNTRIES = "countries";
    private const string P2P = "p2p";
    private const string STREAMING = "streaming";
    private const string PORT_FORWARDING = "port_forwarding";
    private const string PROFILES = "profiles";
    private const string VPN_ACCELERATOR = "vpn_accelerator";
    private const string SPLIT_TUNNELING = "split_tunneling";
    private const string CUSTOM_DNS = "custom_dns";
    private const string ALLOW_LAN = "allow_lan";
    private const string MODERATE_NAT = "moderate_nat";
    private const string SAFE_MODE = "safe_mode"; // Not used on Windows
    private const string CHANGE_SERVER = "change_server";
    private const string PROMO_OFFER = "promo_offer";
    private const string DOWNGRADE = "downgrade";
    private const string MAX_CONNECTIONS = "max_connections";
    private const string HOME_CAROUSEL_COUNTRIES = "home_carousel_countries";
    private const string HOME_CAROUSEL_CUSTOMIZATION = "home_carousel_customization";
    private const string HOME_CAROUSEL_MULTIPLE_DEVICES = "home_carousel_multiple_devices";
    private const string HOME_CAROUSEL_NETSHIELD = "home_carousel_netshield";
    private const string HOME_CAROUSEL_P2P = "home_carousel_p2p";
    private const string HOME_CAROUSEL_SECURE_CORE = "home_carousel_secure_core";
    private const string HOME_CAROUSEL_SPEED = "home_carousel_speed";
    private const string HOME_CAROUSEL_SPLIT_TUNNELING = "home_carousel_split_tunneling";
    private const string HOME_CAROUSEL_STREAMING = "home_carousel_streaming";
    private const string HOME_CAROUSEL_TOR = "home_carousel_tor";
    private const string ACCOUNT = "account";
    private const string TOR = "tor";
    private const string TRAY = "tray";

    public string Map(ModalSource? modalSource)
    {
        return modalSource switch
        {
            ModalSource.SecureCore => SECURE_CORE,
            ModalSource.NetShield => NETSHIELD,
            ModalSource.Countries => COUNTRIES,
            ModalSource.P2P => P2P,
            ModalSource.Streaming => STREAMING,
            ModalSource.PortForwarding => PORT_FORWARDING,
            ModalSource.Profiles => PROFILES,
            ModalSource.VpnAccelerator => VPN_ACCELERATOR,
            ModalSource.SplitTunneling => SPLIT_TUNNELING,
            ModalSource.CustomDns => CUSTOM_DNS,
            ModalSource.AllowLanConnections => ALLOW_LAN,
            ModalSource.ModerateNat => MODERATE_NAT,
            ModalSource.ChangeServer => CHANGE_SERVER,
            ModalSource.PromoOffer => PROMO_OFFER,
            ModalSource.Downgrade => DOWNGRADE,
            ModalSource.MaxConnections => MAX_CONNECTIONS,
            ModalSource.CarouselCountries => HOME_CAROUSEL_COUNTRIES,
            ModalSource.CarouselCustomization => HOME_CAROUSEL_CUSTOMIZATION,
            ModalSource.CarouselMultipleDevices => HOME_CAROUSEL_MULTIPLE_DEVICES,
            ModalSource.CarouselNetShield => HOME_CAROUSEL_NETSHIELD,
            ModalSource.CarouselP2P => HOME_CAROUSEL_P2P,
            ModalSource.CarouselSecureCore => HOME_CAROUSEL_SECURE_CORE,
            ModalSource.CarouselSpeed => HOME_CAROUSEL_SPEED,
            ModalSource.CarouselSplitTunneling => HOME_CAROUSEL_SPLIT_TUNNELING,
            ModalSource.CarouselStreaming => HOME_CAROUSEL_STREAMING,
            ModalSource.CarouselTor => HOME_CAROUSEL_TOR,
            ModalSource.Account => ACCOUNT,
            ModalSource.Tor => TOR,
            ModalSource.Tray => TRAY,
            _ => NOT_AVAILABLE
        };
    }
}
