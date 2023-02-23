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

using ProtonVPN.Api.Contracts;
using ProtonVPN.Api.Contracts.Common;
using ProtonVPN.Common.Abstract;
using ProtonVPN.Common.Extensions;

namespace ProtonVPN.Core.Auth
{
    public class AuthResult : Result<AuthError>
    {
        protected internal AuthResult(AuthError value, bool success, string error) : base(value, success, error)
        {
        }

        public static AuthResult Fail(AuthError authError)
        {
            return new(authError, false, string.Empty);
        }

        public new static AuthResult Fail(string error)
        {
            return new(AuthError.Unknown, false, error);
        }

        public static AuthResult Fail<T>(ApiResponseResult<T> apiResponseResult) where T : BaseResponse
        {
            if (apiResponseResult.Actions.IsNullOrEmpty())
            {
                return apiResponseResult.Value?.Code == ResponseCodes.NoVpnConnectionsAssigned
                    ? Fail(AuthError.NoVpnAccess)
                    : Fail(apiResponseResult.Error);
            }

            return Fail();
        }

        public static AuthResult Fail()
        {
            return new(AuthError.None, false, string.Empty);
        }

        public new static AuthResult Ok()
        {
            return new(AuthError.None, true, string.Empty);
        }
    }
}