﻿/*
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

using ProtonVPN.Common.Core.StatisticalEvents;
using ProtonVPN.StatisticalEvents.Contracts.Models;
using ProtonVPN.StatisticalEvents.MeasurementGroups;
using ProtonVPN.StatisticalEvents.Sending.Contracts;

namespace ProtonVPN.StatisticalEvents;

public abstract class VpnStatisticalEventSenderBase : StatisticalEventSenderBase<VpnConnectionsMeasurementGroup>
{
    private readonly IVpnConnectionDimensionsProvider _dimensionsProvider;
    private readonly IAuthenticatedStatisticalEventSender _authenticatedStatisticalEventSender;

    protected VpnStatisticalEventSenderBase(
        IVpnConnectionDimensionsProvider dimensionsProvider,
        IAuthenticatedStatisticalEventSender statisticalEventSender)
    {
        _dimensionsProvider = dimensionsProvider;
        _authenticatedStatisticalEventSender = statisticalEventSender;
    }

    protected void SendStatisticalEvent(
        VpnConnectionEventData eventData,
        string valueKey,
        float value)
    {
        StatisticalEvent statisticalEvent = CreateStatisticalEvent();
        statisticalEvent.Values.Add(valueKey, value);
        statisticalEvent.Dimensions = _dimensionsProvider.GetDimensions(eventData);

        _authenticatedStatisticalEventSender.EnqueueAsync(statisticalEvent);
    }
}