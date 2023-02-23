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

using System.Diagnostics.CodeAnalysis;
using System.Net;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.Core.OS.Net.Dns;

namespace ProtonVPN.Core.Tests.OS.Net.Dns
{
    [TestClass]
    [SuppressMessage("ReSharper", "CoVariantArrayConversion")]
    public class DnsClientsTest
    {
        [TestMethod]
        public void DnsClient_ShouldUse_DnsServers()
        {
            // Arrange
            var nameServers = new[]
            {
                new IPEndPoint(IPAddress.Parse("15.46.251.79"), 53),
                new IPEndPoint(IPAddress.Parse("8.8.8.8"), 53),
                new IPEndPoint(IPAddress.Parse("10.3.15.47"), 66)
            };
            var clients = new DnsClients();
            // Act
            var result = clients.DnsClient(nameServers).NameServers;
            // Assert
            result.Should().BeEquivalentTo(nameServers);
        }

        [TestMethod]
        public void DnsClient_ShouldBe_NullDnsServer_WhenNameServers_IsEmpty()
        {
            // Arrange
            var nameServers = new IPEndPoint[0];
            var clients = new DnsClients();
            // Act
            var result = clients.DnsClient(nameServers);
            // Assert
            result.Should().BeSameAs(DnsClients.NullDnsClient);
        }

        [TestMethod]
        public void NullDnsClient_ShouldBe_NullDnsClient()
        {
            // Act
            var result = DnsClients.NullDnsClient;
            //
            result.Should().BeOfType<NullDnsClient>();
        }
    }
}
