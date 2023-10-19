﻿/*
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

using Autofac;
using ProtonVPN.StatisticalEvents;
using ProtonVPN.StatisticalEvents.DimensionBuilders;
using ProtonVPN.StatisticalEvents.Sending;

namespace ProtonVPN.Api.Installers
{
    public class StatisticalEventsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<UpsellDimensionBuilder>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<StatisticalEventSender>().AsImplementedInterfaces().SingleInstance();

            RegisterSpecificSenders(builder);
        }

        private void RegisterSpecificSenders(ContainerBuilder builder)
        {
            builder.RegisterType<UpsellDisplayStatisticalEventSender>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<UpsellSuccessStatisticalEventSender>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<UpsellUpgradeAttemptStatisticalEventSender>().AsImplementedInterfaces().SingleInstance();
        }
    }
}