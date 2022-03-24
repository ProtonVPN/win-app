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

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using ProtonVPN.Core.Api.Contracts;
using ProtonVPN.Core.Api.Handlers;
using RichardSzalay.MockHttp;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace ProtonVPN.Core.Test.Api.Handlers
{
    [TestClass]
    public class OutdatedAppHandlerTest
    {
        [TestMethod]
        [DataRow(5003)]
        [DataRow(5005)]
        public async Task ItShouldInvokeOutdatedAppEvent(int code)
        {
            //arrange
            var called = 0;
            var fakeHttpMessageHandler = new MockHttpMessageHandler();
            var handler = new OutdatedAppHandler {InnerHandler = fakeHttpMessageHandler };
            handler.AppOutdated += (sender, args) => called++;
            var httpClient = new HttpClient(handler) {BaseAddress = new Uri("http://127.0.0.1")};
            fakeHttpMessageHandler.When("*").Respond(req => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(new BaseResponse { Code = code}))
            });

            //act
            await httpClient.SendAsync(new HttpRequestMessage());

            //assert
            called.Should().Be(1);
        }
    }
}
