/*
 * Copyright (c) 2023 Proton AG
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

using ProtonVPN.Common.StatisticalEvents;
using ProtonVPN.StatisticalEvents.Contracts;
using ProtonVPN.StatisticalEvents.DimensionBuilders;
using ProtonVPN.StatisticalEvents.MeasurementGroups;
using ProtonVPN.StatisticalEvents.Sending;

namespace ProtonVPN.StatisticalEvents
{
    public class UpsellUpgradeAttemptStatisticalEventSender : StatisticalEventSenderBase<UpsellMeasurementGroup>,
        IUpsellUpgradeAttemptStatisticalEventSender
    {
        public override string Event => "upsell_upgrade_attempt";

        private readonly IUpsellDimensionBuilder _upsellDimensionBuilder;
        private readonly IStatisticalEventSender _statisticalEventSender;

        public UpsellUpgradeAttemptStatisticalEventSender(IUpsellDimensionBuilder upsellDimensionBuilder,
            IStatisticalEventSender statisticalEventSender)
        {
            _upsellDimensionBuilder = upsellDimensionBuilder;
            _statisticalEventSender = statisticalEventSender;
        }

        public void Send(ModalSources modalSource)
        {
            StatisticalEvent statisticalEvent = Create(modalSource);
            _statisticalEventSender.EnqueueAsync(statisticalEvent);
        }

        private StatisticalEvent Create(ModalSources modalSource)
        {
            StatisticalEvent statisticalEvent = CreateStatisticalEvent();
            statisticalEvent.Dimensions = _upsellDimensionBuilder.Build(modalSource);
            return statisticalEvent;
        }
    }
}