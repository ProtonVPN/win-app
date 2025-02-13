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
using ProtonVPN.Common.Core.StatisticalEvents;
using ProtonVPN.Common.Legacy.Extensions;
using ProtonVPN.StatisticalEvents.Contracts;
using ProtonVPN.StatisticalEvents.MeasurementGroups;
using ProtonVPN.StatisticalEvents.Sending.Contracts;

namespace ProtonVPN.StatisticalEvents;

public class ClientInstallsStatisticalEventSender : StatisticalEventSenderBase<ClientInstallsMeasurementGroup>,
    IClientInstallsStatisticalEventSender
{
    public override string Event => "client_launch";

    private readonly IUnauthStatisticalEventSender _statisticalEventSender;

    public ClientInstallsStatisticalEventSender(IUnauthStatisticalEventSender statisticalEventSender)
    {
        _statisticalEventSender = statisticalEventSender;
    }

    public void Send(bool isMailInstalled, bool isDriveInstalled, bool isPassInstalled)
    {
        StatisticalEvent statisticalEvent = CreateStatisticalEvent();
        statisticalEvent.Dimensions = new Dictionary<string, string>
        {
            { "client", "windows" },
            { "product", "vpn" },
            { "is_mail_installed", isMailInstalled.ToBooleanString() },
            { "is_drive_installed", isDriveInstalled.ToBooleanString() },
            { "is_pass_installed", isPassInstalled.ToBooleanString() },
        };

        _statisticalEventSender.EnqueueAsync(statisticalEvent);
    }
}