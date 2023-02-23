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

using System.Collections.Generic;
using System.Net.Http;
using ProtonVPN.Api.Contracts.Common;
using ProtonVPN.Common.Abstract;

namespace ProtonVPN.Api.Contracts
{
    public class ApiResponseResult<T> : Result<T>
        where T : BaseResponse
    {
        public HttpResponseMessage ResponseMessage { get; }
        public IList<BaseResponseDetailAction> Actions { get; }

        protected ApiResponseResult(HttpResponseMessage responseMessage, bool success, string error, T value)
            : base(value, success, error)
        {
            ResponseMessage = responseMessage;
            Actions = value?.Details?.Actions;
        }

        protected ApiResponseResult(HttpResponseMessage responseMessage, bool success, string error)
            : base(default(T), success, error)
        {
            ResponseMessage = responseMessage;
        }

        public static ApiResponseResult<T> Ok(HttpResponseMessage responseMessage, T value)
        {
            return new ApiResponseResult<T>(responseMessage, true, "", value);
        }

        public static ApiResponseResult<T> Fail(HttpResponseMessage responseMessage, string error)
        {
            return new ApiResponseResult<T>(responseMessage, false, error);
        }

        public static ApiResponseResult<T> Fail(T value, HttpResponseMessage responseMessage, string error)
        {
            return new ApiResponseResult<T>(responseMessage, false, error, value);
        }
    }
}