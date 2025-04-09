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

using System.Collections.Concurrent;
using System.Diagnostics;

namespace ProtonVPN.Common.Core.Helpers.Stopwatches;

public static class StopwatchWrapper
{
    public static ConcurrentQueue<StopwatchMeasurement> Cache { get; private set; } = new();

    public static void Measure(string key)
    {
        StopwatchMeasurement stopwatchMeasurement = new(key, DateTimeOffset.Now);
        Cache.Enqueue(stopwatchMeasurement);
    }

    public static T Measure<T>(string key, Func<T> func)
    {
        DateTimeOffset startDate = DateTimeOffset.Now;
        Stopwatch stopwatch = Stopwatch.StartNew();
        T result = func();
        stopwatch.Stop();
        DateTimeOffset endDate = DateTimeOffset.Now;

        StopwatchMeasurement stopwatchMeasurement = new(key, stopwatch.Elapsed, startDate: startDate, endDate: endDate);
        Cache.Enqueue(stopwatchMeasurement);

        return result;
    }

    public static void Measure(string key, Action action)
    {
        Measure(key, () => { action(); return true; });
    }

    public static string ToString()
    {
        List<StopwatchMeasurement> measurements = Cache.ToList();
        DateTimeOffset? lastDate = null;
        List<string> measurementStrings = [];
        foreach (StopwatchMeasurement measurement in measurements)
        {
            measurementStrings.Add(measurement.ToString(lastDate));
            lastDate = measurement.EndDate;
        }
        return $"Cached measurements:{Environment.NewLine}{string.Join(Environment.NewLine, measurementStrings)}";
    }
}