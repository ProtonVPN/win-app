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
using ProtonVPN.StatisticalEvents.Contracts;
using ProtonVPN.StatisticalEvents.DimensionBuilders;
using ProtonVPN.StatisticalEvents.MeasurementGroups;
using ProtonVPN.StatisticalEvents.Sending.Contracts;

namespace ProtonVPN.StatisticalEvents;

public class UpsellSuccessStatisticalEventSender : StatisticalEventSenderBase<UpsellMeasurementGroup>,
    IUpsellSuccessStatisticalEventSender
{
    public override string Event => "upsell_success";

    private readonly IUpsellDimensionBuilder _upsellDimensionBuilder;
    private readonly IAuthenticatedStatisticalEventSender _statisticalEventSender;

    public UpsellSuccessStatisticalEventSender(IUpsellDimensionBuilder upsellDimensionBuilder,
        IAuthenticatedStatisticalEventSender statisticalEventSender)
    {
        _upsellDimensionBuilder = upsellDimensionBuilder;
        _statisticalEventSender = statisticalEventSender;
    }

    public void Send(ModalSource modalSource, string oldPlan, string newPlan, string? reference = null)
    {
        StatisticalEvent statisticalEvent = Create(modalSource, oldPlan, newPlan, reference);
        _statisticalEventSender.EnqueueAsync(statisticalEvent);
    }

    private StatisticalEvent Create(ModalSource modalSource, string oldPlan, string newPlan, string? reference)
    {
        StatisticalEvent statisticalEvent = CreateStatisticalEvent();
        statisticalEvent.Dimensions = _upsellDimensionBuilder.Build(modalSource, reference);
        statisticalEvent.Dimensions["user_plan"] = oldPlan;
        statisticalEvent.Dimensions.Add("upgraded_user_plan", newPlan);
        return statisticalEvent;
    }
}