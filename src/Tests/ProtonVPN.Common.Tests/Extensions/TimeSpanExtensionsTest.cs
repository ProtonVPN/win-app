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
            int numOfGenerations = 100000;
            TimeSpan interval = TimeSpan.FromSeconds(20);
            const double deviation = 0.2;
            TimeSpan? minValue = null;
            TimeSpan? maxValue = null;
            TimeSpan sumValue = TimeSpan.Zero;

            // Act
            for (int i = 0; i < numOfGenerations; i++)
            {
                TimeSpan result = interval.RandomizedWithDeviation(deviation);
                if (minValue is null || result < minValue)
                {
                    minValue = result;
                }
                if (maxValue is null || result > maxValue)
                {
                    maxValue = result;
                }
                sumValue += result;
            }

            TimeSpan medianValue = TimeSpan.FromMilliseconds(sumValue.TotalMilliseconds / numOfGenerations);

            // Assert
            minValue.Should().BeCloseTo(TimeSpan.FromSeconds(20), TimeSpan.FromMilliseconds(100));
            medianValue.Should().BeCloseTo(TimeSpan.FromSeconds(22), TimeSpan.FromMilliseconds(100));
            maxValue.Should().BeCloseTo(TimeSpan.FromSeconds(24), TimeSpan.FromMilliseconds(100));
        }
    }
}