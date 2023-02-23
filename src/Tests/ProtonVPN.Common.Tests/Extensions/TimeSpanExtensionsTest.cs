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
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.Common.Extensions;

namespace ProtonVPN.Common.Tests.Extensions
{
    [TestClass]
    public class TimeSpanExtensionsTest
    {
        [TestMethod]
        public void RandomizedWithDeviation_ShouldBe_Value_WhenDeviation_IsZero()
        {
            // Arrange
            TimeSpan expected = TimeSpan.FromSeconds(20);

            // Act
            TimeSpan result = expected.RandomizedWithDeviation(0.0);

            // Assert
            result.Should().BeCloseTo(expected, TimeSpan.Zero);
        }

        [TestMethod]
        public void RandomizedWithDeviation_ShouldBe_WithinDeviation()
        {
            // Arrange
            TimeSpan interval = TimeSpan.FromSeconds(20);
            const double deviation = 0.2;
            TimeSpan minValue = interval;
            TimeSpan maxValue = interval;
            TimeSpan sumValue = TimeSpan.Zero;

            // Act
            for (int i = 0; i < 1000; i++)
            {
                TimeSpan result = interval.RandomizedWithDeviation(deviation);
                if (result < minValue) minValue = result;
                if (result > maxValue) maxValue = result;
                sumValue += result;
            }

            TimeSpan medianValue = TimeSpan.FromMilliseconds(sumValue.TotalMilliseconds / 1000.0);

            // Assert
            minValue.Should().BeCloseTo(TimeSpan.FromSeconds(16), TimeSpan.FromMilliseconds(100));
            medianValue.Should().BeCloseTo(interval, TimeSpan.FromMilliseconds(300));
            maxValue.Should().BeCloseTo(TimeSpan.FromSeconds(24), TimeSpan.FromMilliseconds(100));
        }
    }
}