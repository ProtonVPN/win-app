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

using System.Threading.Tasks;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Core.Api;
using ProtonVPN.Core.Api.Contracts;
using ProtonVPN.Core.Settings;

namespace ProtonVPN.Core.Auth
{
    public class UserValidator
    {
        private readonly IUserStorage _userStorage;
        private readonly UserAuth _userAuth;

        public UserValidator(IUserStorage userStorage, UserAuth userAuth)
        {
            _userStorage = userStorage;
            _userAuth = userAuth;
        }

        public async Task<AuthResult> GetValidateResult()
        {
            if (!_userStorage.User().VpnPlan.IsNullOrEmpty())
            {
                return AuthResult.Ok();
            }

            ApiResponseResult<VpnInfoResponse> vpnInfoResult = await _userAuth.RefreshVpnInfo();
            if (vpnInfoResult.Failure)
            {
                return AuthResult.Fail(vpnInfoResult);
            }

            _userStorage.StoreVpnInfo(vpnInfoResult.Value);
            return AuthResult.Ok();
        }
    }
}