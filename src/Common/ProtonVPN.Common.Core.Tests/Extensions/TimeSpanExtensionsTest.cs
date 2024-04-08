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

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.Common.Core.Extensions;

namespace ProtonVPN.Common.Core.Tests.Extensions;

[TestClass]
public class TimeSpanExtensionsTest
{
    private const int NUM_OF_TRIES = 1000;

    private readonly IReadOnlyList<double> _deviations = [0.01, 0.1, 0.2, 0.25, 0.333, 0.5];

    private readonly IReadOnlyList<TimeSpan> _testValues = [
        TimeSpan.FromSeconds(5),
        TimeSpan.FromSeconds(30),
        TimeSpan.FromSeconds(60),
        TimeSpan.FromMinutes(5),
        TimeSpan.FromMinutes(15),
        TimeSpan.FromMinutes(30),
        TimeSpan.FromHours(1),
        TimeSpan.FromHours(3),
        TimeSpan.FromHours(6),
        TimeSpan.FromHours(8),
        TimeSpan.FromHours(12),
        TimeSpan.FromHours(24)];

    private readonly IReadOnlyList<Tuple<TimeSpan, double, TimeSpan>> _cornerCases = [
        new Tuple<TimeSpan, double, TimeSpan>(TimeSpan.FromSeconds(200), -0.1, TimeSpan.FromSeconds(180)),
        new Tuple<TimeSpan, double, TimeSpan>(TimeSpan.FromSeconds(1), -100000, TimeSpan.FromSeconds(-99999)),
        new Tuple<TimeSpan, double, TimeSpan>(TimeSpan.FromSeconds(-30), 0.5, TimeSpan.FromSeconds(-45)),
        new Tuple<TimeSpan, double, TimeSpan>(TimeSpan.FromSeconds(-2), 100000, TimeSpan.FromSeconds(-200002)),
        new Tuple<TimeSpan, double, TimeSpan>(TimeSpan.FromSeconds(-60), -0.1, TimeSpan.FromSeconds(-54)),
        new Tuple<TimeSpan, double, TimeSpan>(TimeSpan.FromSeconds(-1), -100000, TimeSpan.FromSeconds(99999))];

    [TestMethod]
    public void TestAddJitterNormalCases()
    {
        foreach (double deviation in _deviations)
        {
            foreach (TimeSpan testValue in _testValues)
            {
                TimeSpan minValue = testValue;
                TimeSpan maxValue = TimeSpan.FromTicks(testValue.Ticks + (long)(testValue.Ticks * deviation));
                TestAddJitterCombination(testValue, deviation, minValue, maxValue);
            }
        }
    }

    private void TestAddJitterCombination(TimeSpan value, double deviation, TimeSpan expectedMinValue, TimeSpan expectedMaxValue)
    {
        for (int i = 0; i < NUM_OF_TRIES; i++)
        {
            TimeSpan result = TimeSpanExtensions.AddJitter(value, deviation);
            Assert.IsTrue(result >= expectedMinValue, $"The jitter result {result} is under the " +
                $"minimum value {expectedMinValue}. Deviation {deviation} and value {value}.");
            Assert.IsTrue(result <= expectedMaxValue, $"The jitter result {result} is above the " +
                $"maximum value {expectedMaxValue}. Deviation {deviation} and value {value}.");
        }
    }

    [TestMethod]
    public void TestAddJitterCornerCases_UsingRandom()
    {
        TestAddJitterCombination(TimeSpan.FromSeconds(200), -0.1, TimeSpan.FromSeconds(180), TimeSpan.FromSeconds(200));
        TestAddJitterCombination(TimeSpan.FromSeconds(1), -100000, TimeSpan.FromSeconds(-99999), TimeSpan.FromSeconds(1));

        TestAddJitterCombination(TimeSpan.FromSeconds(-30), 0.5, TimeSpan.FromSeconds(-45), TimeSpan.FromSeconds(-30));
        TestAddJitterCombination(TimeSpan.FromSeconds(-2), 100000, TimeSpan.FromSeconds(-200002), TimeSpan.FromSeconds(-2));

        TestAddJitterCombination(TimeSpan.FromSeconds(-60), -0.1, TimeSpan.FromSeconds(-60), TimeSpan.FromSeconds(-54));
        TestAddJitterCombination(TimeSpan.FromSeconds(-1), -100000, TimeSpan.FromSeconds(-1), TimeSpan.FromSeconds(99999));
    }

    [TestMethod]
    public void TestAddJitterCornerCases_ForcingRandomAsZero()
    {
        foreach (Tuple<TimeSpan, double, TimeSpan> cornerCase in _cornerCases)
        {
            TimeSpan result = TimeSpanExtensions.AddJitter(cornerCase.Item1, cornerCase.Item2, randomValue: 0d);
            Assert.AreEqual(cornerCase.Item1, result);
        }
    }

    // Note: Random.NextDouble() actually returns a value up to <1.0, it shouldn't return 1.0
    [TestMethod]
    public void TestAddJitterCornerCases_ForcingRandomAsOne()
    {
        foreach (Tuple<TimeSpan, double, TimeSpan> cornerCase in _cornerCases)
        {
            TimeSpan result = TimeSpanExtensions.AddJitter(cornerCase.Item1, cornerCase.Item2, randomValue: 1d);
            Assert.AreEqual(cornerCase.Item3, result);
        }
    }

    [TestMethod]
    [DataRow(5, 8, 5)]
    [DataRow(5, 5, 5)]
    [DataRow(0, -5, -5)]
    [DataRow(2, -5, -5)]
    [DataRow(-2, 5, -2)]
    [DataRow(-2, -5, -5)]
    public void TestMinTimeSpan(double intervalAInSeconds, double intervalBInSeconds, double expectedIntervalInSeconds)
    {
        TimeSpan intervalA = TimeSpan.FromSeconds(intervalAInSeconds);
        TimeSpan intervalB = TimeSpan.FromSeconds(intervalBInSeconds);

        TimeSpan result = TimeSpanExtensions.Min(intervalA, intervalB);
        Assert.AreEqual(expectedIntervalInSeconds, result.TotalSeconds);
    }
}