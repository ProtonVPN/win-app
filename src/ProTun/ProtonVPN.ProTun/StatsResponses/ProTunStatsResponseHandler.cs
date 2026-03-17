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

using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Common.Legacy;
using ProtonVPN.ProTun.Generated;

namespace ProtonVPN.ProTun.StatsResponses;

public class ProTunStatsResponseHandler : IProTunStatsResponseHandler
{
    public event EventHandler<EventArgs<NetworkTraffic>>? TrafficUpdated;

    public void OnStatsResponse(ConnectionStats stats)
    {
        NetworkTraffic traffic = new(stats.receivedBytes, stats.sentBytes);
        InvokeTrafficUpdate(traffic);
    }

    private void InvokeTrafficUpdate(NetworkTraffic traffic)
    {
        TrafficUpdated?.Invoke(this, new EventArgs<NetworkTraffic>(traffic));
    }
}