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
using ProtonVPN.StatisticalEvents.Contracts.Models;
using ProtonVPN.StatisticalEvents.Dimensions.Builders;
using ProtonVPN.StatisticalEvents.Sending.Contracts;

namespace ProtonVPN.StatisticalEvents;

public class VpnConnectionStatisticalEventSender : VpnStatisticalEventSenderBase, IVpnConnectionStatisticalEventSender
{
    public override string Event => "vpn_connection";

    public VpnConnectionStatisticalEventSender(
        IVpnConnectionDimensionsBuilder dimensionsProvider,
        IAuthenticatedStatisticalEventSender statisticalEventSender)
        : base(dimensionsProvider, statisticalEventSender)
    { }

    public void Send(VpnConnectionEventData eventData, float timeToConnectionInMilliseconds)
    {
        SendStatisticalEvent(eventData, "time_to_connection", timeToConnectionInMilliseconds);
    }
}