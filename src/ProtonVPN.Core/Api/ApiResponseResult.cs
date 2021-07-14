/*
 * Copyright (c) 2020 Proton Technologies AG
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
using ProtonVPN.Common.Abstract;
using System.Net;
using ProtonVPN.Core.Api.Contracts;

namespace ProtonVPN.Core.Api
{
    public class ApiResponseResult<T> : Result<T>
        where T : BaseResponse
    {
        protected ApiResponseResult(HttpStatusCode code, bool success, string error, T value)
            : base(value, success, error)
        {
            StatusCode = code;
            Actions = value?.Details?.Actions;
        }

        protected ApiResponseResult(HttpStatusCode code, bool success, BaseResponse other)
            : this(code, success, other?.Error)
        {
            Actions = other?.Details?.Actions;
        }

        protected ApiResponseResult(HttpStatusCode code, bool success, string error)
            : base(default(T), success, error)
        {
            StatusCode = code;
        }

        public HttpStatusCode StatusCode { get; }
        public IList<BaseResponseDetailAction> Actions { get; }

        public static ApiResponseResult<T> Ok(T value)
        {
            return new ApiResponseResult<T>(0, true, "", value);
        }

        public static ApiResponseResult<T> Fail(HttpStatusCode code, string error)
        {
            return new ApiResponseResult<T>(code, false, error);
        }

        public static ApiResponseResult<T> Fail(T value, HttpStatusCode code, string error)
        {
            return new ApiResponseResult<T>(code, false, error, value);
        }

        public static ApiResponseResult<T> Fail<TOther>(ApiResponseResult<TOther> other)
            where TOther : BaseResponse
        {
            return new ApiResponseResult<T>(other.StatusCode, false, other.Value);
        }
    }
}
