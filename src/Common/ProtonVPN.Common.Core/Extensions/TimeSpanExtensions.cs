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

namespace ProtonVPN.Common.Core.Extensions;

public static class TimeSpanExtensions
{
    private static readonly Random _random = new();

    public static TimeSpan AddJitter(this TimeSpan value, double deviation)
    {
        if (value == TimeSpan.Zero || deviation == 0d)
        {
            return value;
        }

        return AddJitter(value, deviation, _random.NextDouble());
    }

    public static TimeSpan AddJitter(TimeSpan value, double deviation, double randomValue)
    {
        TimeSpan interval = value * deviation;
        TimeSpan randomInterval = interval * randomValue;
        return value + randomInterval;
    }

    public static TimeSpan Min(TimeSpan value1, TimeSpan value2)
    {
        return TimeSpan.FromTicks(Math.Min(value1.Ticks, value2.Ticks));
    }

    public static TimeSpan Max(TimeSpan value1, TimeSpan value2)
    {
        return TimeSpan.FromTicks(Math.Max(value1.Ticks, value2.Ticks));
    }

    public static float GetTotalMilliseconds(this TimeSpan value)
    {
        return (float)Math.Truncate(value.TotalMilliseconds);
    }
}