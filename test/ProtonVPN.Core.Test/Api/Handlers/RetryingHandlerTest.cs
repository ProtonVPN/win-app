/*
 * Copyright (c) 2021 Proton Technologies AG
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

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.Core.Api.Handlers;
using RichardSzalay.MockHttp;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Polly.Timeout;

namespace ProtonVPN.Core.Test.Api.Handlers
{
    [TestClass]
    public class RetryingHandlerTest
    {
        [TestMethod]
        public async Task It_ShouldRetry_WhenRequestFails()
        {
            // Arrange
            TimeSpan timeout = TimeSpan.FromSeconds(5);
            const int maxRetries = 3;

            MockHttpMessageHandler innerHandler = new();
            MockedRequest request = innerHandler.When("*").Respond(_ => new HttpResponseMessage((HttpStatusCode) 429));

            RetryingHandler handler = new(timeout, timeout, maxRetries, (i, result, arg3) => TimeSpan.Zero)
            {
                InnerHandler = innerHandler
            };
            HttpClient httpClient = new(handler) { BaseAddress = new Uri("http://127.0.0.1") };
            
            // Act
            await httpClient.SendAsync(new HttpRequestMessage());
            
            // Assert
            innerHandler.GetMatchCount(request).Should().Be(maxRetries + 1);
        }

        [TestMethod]
        public void It_ShouldTimeOut()
        {
            // Arrange
            var timeout = TimeSpan.FromMilliseconds(100);
            const int maxRetries = 0;

            MockHttpMessageHandler innerHandler = new() {AutoFlush = false};
            innerHandler.When("*").Respond(_ => new HttpResponseMessage(HttpStatusCode.OK));

            RetryingHandler handler = new(timeout, timeout, maxRetries, (i, result, arg3) => TimeSpan.Zero)
            {
                InnerHandler = innerHandler
            };
            HttpClient httpClient = new(handler) { BaseAddress = new Uri("http://127.0.0.1") };

            // Act
            Task.Delay(TimeSpan.FromMilliseconds(200))
                .ContinueWith(_ => innerHandler.Flush());

            Func<Task> action = () => httpClient.SendAsync(new HttpRequestMessage());

            // Assert
            action.Should().Throw<TimeoutRejectedException>();
        }
    }
}
