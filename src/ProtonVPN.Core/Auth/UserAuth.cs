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

using System;
using System.Net.Http;
using System.Security;
using System.Threading.Tasks;
using ProtonVPN.Common.Logging;
using ProtonVPN.Core.Abstract;
using ProtonVPN.Core.Api;
using ProtonVPN.Core.Api.Contracts;
using ProtonVPN.Core.Api.Data;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Srp;

namespace ProtonVPN.Core.Auth
{
    public class UserAuth
    {
        private readonly IApiClient _apiClient;
        private readonly ILogger _logger;
        private readonly IUserStorage _userStorage;
        private readonly ITokenStorage _tokenStorage;

        public UserAuth(
            IApiClient apiClient,
            ILogger logger,
            IUserStorage userStorage,
            ITokenStorage tokenStorage)
        {
            _tokenStorage = tokenStorage;
            _apiClient = apiClient;
            _logger = logger;
            _userStorage = userStorage;
        }

        public const int UserStatusVpnAccess = 1;
        public bool LoggedIn { get; private set; }

        public event EventHandler<EventArgs> UserLoggedOut;
        public event EventHandler<UserLoggedInEventArgs> UserLoggedIn;
        public event EventHandler<EventArgs> UserLoggingIn;

        public async Task<ApiResponseResult<VpnInfoResponse>> RefreshVpnInfo()
        {
            ApiResponseResult<VpnInfoResponse> vpnInfo = await _apiClient.GetVpnInfoResponse();
            if (vpnInfo.Success)
            {
                if (!vpnInfo.Value.Vpn.Status.Equals(UserStatusVpnAccess))
                {
                    return ApiResponseResult<VpnInfoResponse>.Fail(vpnInfo.StatusCode, "User has no vpn access.");
                }

                _userStorage.StoreVpnInfo(vpnInfo.Value);
            }

            return vpnInfo;
        }

        public async Task RefreshVpnInfo(Action<VpnInfoResponse> onSuccess)
        {
            try
            {
                ApiResponseResult<VpnInfoResponse> infoResult = await RefreshVpnInfo();
                if (infoResult.Success)
                {
                    onSuccess(infoResult.Value);
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.Error(ex.Message);
            }
        }

        public async Task<ApiResponseResult<AuthResponse>> LoginUserAsync(string username, SecureString password)
        {
            _logger?.Info("Trying to login user");
            UserLoggingIn?.Invoke(this, EventArgs.Empty);

            ApiResponseResult<AuthResponse> authResult = await AuthAsync(username, password);
            if (authResult.Failure)
            {
                return authResult;
            }

            ApiResponseResult<VpnInfoResponse> vpnInfo = await RefreshVpnInfo();
            if (vpnInfo.Success)
            {
                _userStorage.StoreVpnInfo(vpnInfo.Value);
                InvokeUserLoggedIn(false);
                return authResult;
            }

            return ApiResponseResult<AuthResponse>.Fail(vpnInfo);
        }

        public async Task<ApiResponseResult<AuthResponse>> AuthAsync(string username, SecureString password)
        {
            ApiResponseResult<AuthInfo> authInfo = await _apiClient.GetAuthInfoResponse(new AuthInfoRequestData
            {
                Username = username
            });

            if (!authInfo.Success)
            {
                return ApiResponseResult<AuthResponse>.Fail(authInfo.StatusCode, authInfo.Error);
            }

            SrpPInvoke.GoProofs proofs = SrpPInvoke.GenerateProofs(4, username, password, authInfo.Value.Salt, authInfo.Value.Modulus, authInfo.Value.ServerEphemeral);
            AuthRequestData authData = new AuthRequestData
            {
                ClientEphemeral = Convert.ToBase64String(proofs.ClientEphemeral),
                ClientProof = Convert.ToBase64String(proofs.ClientProof),
                SrpSession = authInfo.Value.SrpSession,
                TwoFactorCode = "",
                Username = username
            };

            ApiResponseResult<AuthResponse> response = await _apiClient.GetAuthResponse(authData);
            if (response.Success)
            {
                if (!Convert.ToBase64String(proofs.ExpectedServerProof).Equals(response.Value.ServerProof))
                {
                    return ApiResponseResult<AuthResponse>.Fail(0, "Invalid server proof.");
                }

                _userStorage.SaveUsername(username);
                _tokenStorage.Uid = response.Value.Uid;
                _tokenStorage.AccessToken = response.Value.AccessToken;
                _tokenStorage.RefreshToken = response.Value.RefreshToken;
            }

            return response;
        }

        public void InvokeAutoLoginEvent()
        {
            InvokeUserLoggedIn(true);
        }

        private void InvokeUserLoggedIn(bool autoLogin)
        {
            LoggedIn = true;
            UserLoggedIn?.Invoke(this, new UserLoggedInEventArgs(autoLogin));
        }

        public void Logout()
        {
            if (!LoggedIn)
                return;

            LoggedIn = false;
            UserLoggedOut?.Invoke(this, EventArgs.Empty);

            SendLogoutRequest();

            _tokenStorage.AccessToken = string.Empty;
        }

        private async void SendLogoutRequest()
        {
            try
            {
                await _apiClient.GetLogoutResponse();
            }
            catch (HttpRequestException e)
            {
                _logger.Error(e.Message);
            }
        }
    }
}
