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
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polly;
using ProtonVPN.Api.Handlers.Retries;

namespace ProtonVPN.Api.Tests.Handlers.Retries
{
    [TestClass]
    public class SleepDurationProviderTest
    {
        [TestMethod]
        public void ItShouldUseRetryAfterHeaderValue()
        {
            // Arrange
            TimeSpan retryAfter = TimeSpan.FromSeconds(25);
            HttpResponseMessage message = new();
            message.Headers.RetryAfter = new RetryConditionHeaderValue(retryAfter);
            DelegateResult<HttpResponseMessage> response = new(message);

            // Assert
            new SleepDurationProvider().ResponseMessageDurationFunction(3, response, null).Should().Be(retryAfter);
        }

        [TestMethod]
        public void ItShouldFallbackToExponentialBackoff()
        {
            // Arrange
            // Any other HttpStatusCode
            HttpResponseMessage message = new(HttpStatusCode.Unused);
            DelegateResult<HttpResponseMessage> response = new(message);
            TimeSpan expectedResult = TimeSpan.FromSeconds(4);

            // Assert
            new SleepDurationProvider().ResponseMessageDurationFunction(3, response, null).Should()
                .BeCloseTo(expectedResult, GetPrecision(expectedResult));
        }

        [TestMethod]
        public void ItShouldNotExceedSpecificTimeLimit()
        {
            // Arrange
            // Any other HttpStatusCode
            HttpResponseMessage message = new(HttpStatusCode.Unused);
            DelegateResult<HttpResponseMessage> response = new(message);
            TimeSpan expectedResult = TimeSpan.FromSeconds(128);

            // Assert
            new SleepDurationProvider().ResponseMessageDurationFunction(100, response, null).Should()
                .BeCloseTo(expectedResult, GetPrecision(expectedResult));
        }

        private TimeSpan GetPrecision(TimeSpan timeSpan)
        {
            return TimeSpan.FromMilliseconds(timeSpan.TotalMilliseconds * 0.2);
        }
    }
}