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
using Newtonsoft.Json;
using ProtonVPN.Api.Contracts.Common;
using ProtonVPN.Api.Handlers.Retries;
using RichardSzalay.MockHttp;

namespace ProtonVPN.Api.Tests.Mocks
{
    public class MockOfRetryingHandler : RetryingHandlerBase
    {
        private readonly MockHttpMessageHandler _fakeHttpMessageHandler = new();

        public MockOfRetryingHandler()
        {
            InnerHandler = _fakeHttpMessageHandler;
        }

        public void SetResponseAsSuccess(int code)
        {
            _fakeHttpMessageHandler.When("*").Respond(req => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(new BaseResponse { Code = code }))
            });
        }
    }
}