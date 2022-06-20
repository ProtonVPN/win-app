/*
 * Copyright (c) 2022 Proton Technologies AG
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
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Polly.Timeout;
using ProtonVPN.Api.Contracts;
using ProtonVPN.Api.Handlers.Retries;
using ProtonVPN.Common.Logging;
using RichardSzalay.MockHttp;

namespace ProtonVPN.Api.Tests.Handlers.Retries
{
    [TestClass]
    public class RetryingHandlerTest
    {
        [TestMethod]
        public async Task It_ShouldRetry_WhenRequestFails()
        {
            // Arrange
            const int maxRetries = 3;
            MockHttpMessageHandler mockHttpMessageHandler = new();
            MockedRequest request = mockHttpMessageHandler.When("*")
                .Respond(_ =>
                {
                    HttpResponseMessage response = new(ExpandedHttpStatusCodes.TOO_MANY_REQUESTS);
                    response.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(1));
                    return response;
                });
            MockOfLoggingHandler loggingHandler = new(mockHttpMessageHandler);
            RetryingHandler handler = new(loggingHandler, GetRetryPolicyProvider(maxRetries));
            HttpClient httpClient = new(handler) { BaseAddress = new Uri("http://127.0.0.1") };

            // Act
            await httpClient.SendAsync(new HttpRequestMessage());

            // Assert
            mockHttpMessageHandler.GetMatchCount(request).Should().Be(maxRetries + 1);
        }

        [TestMethod]
        public void It_ShouldTimeOut()
        {
            // Arrange
            const int maxRetries = 0;

            MockHttpMessageHandler innerHandler = new() { AutoFlush = false };
            innerHandler.When("*").Respond(_ => new HttpResponseMessage(HttpStatusCode.OK));

            MockOfLoggingHandler loggingHandler = new(innerHandler);
            RetryingHandler handler = new(loggingHandler, GetRetryPolicyProvider(maxRetries)) { InnerHandler = innerHandler };
            HttpClient httpClient = new(handler) { BaseAddress = new Uri("http://127.0.0.1") };

            // Act
            Task.Delay(TimeSpan.FromMilliseconds(200))
                .ContinueWith(_ => innerHandler.Flush());

            Func<Task> action = () => httpClient.SendAsync(new HttpRequestMessage());

            // Assert
            action.Should().ThrowAsync<TimeoutRejectedException>();
        }

        private IRetryPolicyProvider GetRetryPolicyProvider(int retryCount)
        {
            IRetryCountProvider retryCountProvider = Substitute.For<IRetryCountProvider>();
            retryCountProvider.GetRetryCount(Arg.Any<HttpRequestMessage>()).Returns(retryCount);

            IRequestTimeoutProvider requestTimeoutProvider = Substitute.For<IRequestTimeoutProvider>();
            requestTimeoutProvider.GetTimeout(Arg.Any<HttpRequestMessage>()).Returns(TimeSpan.FromSeconds(1));

            ILogger logger = Substitute.For<ILogger>();

            return new RetryPolicyProvider(logger, new SleepDurationProvider(), retryCountProvider,
                requestTimeoutProvider);
        }
    }
}