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

using ProtonVPN.StatisticalEvents.Contracts.Models;
using ProtonVPN.StatisticalEvents.Dimensions.Mappers.Bases;

namespace ProtonVPN.StatisticalEvents.Dimensions.Mappers;

public class VpnFeatureIntentDimensionMapper : DimensionMapperBase, IVpnFeatureIntentDimensionMapper
{
    private const string STANDARD = "standard";
    private const string SECURE_CORE = "secure_core";
    private const string P2P = "p2p";
    private const string TOR = "tor";
    private const string GATEWAY = "gateway";

    public string Map(VpnFeatureIntent? featureIntent)
    {
        return featureIntent switch
        {
            VpnFeatureIntent.Standard => STANDARD,
            VpnFeatureIntent.SecureCore => SECURE_CORE,
            VpnFeatureIntent.P2P => P2P,
            VpnFeatureIntent.Tor => TOR,
            VpnFeatureIntent.Gateway => GATEWAY,
            _ => NOT_AVAILABLE
        };
    }
}