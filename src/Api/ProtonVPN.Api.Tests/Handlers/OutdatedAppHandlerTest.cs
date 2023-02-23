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
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.Api.Contracts;
using ProtonVPN.Api.Contracts.Common;
using ProtonVPN.Api.Handlers;
using ProtonVPN.Api.Tests.Deserializers;
using ProtonVPN.Api.Tests.Mocks;

namespace ProtonVPN.Api.Tests.Handlers
{
    [TestClass]
    public class OutdatedAppHandlerTest
    {
        [TestMethod]
        [DataRow(ResponseCodes.OutdatedAppResponse)]
        [DataRow(ResponseCodes.OutdatedApiResponse)]
        public async Task ItShouldInvokeOutdatedAppEvent(int code)
        {
            // Arrange
            int called = 0;

            MockOfRetryingHandler mockOfRetryingHandler = new();
            MockOfBaseResponseMessageDeserializer mockOfBaseResponseDeserializer = new()
            {
                ExpectedBaseResponse = new BaseResponse() { Code = code }
            };
            mockOfRetryingHandler.SetResponseAsSuccess(code);
            OutdatedAppHandler handler = new(mockOfBaseResponseDeserializer) { InnerHandler = mockOfRetryingHandler };
            handler.AppOutdated += (sender, args) => called++;
            HttpClient httpClient = new(handler) {BaseAddress = new Uri("http://127.0.0.1")};

            // Act
            await httpClient.SendAsync(new HttpRequestMessage());

            // Assert
            called.Should().Be(1);
        }
    }
}