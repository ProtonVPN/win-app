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
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.Dns.Resolvers.System;
using ProtonVPN.Dns.Tests.Mocks;

namespace ProtonVPN.Dns.Tests.Resolvers.System
{
    [TestClass]
    public class SystemDnsResolverTest
    {
        private const string HOST = "api.protonvpn.ch";

        private MockOfLogger _logger;
        private SystemDnsResolver _appSettings;
        private CancellationTokenSource _cancellationTokenSource;

        [TestInitialize]
        public void TestInitialize()
        {
            _logger = new MockOfLogger();
            _appSettings = new SystemDnsResolver(_logger);
            _cancellationTokenSource = new CancellationTokenSource();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _logger = null;
            _appSettings = null;
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = null;
        }

        [TestMethod]
        public async Task TestResolveWithSystemAsync()
        {
            IList<IPAddress> result = await _appSettings.ResolveWithSystemAsync(HOST, _cancellationTokenSource.Token);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count > 0);
        }

        [TestMethod]
        public async Task TestResolveWithSystemAsync_WithNonExistentHost()
        {
            string host = "g5f16gfds1gdsf5g16dsfg15fs5gfds651d61s651g6516gf1s6fdgfs.vhbverhu";

            IList<IPAddress> result = await _appSettings.ResolveWithSystemAsync(host, _cancellationTokenSource.Token);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task TestResolveWithSystemAsync_WithCancelledToken()
        {
            CancellationToken cancellationToken = _cancellationTokenSource.Token;
            _cancellationTokenSource.Cancel();

            IList<IPAddress> result = await _appSettings.ResolveWithSystemAsync(HOST, cancellationToken);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }
    }
}