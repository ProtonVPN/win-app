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
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Core.OS.Net.Dns;

namespace ProtonVPN.Core.Tests.OS.Net.Dns
{
    [TestClass]
    [SuppressMessage("ReSharper", "InvokeAsExtensionMethod")]
    public class DnsClientExtensionsTest
    {
        private IDnsClient _dnsClient;

        [TestInitialize]
        public void TestInitialize()
        {
            _dnsClient = Substitute.For<IDnsClient>();
        }

        [TestMethod]
        public async Task Resolve_ShouldCall_DnsClientResolve_WithHostParameter()
        {
            // Arrange
            const string host = "hostname";
            const string ip = "125.247.41.7";
            _ = _dnsClient.Resolve(host, Arg.Any<CancellationToken>()).Returns(ip);
            // Act
            var result = await DnsClientExtensions.Resolve(_dnsClient, host, TimeSpan.Zero);
            // Assert
            result.Should().Be(ip);
        }

        [TestMethod]
        public async Task Resolve_ShouldBe_NullOrEmpty_WhenTimedOut()
        {
            // Arrange
            var timeout = TimeSpan.FromMilliseconds(100);
            _ = _dnsClient.Resolve(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(async _ => 
            {
                await Task.Delay(timeout + TimeSpan.FromMilliseconds(200));
                return "success";
            });
            // Act
            var result = await DnsClientExtensions.Resolve(_dnsClient, "", timeout);
            // Assert
            result.Should().BeNullOrEmpty();
        }

        [TestMethod]
        public async Task Resolve_ShouldCancel_CancellationToken_AfterTimeout()
        {
            // Arrange
            var timeout = TimeSpan.FromMilliseconds(100);
            CancellationToken token;
            _ = _dnsClient.Resolve(Arg.Any<string>(), Arg.Do<CancellationToken>(x => token = x)).Returns(async _ =>
            {
                await Task.Delay(timeout + TimeSpan.FromMilliseconds(200));
                return "success";
            });
            // Act
            await DnsClientExtensions.Resolve(_dnsClient, "", timeout);
            // Assert
            token.IsCancellationRequested.Should().BeTrue();
        }
    }
}
