/*
 * Copyright (c) 2020 Proton Technologies AG
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

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.Core.Settings;

namespace ProtonVPN.Core.Test.Settings
{
    [TestClass]
    public class TruncatedLocationTest
    {
        [TestMethod]
        [DataRow("85.24.60.44")]
        [DataRow("198.11.10.11")]
        public void ValueShouldReturnZeroAsLastIpNumber(string ip)
        {
            var location = new TruncatedLocation(ip);

            location.Value().Split('.')[3].Should().Be("0");
        }

        [TestMethod]
        public void ValueShouldBeEmptyIfIpIsNull()
        {
            var location = new TruncatedLocation(null);

            location.Value().Should().Be(string.Empty);
        }

        [TestMethod]
        public void ValueShouldBeEmptyIfIpIsEmpty()
        {
            var location = new TruncatedLocation(string.Empty);

            location.Value().Should().Be(string.Empty);
        }
    }
}
