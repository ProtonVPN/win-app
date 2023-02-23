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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using DnsClient;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using ProtonVPN.Core.OS.Net.Dns;

namespace ProtonVPN.Core.Tests.OS.Net.Dns
{
    [TestClass]
    [SuppressMessage("ReSharper", "CoVariantArrayConversion")]
    [SuppressMessage("ReSharper", "ObjectCreationAsStatement")]
    public class SafeDnsClientTest
    {
        private IDnsClient _dnsClient;

        [TestInitialize]
        public void TestInitialize()
        {
            _dnsClient = Substitute.For<IDnsClient>();
        }

        [TestMethod]
        public void SafeDnsClient_ShouldThrow_WhenOrigin_IsNull()
        {
            // Act
            Action action = () => new SafeDnsClient(null);
            // Assert
            action.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public async Task Resolve_ShouldCall_OriginResolve_WithParameters()
        {
            // Arrange
            const string host = "hostname";
            var cancellationToken = CancellationToken.None;
            var client = new SafeDnsClient(_dnsClient);
            // Act
            await client.Resolve(host, cancellationToken);
            // Assert
            await _dnsClient.Received(1).Resolve(host, cancellationToken);
        }

        [TestMethod]
        public async Task Resolve_ShouldBe_OriginResolve()
        {
            // Arrange
            const string ip = "135.11.47.210";
            _dnsClient.Resolve("", Arg.Any<CancellationToken>()).ReturnsForAnyArgs(ip);
            var client = new SafeDnsClient(_dnsClient);
            // Act
            var result = await client.Resolve("host", CancellationToken.None);
            // Assert
            result.Should().Be(ip);
        }

        [DataTestMethod]
        [DataRow(typeof(SocketException))]
        [DataRow(typeof(DnsResponseException))]
        [DataRow(typeof(OperationCanceledException))]
        public async Task Resolve_ShouldSuppress_ExpectedException(Type exceptionType)
        {
            // Arrange
            var exception = (Exception) Activator.CreateInstance(exceptionType);
            _dnsClient.Resolve("", Arg.Any<CancellationToken>()).ThrowsForAnyArgs(exception);
            var client = new SafeDnsClient(_dnsClient);
            // Act
            var result = await client.Resolve("abc.def", CancellationToken.None);
            // Assert
            result.Should().BeNullOrEmpty();
        }

        [DataTestMethod]
        [DataRow(typeof(DnsResponseException))]
        [DataRow(typeof(OperationCanceledException))]
        public async Task Resolve_ShouldSuppress_ExpectedException_Async(Type exceptionType)
        {
            // Arrange
            var exception = (Exception)Activator.CreateInstance(exceptionType);
            _dnsClient.Resolve("", Arg.Any<CancellationToken>()).ThrowsForAnyArgs(exception);
            var client = new SafeDnsClient(_dnsClient);            
            // Act
            var result = await client.Resolve("abc.def", CancellationToken.None);
            // Assert
            result.Should().BeNullOrEmpty();
        }

        [TestMethod]
        public void Resolve_ShouldPass_Exception()
        {
            // Arrange
            _dnsClient.Resolve("", Arg.Any<CancellationToken>()).ThrowsForAnyArgs<Exception>();
            var client = new SafeDnsClient(_dnsClient);
            // Act
            Func<Task> action = () => client.Resolve("abc.def", CancellationToken.None);
            // Assert
            action.Should().ThrowAsync<Exception>();
        }

        [TestMethod]
        public void Resolve_ShouldPass_Exception_Async()
        {
            // Arrange
            _dnsClient.Resolve("", Arg.Any<CancellationToken>()).ReturnsForAnyArgs(Task.FromException<string>(new Exception()));
            var client = new SafeDnsClient(_dnsClient);
            // Act
            Func<Task> action = async () => await client.Resolve("abc.def", CancellationToken.None);
            // Assert
            action.Should().ThrowAsync<Exception>();
        }

        [TestMethod]
        public void NameServers_ShouldBe_OriginNameServers()
        {
            // Arrange
            var nameServers = new[]
            {
                new IPEndPoint(IPAddress.Parse("15.46.251.79"), 53),
                new IPEndPoint(IPAddress.Parse("8.8.8.8"), 53),
                new IPEndPoint(IPAddress.Parse("10.3.15.47"), 66)
            };
            _dnsClient.NameServers.Returns(nameServers.ToList());
            var client = new SafeDnsClient(_dnsClient);
            // Act
            var result = client.NameServers;
            // Assert
            result.Should().BeEquivalentTo(nameServers);
        }
    }
}
