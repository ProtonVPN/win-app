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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.Common.Helpers;

namespace ProtonVPN.Common.Tests.Helpers
{
    [TestClass]
    public class RoundDownCalculatorTest
    {
        [TestMethod]
        [DataRow(3456, 3400)]
        [DataRow(2000, 2000)]
        [DataRow(567, 560)]
        [DataRow(900, 900)]
        [DataRow(89, 80)]
        [DataRow(10, 10)]
        [DataRow(7, 7)]
        [DataRow(0, 0)]
        [DataRow(-5, -5)]
        [DataRow(-10, -10)]
        [DataRow(-72, -70)]
        [DataRow(-800, -800)]
        [DataRow(-284, -280)]
        [DataRow(-5000, -5000)]
        [DataRow(-8642, -8600)]
        public void RoundDown(int value, int expectedResult)
        {
            int result = RoundDownCalculator.RoundDown(value);

            Assert.AreEqual(expectedResult, result);
        }
    }
}
