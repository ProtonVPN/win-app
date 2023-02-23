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
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.Core.OS.Net.Dns;

namespace ProtonVPN.Core.Tests.OS.Net.Dns
{
    [TestClass]
    public class NullDnsClientTest
    {
        [TestMethod]
        public async Task Resolve_ShouldBe_Null()
        {
            // Arrange
            var client = DnsClients.NullDnsClient;
            // Act
            var result = await client.Resolve("aaa.bbb", CancellationToken.None);
            // Assert
            result.Should().BeNullOrEmpty();
        }

        [TestMethod]
        public void NameServers_ShouldBe_Empty()
        {
            // Arrange
            var client = DnsClients.NullDnsClient;
            // Act
            var result = client.NameServers;
            // Assert
            result.Should().BeEmpty();
        }
    }
}
