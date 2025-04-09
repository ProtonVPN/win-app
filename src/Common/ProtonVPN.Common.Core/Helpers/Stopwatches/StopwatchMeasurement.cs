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

namespace ProtonVPN.Common.Core.Helpers.Stopwatches;

public class StopwatchMeasurement
{
    public string? Key { get; }
    public TimeSpan? ElapsedTime { get; }
    public DateTimeOffset StartDate { get; }
    public DateTimeOffset EndDate { get; }

    public StopwatchMeasurement(string? key, DateTimeOffset date)
    {
        Key = key;
        ElapsedTime = null;
        StartDate = date;
        EndDate = date;
    }

    public StopwatchMeasurement(string? key, TimeSpan elapsedTime, DateTimeOffset startDate, DateTimeOffset endDate)
    {
        Key = key;
        ElapsedTime = elapsedTime;
        StartDate = startDate;
        EndDate = endDate;
    }

    public override string ToString()
    {
        return ElapsedTime is null
            ? $"- {Key}: {StartDate.TimeOfDay}"
            : $"- {Key}: {ElapsedTime} [{StartDate.TimeOfDay} - {EndDate.TimeOfDay}]";
    }

    public string ToString(DateTimeOffset? previousDate)
    {
        return previousDate is null
            ? ToString()
            : $"- {StartDate - previousDate} {ToString()}";
    }
}
