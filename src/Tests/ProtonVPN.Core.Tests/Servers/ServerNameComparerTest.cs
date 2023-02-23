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

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.Core.Servers;

namespace ProtonVPN.Core.Tests.Servers
{
    [TestClass]
    public class ServerNameComparerTest
    {
        [TestMethod]
        [DataRow("NL-FREE#1", "NL-FREE#2", -1)]
        [DataRow("JP-FREE#194120", "NL-FREE#351157", -1)]
        [DataRow("NL-FREE#10", "NL-FREE#1", 1)]
        [DataRow("NL#10", "NL-FREE#NEWS", -1)]
        [DataRow("ServerA", "Server B", 1)]
        [DataRow("FR#13-TOR", "FR#1", 1)]
        [DataRow("FR#13-TOR", "FR#2-TOR", 1)]
        [DataRow("NL-FREE#2-NEWS", "NL-FREE#10-NEWS", -1)]
        [DataRow("NL#-TOR", "NL#1-TOR", 1)]
        [DataRow("SE-LT#1", "SE-LT#10", -1)]
        [DataRow("#", "#B", -1)]
        [DataRow("#", ".", -1)]
        [DataRow("Server#Test 1", "Server# B2", 1)]
        [DataRow("123", "987", -1)]
        [DataRow(null, "Server#5", -1)]
        [DataRow("", "Server#5", -1)]
        [DataRow("Server", null, 1)]
        [DataRow("Server", "", 1)]
        [DataRow(null, null, 0)]
        [DataRow("", "", 0)]
        [DataRow(null, "", 0)]
        [DataRow("", null, 0)]
        public void ItShouldCompareNames(string name1, string name2, int expectedResult)
        {
            ServerNameComparer serverNameComparer = new();
            serverNameComparer.Compare(name1, name2).Should().Be(expectedResult);
        }
    }
}