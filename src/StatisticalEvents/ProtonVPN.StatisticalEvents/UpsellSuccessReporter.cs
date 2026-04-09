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

using ProtonVPN.Client.Logic.Users.Contracts.Messages;
using ProtonVPN.StatisticalEvents.Contracts;
using ProtonVPN.StatisticalEvents.Dimensions.Builders;
using ProtonVPN.StatisticalEvents.Events.Senders.Contracts;
using ProtonVPN.StatisticalEvents.MeasurementGroups;

namespace ProtonVPN.StatisticalEvents;

public class UpsellSuccessReporter : ReporterBase<UpsellMeasurementGroup>,
    IUpsellSuccessReporter
{
    private readonly IUpsellDimensionsBuilder _dimensionsBuilder;

    public override string Event => "upsell_success";

    public UpsellSuccessReporter(
        IUpsellDimensionsBuilder dimensionsBuilder,
        IAuthenticatedStatisticalEventSender statisticalEventSender)
        : base(statisticalEventSender)
    {
        _dimensionsBuilder = dimensionsBuilder;
    }

    public void Report(string url, ModalSource modalSource, VpnPlan oldPlan, VpnPlan newPlan, string? reference = null)
    {
        ReportEvent(
            CreateStatisticalEventBuilder()
                .WithDimensions(_dimensionsBuilder.Build(modalSource, reference))
                .WithDimensions(_dimensionsBuilder.BuildSuccessDimensions(url, oldPlan, newPlan))
                .Build());
    }
}