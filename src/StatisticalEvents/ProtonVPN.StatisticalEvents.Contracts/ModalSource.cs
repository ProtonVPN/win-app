/*
 * Copyright (c) 2026 Proton AG
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

using System;

namespace ProtonVPN.StatisticalEvents.Contracts;

/// <summary>
/// Defines the feature which leads to upsell modal.
/// 
/// This enum is defined in accordance with the upsell 'modal_source' dimension,
/// which specifies a shared set of allowed values across all ProtonVPN platforms.
/// </summary>
public enum ModalSource
{
    SecureCore,
    NetShield,
    Countries,
    P2P,
    P2PActivity,
    Streaming,
    StreamingActivity,
    PortForwarding,
    Profiles,
    VpnAccelerator,
    SplitTunneling,
    CustomDns,
    AllowLanConnections,
    ModerateNat,
    SafeMode,
    ChangeServer,
    PromoOffer,
    Downgrade,
    MaxConnections,
    [Obsolete("Use ModalSource.Countries with ModalTrigger.Carousel")]
    CarouselCountries,
    [Obsolete("Use ModalSource.AdvancedCustomization with ModalTrigger.Carousel")]
    CarouselCustomization,
    [Obsolete("Use ModalSource.Devices with ModalTrigger.Carousel")]
    CarouselMultipleDevices,
    [Obsolete("Use ModalSource.NetShield with ModalTrigger.Carousel")]
    CarouselNetShield,
    [Obsolete("Use ModalSource.P2P with ModalTrigger.Carousel")]
    CarouselP2P,
    [Obsolete("Use ModalSource.SecureCore with ModalTrigger.Carousel")]
    CarouselSecureCore,
    [Obsolete("Use ModalSource.Speed with ModalTrigger.Carousel")]
    CarouselSpeed,
    [Obsolete("Use ModalSource.SplitTunneling with ModalTrigger.Carousel")]
    CarouselSplitTunneling,
    [Obsolete("Use ModalSource.Streaming with ModalTrigger.Carousel")]
    CarouselStreaming,
    [Obsolete("Use ModalSource.Tor with ModalTrigger.Carousel")]
    CarouselTor,
    Account,
    Tor,
    Tray,
    Onboarding,
    AdvancedCustomization,
    [Obsolete("Use ModalSource.Profiles with ModalTrigger.Carousel")]
    CarouselProfiles,
    Devices,
    Hermes,
    Speed,
    DefaultConnection,
    ExcludeLocations
}