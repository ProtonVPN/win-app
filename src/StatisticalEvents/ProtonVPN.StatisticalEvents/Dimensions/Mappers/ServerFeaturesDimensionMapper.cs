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
using ProtonVPN.StatisticalEvents.Dimensions.Mappers.Bases;

namespace ProtonVPN.StatisticalEvents.Dimensions.Mappers;

public class ServerFeaturesDimensionMapper : DimensionMapperBase, IServerFeaturesDimensionMapper
{
    private const string FREE = "free";
    private const string TOR = "tor";
    private const string P2P = "p2p";
    private const string SECURE_CORE = "secureCore";
    private const string PARTNERSHIP = "partnership";
    private const string STREAMING = "streaming";
    private const string IPV6 = "ipv6";

    public string Map(ServerDetailsEventData serverDetails)
    {
        List<string> features = [];

        if (serverDetails.IsFree)
        {
            features.Add(FREE);
        }

        if (serverDetails.SupportsTor)
        {
            features.Add(TOR);
        }

        if (serverDetails.SupportsP2P)
        {
            features.Add(P2P);
        }

        if (serverDetails.SecureCore)
        {
            features.Add(SECURE_CORE);
        }

        if (serverDetails.IsB2B)
        {
            features.Add(PARTNERSHIP);
        }

        if (serverDetails.SupportsStreaming)
        {
            features.Add(STREAMING);
        }

        if (serverDetails.SupportsIpv6)
        {
            features.Add(IPV6);
        }

        features.Sort();

        return string.Join(",", features);
    }
}