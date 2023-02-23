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

using System.Net.Http;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.Api.Handlers.Retries;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.OS.Net.Http;

namespace ProtonVPN.Api.Tests.Handlers.Retries
{
    [TestClass]
    public class RetryCountProviderTest
    {
        [TestMethod]
        public void ItShouldUseDefaultRetryCount()
        {
            // Arrange
            int retryCount = 3;
            IConfiguration config = new Config() { ApiRetries = retryCount };
            RetryCountProvider sut = new(config);

            // Assert
            sut.GetRetryCount(new HttpRequestMessage()).Should().Be(retryCount);
        }

        [TestMethod]
        public void ItShouldUseCustomRetryCount()
        {
            // Arrange
            IConfiguration config = new Config() { ApiRetries = 3 };
            RetryCountProvider sut = new(config);
            HttpRequestMessage request = new();
            int retryCount = 10;
            request.SetRetryCount(retryCount);

            // Assert
            sut.GetRetryCount(request).Should().Be(retryCount);
        }
    }
}