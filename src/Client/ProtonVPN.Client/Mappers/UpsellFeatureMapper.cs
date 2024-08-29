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
using ProtonVPN.Client.UI.Upsell.Carousel.Models;

namespace ProtonVPN.Client.Mappers;

public static class UpsellFeatureMapper
{
    public static UpsellFeature Map(this ModalSources source)
    {
        return source switch
        {
            ModalSources.SecureCore => UpsellFeature.SecureCore,
            ModalSources.NetShield => UpsellFeature.NetShield,
            ModalSources.Streaming => UpsellFeature.Streaming,
            ModalSources.VpnAccelerator => UpsellFeature.Speed,
            ModalSources.SplitTunneling => UpsellFeature.SplitTunneling,
            ModalSources.Countries or
            ModalSources.ChangeServer => UpsellFeature.WorldwideCoverage,
            ModalSources.P2P or
            ModalSources.PortForwarding => UpsellFeature.P2P,
            ModalSources.CustomDns or
            ModalSources.ModerateNat or
            ModalSources.Profiles or
            ModalSources.MaxConnections => UpsellFeature.MultipleDevices,
            ModalSources.Undefined or
            ModalSources.PromoOffer or
            ModalSources.Downgrade or
            _ => UpsellFeature.WorldwideCoverage,
        };
    }

    public static ModalSources Map(this UpsellFeature? feature)
    {
        return feature switch
        {
            UpsellFeature.WorldwideCoverage => ModalSources.Countries,
            UpsellFeature.Speed => ModalSources.VpnAccelerator,
            UpsellFeature.Streaming => ModalSources.Streaming,
            UpsellFeature.NetShield => ModalSources.NetShield,
            UpsellFeature.SecureCore => ModalSources.SecureCore,
            UpsellFeature.P2P => ModalSources.P2P,
            UpsellFeature.MultipleDevices => ModalSources.MaxConnections,
            UpsellFeature.Tor => ModalSources.Countries,
            UpsellFeature.SplitTunneling => ModalSources.SplitTunneling,
            _ => ModalSources.Undefined
        };
    }
}