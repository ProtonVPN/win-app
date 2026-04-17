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

public class ProductPromptDisplayReporter : ReporterBase<ProductPromptsMeasurementGroup>, IProductPromptDisplayReporter
{
    private readonly IProductPromptDimensionsBuilder _dimensionsBuilder;

    public override string Event => "product_prompt_display";

    public ProductPromptDisplayReporter(
        IProductPromptDimensionsBuilder dimensionsBuilder,
        IAuthenticatedStatisticalEventSender statisticalEventSender)
        : base(statisticalEventSender)
    {
        _dimensionsBuilder = dimensionsBuilder;
    }

    public void Report(PromptType promptType, PromptContext promptContext)
    {
        ReportEvent(
            CreateStatisticalEventBuilder()
                .WithDimensions(_dimensionsBuilder.Build(promptType, promptContext))
                .Build());
    }
}