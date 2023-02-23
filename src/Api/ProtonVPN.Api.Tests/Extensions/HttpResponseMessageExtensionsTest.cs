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

using System.Net;
using System.Net.Http;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.Api.Contracts;
using ProtonVPN.Api.Extensions;

namespace ProtonVPN.Api.Tests.Extensions
{
    [TestClass]
    public class HttpResponseMessageExtensionsTest
    {
        [TestMethod]
        [DataRow(HttpStatusCode.NotFound)]
        [DataRow(HttpStatusCode.OK)]
        [DataRow(HttpStatusCode.BadRequest)]
        [DataRow(HttpStatusCode.Conflict)]
        [DataRow(ExpandedHttpStatusCodes.UNPROCESSABLE_ENTITY)]
        public void IsToRetry_ShouldAvoidRetryingOnSpecificStatusCodes(HttpStatusCode httpStatusCode)
        {
            // Arrange
            HttpResponseMessage message = GetResponseMessage(httpStatusCode);

            // Assert
            message.IsToRetry().Should().BeFalse();
        }

        [TestMethod]
        public void IsToRetry_ShouldNotRetryOn503WithoutRetryAfterHeader()
        {
            // Arrange
            HttpResponseMessage message = GetResponseMessage(HttpStatusCode.ServiceUnavailable);

            // Assert
            message.IsToRetry().Should().BeFalse();
        }

        [TestMethod]
        [DataRow(HttpStatusCode.ServiceUnavailable)]
        [DataRow(ExpandedHttpStatusCodes.TOO_MANY_REQUESTS)]
        public void IsToRetry_ShouldRetryOnHttpStatusCodeWithRetryAfterHeader(HttpStatusCode httpStatusCode)
        {
            // Arrange
            HttpResponseMessage message = GetResponseMessage(httpStatusCode);
            message.Headers.Add("Retry-After", "10");

            // Assert
            message.IsToRetry().Should().BeTrue();
        }

        [TestMethod]
        public void IsToRetry_ShouldNotRetryOnTooManyRequestWithoutRetryAfterHeader()
        {
            // Arrange
            HttpResponseMessage message = GetResponseMessage(ExpandedHttpStatusCodes.TOO_MANY_REQUESTS);

            // Assert
            message.IsToRetry().Should().BeFalse();
        }

        [TestMethod]
        [DataRow(HttpStatusCode.RequestTimeout)]
        [DataRow(HttpStatusCode.BadGateway)]
        public void IsToRetryOnce_ShouldReturnTrue(HttpStatusCode httpStatusCode)
        {
            // Arrange
            HttpResponseMessage message = GetResponseMessage(httpStatusCode);

            // Assert
            message.IsToRetryOnce().Should().BeTrue();
        }

        [TestMethod]
        [DataRow(HttpStatusCode.OK)]
        [DataRow(HttpStatusCode.InternalServerError)]
        public void IsToRetryOnce_ShouldReturnFalse(HttpStatusCode httpStatusCode)
        {
            // Arrange
            HttpResponseMessage message = GetResponseMessage(httpStatusCode);

            // Assert
            message.IsToRetryOnce().Should().BeFalse();
        }

        private HttpResponseMessage GetResponseMessage(HttpStatusCode statusCode)
        {
            return new HttpResponseMessage
            {
                Content = new StringContent(string.Empty),
                StatusCode = statusCode
            };
        }
    }
}