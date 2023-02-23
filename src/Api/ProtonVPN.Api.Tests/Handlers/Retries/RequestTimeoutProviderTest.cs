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
using System.Net.Http;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.Api.Handlers.Retries;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.OS.Net.Http;

namespace ProtonVPN.Api.Tests.Handlers.Retries
{
    [TestClass]
    public class RequestTimeoutProviderTest
    {
        [TestMethod]
        public void ItShouldUseCustomTimeout()
        {
            // Arrange
            IConfiguration config = new Config();
            RequestTimeoutProvider sut = new(config);
            HttpRequestMessage request = new();
            TimeSpan timeout = TimeSpan.FromSeconds(30);
            request.SetCustomTimeout(timeout);

            // Assert
            sut.GetTimeout(request).Should().Be(timeout);
        }

        [TestMethod]
        public void ItShouldUseDefaultTimeoutWhenNotUploadingFiles()
        {
            // Arrange
            TimeSpan timeout = TimeSpan.FromSeconds(60);
            IConfiguration config = new Config { ApiTimeout = timeout };
            RequestTimeoutProvider sut = new(config);

            // Assert
            sut.GetTimeout(new HttpRequestMessage()).Should().Be(timeout);
        }

        [TestMethod]
        public void ItShouldUseSpecialTimeoutForUploadRequests()
        {
            // Arrange
            TimeSpan timeout = TimeSpan.FromSeconds(120);
            IConfiguration config = new Config { ApiUploadTimeout = timeout };
            HttpRequestMessage request = new()
            {
                Content = new MultipartFormDataContent()
            };
            RequestTimeoutProvider sut = new(config);

            // Assert
            sut.GetTimeout(request).Should().Be(timeout);
        }
    }
}