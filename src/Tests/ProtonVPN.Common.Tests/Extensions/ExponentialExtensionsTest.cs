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

using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.Common.Legacy.Extensions;
using static ProtonVPN.Common.Legacy.Extensions.ExponentialExtensions;

namespace ProtonVPN.Common.Tests.Extensions
{
    [TestClass]
    public class ExponentialExtensionsTest
    {
        [DataTestMethod]
        [DataRow(0, "B", UnitSpacing.WithSpace, "0 B")]
        [DataRow(1, "B", UnitSpacing.WithSpace, "1 B")]
        [DataRow(21, "B", UnitSpacing.WithSpace, "21 B")]
        [DataRow(321, "B", UnitSpacing.WithSpace, "321 B")]
        [DataRow(4321, "B", UnitSpacing.WithSpace, "4.3 KB")]
        [DataRow(54321, "B", UnitSpacing.WithSpace, "54 KB")]
        [DataRow(654321, "B", UnitSpacing.WithSpace, "654 KB")]
        [DataRow(7654321, "B", UnitSpacing.WithSpace, "7.7 MB")]
        [DataRow(87654321, "B", UnitSpacing.WithSpace, "88 MB")]
        [DataRow(987654321, "B", UnitSpacing.WithSpace, "988 MB")]
        [DataRow(9987654321, "B", UnitSpacing.WithSpace, "10 GB")]
        [DataRow(89987654321, "B", UnitSpacing.WithSpace, "90 GB")]
        [DataRow(789987654321, "B", UnitSpacing.WithSpace, "790 GB")]
        [DataRow(6789987654321, "B", UnitSpacing.WithSpace, "6.8 TB")]
        [DataRow(56789987654321, "B", UnitSpacing.WithSpace, "57 TB")]
        [DataRow(456789987654321, "B", UnitSpacing.WithSpace, "457 TB")]
        // Test rounding to next metric prefix 999.500 B => 1000 KB => 1MB
        [DataRow(999500, "B", UnitSpacing.WithSpace, "1 MB")]
        [DataRow(-999500, "B", UnitSpacing.WithSpace, "-1 MB")]
        // Test negative and different unit
        [DataRow(-1, "abc", UnitSpacing.WithSpace, "-1 abc")]
        [DataRow(-9123456789, "abc", UnitSpacing.WithSpace, "-9.1 Gabc")]
        [DataRow(-89123456789, "abc", UnitSpacing.WithSpace, "-89 Gabc")]
        [DataRow(-789123456789, "abc", UnitSpacing.WithSpace, "-789 Gabc")]
        // Test no spacing and different unit
        [DataRow(2, "XYZ", UnitSpacing.WithoutSpace, "2XYZ")]
        [DataRow(1234987654321, "XYZ", UnitSpacing.WithoutSpace, "1.2TXYZ")]
        [DataRow(12345987654321, "XYZ", UnitSpacing.WithoutSpace, "12TXYZ")]
        [DataRow(123456987654321, "XYZ", UnitSpacing.WithoutSpace, "123TXYZ")]
        public void ToShortString(long number, string unit, UnitSpacing unitSpacing, string expectedResult)
        {
            expectedResult = expectedResult.Replace(".", Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator);

            string result = number.ToShortString(unit, unitSpacing);

            Assert.AreEqual(expectedResult, result);
        }
    }
}