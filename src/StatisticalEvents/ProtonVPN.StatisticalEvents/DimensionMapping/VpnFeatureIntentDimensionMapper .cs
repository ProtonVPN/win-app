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

namespace ProtonVPN.StatisticalEvents.DimensionMapping;

public class VpnFeatureIntentDimensionMapper : DimensionMapperBase, IDimensionMapper<VpnFeatureIntent?>
{
    public string Map(VpnFeatureIntent? featureIntent)
    {
        return featureIntent switch
        {
            VpnFeatureIntent.Standard => "standard",
            VpnFeatureIntent.SecureCore => "secure_core",
            VpnFeatureIntent.P2P => "p2p",
            VpnFeatureIntent.Tor => "tor",
            VpnFeatureIntent.Gateway => "gateway",
            _ => NOT_AVAILABLE
        };
    }
}