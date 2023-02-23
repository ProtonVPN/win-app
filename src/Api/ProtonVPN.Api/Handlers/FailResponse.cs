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
using ProtonVPN.Api.Contracts;
using ProtonVPN.Api.Contracts.Common;

namespace ProtonVPN.Api.Handlers
{
    public class FailResponse : HttpResponseMessage
    {
        private FailResponse(HttpStatusCode httpStatusCode, int errorCode, string error = "") : base(httpStatusCode)
        {
            Content = new StringContent(
                JsonConvert.SerializeObject(
                    new BaseResponse {
                        Code = errorCode,
                        Error = error
                    }));
        }

        public static HttpResponseMessage UnauthorizedResponse()
        {
            return new FailResponse(HttpStatusCode.Unauthorized, 10013);
        }

        public static HttpResponseMessage HumanVerificationFailureResponse(string error)
        {
            return new FailResponse(HttpStatusCode.OK, ResponseCodes.HumanVerificationRequired, error);
        }
    }
}