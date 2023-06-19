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

using ProtonVPN.Client.Common.Enums;
using ProtonVPN.Client.Common.Helpers;

namespace ProtonVPN.Client.Common.Tests.Helpers;

[TestClass]
public class ByteConversionHelperTests
{
    [DataTestMethod]
    // Test positive values
    [DataRow(0, 0, ByteMetrics.Bytes)]
    [DataRow(1, 1, ByteMetrics.Bytes)]
    [DataRow(21, 21, ByteMetrics.Bytes)]
    [DataRow(321, 321, ByteMetrics.Bytes)]
    [DataRow(4321, 4.3, ByteMetrics.Kilobytes)]
    [DataRow(54321, 54, ByteMetrics.Kilobytes)]
    [DataRow(654321, 654, ByteMetrics.Kilobytes)]
    [DataRow(7654321, 7.7, ByteMetrics.Megabytes)]
    [DataRow(87654321, 88, ByteMetrics.Megabytes)]
    [DataRow(987654321, 988, ByteMetrics.Megabytes)]
    [DataRow(9987654321, 10, ByteMetrics.Gigabytes)]
    [DataRow(89987654321, 90, ByteMetrics.Gigabytes)]
    [DataRow(789987654321, 790, ByteMetrics.Gigabytes)]
    [DataRow(6789987654321, 6.8, ByteMetrics.Terabytes)]
    [DataRow(56789987654321, 57, ByteMetrics.Terabytes)]
    [DataRow(456789987654321, 457, ByteMetrics.Terabytes)]
    [DataRow(3456789987654321, 3.5, ByteMetrics.Petabytes)]
    [DataRow(23456789987654321, 23, ByteMetrics.Petabytes)]
    [DataRow(123456789987654321, 123, ByteMetrics.Petabytes)]
    [DataRow(9123456789987654321, 9.1, ByteMetrics.Exabytes)]
    // Test negative values
    [DataRow(-1, -1, ByteMetrics.Bytes)]
    [DataRow(-4321, -4.3, ByteMetrics.Kilobytes)]
    [DataRow(-7654321, -7.7, ByteMetrics.Megabytes)]
    [DataRow(-9987654321, -10, ByteMetrics.Gigabytes)]
    [DataRow(-6789987654321, -6.8, ByteMetrics.Terabytes)]
    [DataRow(-9123456789987654321, -9.1, ByteMetrics.Exabytes)]
    // Test rounding to next metric prefix 999.500 B => 1000 KB => 1MB
    [DataRow(999500, 1, ByteMetrics.Megabytes)]
    [DataRow(-999500, -1, ByteMetrics.Megabytes)]
    public void ByteConversionTest(long sizeInBytes, double expectedSize, ByteMetrics expectedMetric)
    {
        (double size, ByteMetrics metric) result = ByteConversionHelper.CalculateSize(sizeInBytes);

        Assert.AreEqual(expectedSize, result.size);
        Assert.AreEqual(expectedMetric, result.metric);
    }
}