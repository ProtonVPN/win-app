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
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using DnsClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Dns.NameServers;
using ProtonVPN.Dns.Tests.Mocks;

namespace ProtonVPN.Dns.Tests.NameServers
{
    [TestClass]
    public class NameServersLoaderTest
    {
        [TestMethod]
        public void TestGet()
        {
            MockOfLogger logger = new();
            NameServersResolver realNameServersResolver = new();
            NameServersLoader nameServersLoader = new(realNameServersResolver, logger);

            IList<IPEndPoint> result = nameServersLoader.Get();

            Assert.IsTrue(result.Count > 0);
            result.ForEach(AssertIPEndPoint);
        }

        private void AssertIPEndPoint(IPEndPoint endpoint)
        {
            Assert.AreEqual(MockOfNameServersLoader.DNS_PORT, endpoint.Port);
            Assert.AreEqual(AddressFamily.InterNetwork, endpoint.AddressFamily);
            Assert.AreNotEqual(IPAddress.Loopback, endpoint.Address);
            Assert.AreNotEqual(IPAddress.Any, endpoint.Address);
            Assert.AreNotEqual(IPAddress.Broadcast, endpoint.Address);
            Assert.AreNotEqual(IPAddress.None, endpoint.Address);
        }

        [TestMethod]
        public void TestGet_WithWrongIpAddresses()
        {
            MockOfLogger logger = new();
            IReadOnlyCollection<NameServer> expectedNameServers = new List<NameServer>
            {
                new NameServer(IPAddress.None),
                new NameServer(IPAddress.Any),
                new NameServer(IPAddress.Broadcast),
                new NameServer(IPAddress.Loopback)
            };
            MockOfNameServersResolver mockOfNameServersResolver = new(expectedNameServers);
            NameServersLoader nameServersLoader = new(mockOfNameServersResolver, logger);

            IList<IPEndPoint> result = nameServersLoader.Get();

            Assert.IsTrue(result.Count == 0);
        }

        [TestMethod]
        public void TestGet_WithWrongAndCorrectIpAddresses()
        {
            MockOfLogger logger = new();
            IReadOnlyCollection<NameServer> expectedNameServers = new List<NameServer>
            {
                new NameServer(IPAddress.None),
                new NameServer(IPAddress.Parse("192.168.123.123")),
                new NameServer(IPAddress.Any),
                new NameServer(IPAddress.Parse("172.18.231.231")),
                new NameServer(IPAddress.Broadcast),
                new NameServer(IPAddress.Parse("10.99.99.99")),
                new NameServer(IPAddress.Loopback)
            };
            MockOfNameServersResolver mockOfNameServersResolver = new(expectedNameServers);
            NameServersLoader nameServersLoader = new(mockOfNameServersResolver, logger);

            IList<IPEndPoint> result = nameServersLoader.Get();

            Assert.AreEqual(3, result.Count);
            result.ForEach(AssertIPEndPoint);
        }

        [TestMethod]
        public void TestGet_WithCorrectIpAddresses()
        {
            MockOfLogger logger = new();
            IReadOnlyCollection<NameServer> expectedNameServers = new List<NameServer>
            {
                new NameServer(IPAddress.Parse("192.168.123.123")),
                new NameServer(IPAddress.Parse("172.18.231.231")),
                new NameServer(IPAddress.Parse("10.99.99.99"))
            };
            MockOfNameServersResolver mockOfNameServersResolver = new(expectedNameServers);
            NameServersLoader nameServersLoader = new(mockOfNameServersResolver, logger);

            IList<IPEndPoint> result = nameServersLoader.Get();

            Assert.AreEqual(3, result.Count);
            result.ForEach(AssertIPEndPoint);
        }

        [TestMethod]
        public void TestGet_WithExceptionOnResolver()
        {
            MockOfLogger logger = new();
            MockOfNameServersResolver mockOfNameServersResolver = new(new Exception("Test"));
            NameServersLoader nameServersLoader = new(mockOfNameServersResolver, logger);

            IList<IPEndPoint> result = nameServersLoader.Get();

            Assert.AreEqual(0, result.Count);
        }
    }
}