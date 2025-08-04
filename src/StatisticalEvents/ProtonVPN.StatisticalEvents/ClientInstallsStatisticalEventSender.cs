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

using ProtonVPN.Common.Core.StatisticalEvents;
using ProtonVPN.StatisticalEvents.Contracts;
using ProtonVPN.StatisticalEvents.Dimensions.Builders;
using ProtonVPN.StatisticalEvents.MeasurementGroups;
using ProtonVPN.StatisticalEvents.Sending.Contracts;

namespace ProtonVPN.StatisticalEvents;

public class ClientInstallsStatisticalEventSender : StatisticalEventSenderBase<ClientInstallsMeasurementGroup>,
    IClientInstallsStatisticalEventSender
{
    private readonly IClientInstallsDimensionsBuilder _dimensionsBuilder;
    private readonly IUnauthenticatedStatisticalEventSender _statisticalEventSender;
    public override string Event => "client_launch";

    public ClientInstallsStatisticalEventSender(
        IClientInstallsDimensionsBuilder dimensionsBuilder,
        IUnauthenticatedStatisticalEventSender statisticalEventSender)
    {
        _dimensionsBuilder = dimensionsBuilder;
        _statisticalEventSender = statisticalEventSender;
    }

    public void Send(bool isMailInstalled, bool isDriveInstalled, bool isPassInstalled)
    {
        StatisticalEvent statisticalEvent = Create(isMailInstalled, isDriveInstalled, isPassInstalled);
        _statisticalEventSender.EnqueueAsync(statisticalEvent);
    }

    private StatisticalEvent Create(bool isMailInstalled, bool isDriveInstalled, bool isPassInstalled)
    {
        StatisticalEvent statisticalEvent = CreateStatisticalEvent();
        statisticalEvent.Dimensions = _dimensionsBuilder.Build(isMailInstalled, isDriveInstalled, isPassInstalled);
        return statisticalEvent;
    }
}