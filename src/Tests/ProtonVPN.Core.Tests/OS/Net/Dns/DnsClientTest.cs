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

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Common.OS.Net.NetworkInterface;
using ProtonVPN.Core.OS.Net.Dns;

namespace ProtonVPN.Core.Tests.OS.Net.Dns
{
    [TestClass]
    [SuppressMessage("ReSharper", "CoVariantArrayConversion")]
    public class DnsClientTest
    {
        private IDnsClients _dnsClients;
        private INetworkInterfaces _networkInterfaces;

        [TestInitialize]
        public void TestInitialize()
        {
            _dnsClients = Substitute.For<IDnsClients>();
            _networkInterfaces = Substitute.For<INetworkInterfaces>();
        }

        [TestMethod]
        public async Task Resolve_ShouldUse_DnsClients_DnsServers()
        {
            // Arrange
            var nameServers = new[]
            {
                new IPEndPoint(IPAddress.Parse("15.46.251.79"), 53),
                new IPEndPoint(IPAddress.Parse("8.8.8.8"), 53),
                new IPEndPoint(IPAddress.Parse("10.3.15.47"), 66)
            };
            _dnsClients.NameServers().Returns(nameServers.ToList());

            _dnsClients.DnsClient(Arg.Any<IReadOnlyCollection<IPEndPoint>>()).Returns(args =>
            {
                var dnsClient = Substitute.For<IDnsClient>();
                dnsClient.NameServers.Returns(args.ArgAt<IReadOnlyCollection<IPEndPoint>>(0));
                return dnsClient;
            });

            var client = new Core.OS.Net.Dns.DnsClient(_dnsClients, _networkInterfaces);
            
            // Act
            await client.Resolve("some.host", CancellationToken.None);

            // Assert
            client.NameServers.Should().BeEquivalentTo(nameServers);
        }

        [TestMethod]
        public async Task Resolve_ShouldRefresh_DnsServers_When_NetworkAddressChanged()
        {
            // Arrange
            var nameServers = new[]
            {
                new IPEndPoint(IPAddress.Parse("15.46.251.79"), 53),
                new IPEndPoint(IPAddress.Parse("8.8.8.8"), 53),
                new IPEndPoint(IPAddress.Parse("10.3.15.47"), 66)
            };
            _dnsClients.NameServers().Returns(nameServers.ToList());

            _dnsClients.DnsClient(Arg.Any<IReadOnlyCollection<IPEndPoint>>()).Returns(args =>
            {
                var dnsClient = Substitute.For<IDnsClient>();
                dnsClient.NameServers.Returns(args.ArgAt<IReadOnlyCollection<IPEndPoint>>(0));
                return dnsClient;
            });

            var client = new Core.OS.Net.Dns.DnsClient(_dnsClients, _networkInterfaces);

            // Call Resolve the DNS client to initialize
            await client.Resolve("some.host", CancellationToken.None);
            client.NameServers.Should().BeEquivalentTo(nameServers);

            // Act

            // Change DNS servers
            var newNameServers = new[]
            {
                new IPEndPoint(IPAddress.Parse("1.2.3.4"), 53),
                new IPEndPoint(IPAddress.Parse("8.8.8.8"), 53)
            };
            _dnsClients.NameServers().Returns(newNameServers.ToList());

            // Raise NetworkAddressChanged
            _networkInterfaces.NetworkAddressChanged += Raise.Event();

            // Call Resolve the DNS client to refresh DNS servers
            await client.Resolve("another.host", CancellationToken.None);

            // Assert
            client.NameServers.Should().BeEquivalentTo(newNameServers);
        }

        [TestMethod]
        public async Task Resolve_ShouldNotRefresh_DnsServers_WhenNo_NetworkAddressChanged()
        {
            // Arrange
            var nameServers = new[]
            {
                new IPEndPoint(IPAddress.Parse("15.46.251.79"), 53),
                new IPEndPoint(IPAddress.Parse("8.8.8.8"), 53),
                new IPEndPoint(IPAddress.Parse("10.3.15.47"), 66)
            };
            _dnsClients.NameServers().Returns(nameServers.ToList());

            _dnsClients.DnsClient(Arg.Any<IReadOnlyCollection<IPEndPoint>>()).Returns(args =>
            {
                var dnsClient = Substitute.For<IDnsClient>();
                dnsClient.NameServers.Returns(args.ArgAt<IReadOnlyCollection<IPEndPoint>>(0));
                return dnsClient;
            });

            var client = new Core.OS.Net.Dns.DnsClient(_dnsClients, _networkInterfaces);

            // Call Resolve the DNS client to initialize
            await client.Resolve("some.host", CancellationToken.None);
            client.NameServers.Should().BeEquivalentTo(nameServers);

            // Act

            // Change DNS servers
            var newNameServers = new[]
            {
                new IPEndPoint(IPAddress.Parse("1.2.3.4"), 53),
                new IPEndPoint(IPAddress.Parse("8.8.8.8"), 53)
            };
            _dnsClients.NameServers().Returns(newNameServers.ToList());

            await client.Resolve("another.host", CancellationToken.None);

            // Assert
            client.NameServers.Should().BeEquivalentTo(nameServers);
        }
    }
}
