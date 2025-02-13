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

using System.Collections.Generic;
using ProtonVPN.StatisticalEvents.Contracts.Models;

namespace ProtonVPN.StatisticalEvents.DimensionMapping;

public class ServerFeaturesDimensionMapper : DimensionMapperBase, IDimensionMapper<ServerDetails>
{
    public string Map(ServerDetails serverDetails)
    {
        List<string> features = [];

        if (serverDetails.IsFree)
        {
            features.Add("free");
        }

        if (serverDetails.SupportsTor)
        {
            features.Add("tor");
        }

        if (serverDetails.SupportsP2P)
        {
            features.Add("p2p");
        }

        if (serverDetails.SecureCore)
        {
            features.Add("secureCore");
        }

        if (serverDetails.IsB2B)
        {
            features.Add("partnership");
        }

        if (serverDetails.SupportsStreaming)
        {
            features.Add("streaming");
        }

        if (serverDetails.SupportsIpv6)
        {
            features.Add("ipv6");
        }

        features.Sort();

        return string.Join(",", features);
    }
}