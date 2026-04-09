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

using System.Threading.Tasks;
using ProtonVPN.Common.Core.Extensions;
using ProtonVPN.Common.Core.StatisticalEvents;
using ProtonVPN.StatisticalEvents.Events.Builders;
using ProtonVPN.StatisticalEvents.Events.Senders.Contracts;
using ProtonVPN.StatisticalEvents.MeasurementGroups;

namespace ProtonVPN.StatisticalEvents;

public abstract class ReporterBase<TMeasurementGroup>
    where TMeasurementGroup : class, IMeasurementGroup, new()
{
    public abstract string Event { get; }

    private readonly string _measurementGroup;

    private readonly IStatisticalEventSender _statisticalEventSender;

    protected ReporterBase(IStatisticalEventSender statisticalEventSender)
    {
        _statisticalEventSender = statisticalEventSender;

        _measurementGroup = new TMeasurementGroup().Name;
    }

    private StatisticalEvent CreateStatisticalEvent()
    {
        return new StatisticalEvent()
        {
            MeasurementGroup = _measurementGroup,
            Event = Event
        };
    }

    protected StatisticalEventBuilder CreateStatisticalEventBuilder()
    {
        return new StatisticalEventBuilder(CreateStatisticalEvent());
    }

    protected Task ReportEventAsync(StatisticalEvent statisticalEvent)
    {
        return _statisticalEventSender.EnqueueAsync(statisticalEvent);
    }

    protected void ReportEvent(StatisticalEvent statisticalEvent)
    {
        ReportEventAsync(statisticalEvent).FireAndForget();
    }
}