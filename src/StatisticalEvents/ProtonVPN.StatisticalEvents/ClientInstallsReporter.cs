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

using ProtonVPN.StatisticalEvents.Contracts;
using ProtonVPN.StatisticalEvents.Dimensions.Builders;
using ProtonVPN.StatisticalEvents.Events.Senders.Contracts;
using ProtonVPN.StatisticalEvents.MeasurementGroups;

namespace ProtonVPN.StatisticalEvents;

public class ClientInstallsReporter : ReporterBase<ClientInstallsMeasurementGroup>,
    IClientInstallsReporter
{
    private readonly IClientInstallsDimensionsBuilder _dimensionsBuilder;

    public override string Event => "client_launch";

    public ClientInstallsReporter(
        IClientInstallsDimensionsBuilder dimensionsBuilder,
        IUnauthenticatedStatisticalEventSender statisticalEventSender)
        : base(statisticalEventSender)
    {
        _dimensionsBuilder = dimensionsBuilder;
    }

    public void Report(bool isMailInstalled, bool isDriveInstalled, bool isPassInstalled)
    {
        ReportEvent(
            CreateStatisticalEventBuilder()
                .WithDimensions(_dimensionsBuilder.Build(isMailInstalled, isDriveInstalled, isPassInstalled))
                .Build());
    }
}