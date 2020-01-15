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

using DnsClient;
using DnsClient.Protocol;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Core.OS.Net.Dns;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ProtonVPN.Core.Test.OS.Net.Dns
{
    [TestClass]
    [SuppressMessage("ReSharper", "CoVariantArrayConversion")]
    [SuppressMessage("ReSharper", "UseObjectOrCollectionInitializer")]
    public class FixedDnsClientTest
    {
        private ILookupClient _lookupClient;

        [TestInitialize]
        public void TestInitialize()
        {
            _lookupClient = Substitute.For<ILookupClient>();
        }

        [TestMethod]
        public void FixedDnsClient_ShouldThrow_WhenLookupClient_IsNull()
        {
            // Act
            Action action = () => new FixedDnsClient(null);
            // Assert
            action.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public async Task Resolve_ShouldResolve_IPAddress()
        {
            // Arrange
            const string ip = "134.27.41.216";
            var client = new FixedDnsClient(_lookupClient);
            // Act
            var result = await client.Resolve(ip, CancellationToken.None);
            // Assert
            result.Should().Be(ip);
        }

        [TestMethod]
        public async Task Resolve_ShouldResolve_HostName()
        {
            // Arrange
            const string host = "some.host.com";
            const string ip = "134.27.41.216";
            var token = new CancellationToken();
            _lookupClient.QueryAsync(host, QueryType.A, cancellationToken: token)
                .Returns(new DnsQueryResponse
                {
                    HasError = false,
                    Answers = new DnsResourceRecord[] 
                    {
                        new ARecord(
                            new ResourceRecordInfo(host, ResourceRecordType.A, QueryClass.IN, 5, 5),
                            IPAddress.Parse(ip))
                    }
                });
            var client = new FixedDnsClient(_lookupClient);
            // Act
            var result = await client.Resolve(ip, token);
            // Assert
            result.Should().Be(ip);
        }

        [TestMethod]
        public void NameServers_ShouldBe_LookupClientNameServers()
        {
            // Arrange
            var nameServers = new []
            {
                new IPEndPoint(IPAddress.Parse("15.46.251.79"), 53),
                new IPEndPoint(IPAddress.Parse("8.8.8.8"), 53),
                new IPEndPoint(IPAddress.Parse("10.3.15.47"), 66)
            };
            _lookupClient.NameServers.Returns(nameServers.Select(s => new NameServer(s)).ToList());
            var client = new FixedDnsClient(_lookupClient);
            // Act
            var result = client.NameServers;
            // Assert
            result.Should().BeEquivalentTo(nameServers);
        }

        #region Helpers

        private class DnsQueryResponse : IDnsQueryResponse
        {
            public IReadOnlyList<DnsQuestion> Questions { get; set; }
            public IReadOnlyList<DnsResourceRecord> Additionals { get; set; }
            public IEnumerable<DnsResourceRecord> AllRecords { get; set; }
            public IReadOnlyList<DnsResourceRecord> Answers { get; set; }
            public IReadOnlyList<DnsResourceRecord> Authorities { get; set; }
            public string AuditTrail { get; set; }
            public string ErrorMessage { get; set; }
            public bool HasError { get; set; }
            public DnsResponseHeader Header { get; set; }
            public int MessageSize { get; set; }
            public NameServer NameServer { get; set; }
        }

        #endregion
    }
}
