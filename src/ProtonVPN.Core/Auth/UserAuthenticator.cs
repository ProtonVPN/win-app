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
using System.Security;
using System.Threading.Tasks;
using ProtonVPN.Api.Contracts;
using ProtonVPN.Api.Contracts.Auth;
using ProtonVPN.Api.Contracts.Common;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.UserLogs;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Srp;

namespace ProtonVPN.Core.Auth
{
    public class UserAuthenticator : IUserAuthenticator
    {
        private readonly IApiClient _apiClient;
        private readonly ILogger _logger;
        private readonly IUserStorage _userStorage;
        private readonly IAppSettings _appSettings;
        private readonly IAuthCertificateManager _authCertificateManager;

        private string _username;
        private AuthResponse _authResponse;

        public UserAuthenticator(IApiClient apiClient,
            ILogger logger,
            IUserStorage userStorage,
            IAppSettings appSettings,
            IAuthCertificateManager authCertificateManager)
        {
            _appSettings = appSettings;
            _authCertificateManager = authCertificateManager;
            _apiClient = apiClient;
            _logger = logger;
            _userStorage = userStorage;
        }

        public bool IsLoggedIn { get; private set; }

        public event EventHandler<EventArgs> UserLoggedOut;
        public event EventHandler<UserLoggedInEventArgs> UserLoggedIn;
        public event EventHandler<EventArgs> UserLoggingIn;

        public async Task<AuthResult> LoginUserAsync(string username, SecureString password)
        {
            _logger?.Info<UserLog>("Trying to login user");
            InvokeLoggingInEvent();

            AuthResult authResult = await AuthAsync(username, password);
            return authResult.Failure ? authResult : await RefreshVpnInfoAndInvokeLoginAsync();
        }

        private async Task<AuthResult> RefreshVpnInfoAndInvokeLoginAsync()
        {
            ApiResponseResult<VpnInfoWrapperResponse> vpnInfoResult = await RefreshVpnInfoAsync();
            if (vpnInfoResult.Failure)
            {
                return AuthResult.Fail(vpnInfoResult);
            }

            _userStorage.StoreVpnInfo(vpnInfoResult.Value);
            await InvokeUserLoggedInAsync(false);

            return AuthResult.Ok();
        }

        public async Task<AuthResult> AuthAsync(string username, SecureString password)
        {
            ApiResponseResult<AuthInfoResponse> authInfoResponse =
                await _apiClient.GetAuthInfoResponse(new AuthInfoRequest { Username = username });
            if (!authInfoResponse.Success)
            {
                return AuthResult.Fail(authInfoResponse);
            }

            if (authInfoResponse.Value.Salt.IsNullOrEmpty())
            {
                return AuthResult.Fail("Incorrect login credentials. Please try again");
            }

            try
            {
                SrpPInvoke.GoProofs proofs = SrpPInvoke.GenerateProofs(4, username, password, authInfoResponse.Value.Salt,
                    authInfoResponse.Value.Modulus, authInfoResponse.Value.ServerEphemeral);

                AuthRequest authRequest = GetAuthRequestData(proofs, authInfoResponse.Value.SrpSession, username);
                ApiResponseResult<AuthResponse> response = await _apiClient.GetAuthResponse(authRequest);
                if (response.Failure)
                {
                    return AuthResult.Fail(response);
                }

                if (!Convert.ToBase64String(proofs.ExpectedServerProof).Equals(response.Value.ServerProof))
                {
                    return AuthResult.Fail(AuthError.InvalidServerProof);
                }

                if ((response.Value.TwoFactor.Enabled & 1) != 0)
                {
                    _username = username;
                    _authResponse = response.Value;
                    return AuthResult.Fail(AuthError.TwoFactorRequired);
                }

                StoreUserDetails(username, response.Value);
                return AuthResult.Ok();
            }
            catch (TypeInitializationException e) when (e.InnerException is DllNotFoundException)
            {
                return AuthResult.Fail(AuthError.MissingGoSrpDll);
            }
        }

        public async Task<AuthResult> SendTwoFactorCodeAsync(string code)
        {
            InvokeLoggingInEvent();

            TwoFactorRequest request = new() { TwoFactorCode = code };
            ApiResponseResult<BaseResponse> response =
                await _apiClient.GetTwoFactorAuthResponse(request, _authResponse.AccessToken, _authResponse.Uid);

            if (response.Failure)
            {
                return AuthResult.Fail(response.Value.Code == ResponseCodes.IncorrectLoginCredentials
                    ? AuthError.IncorrectTwoFactorCode
                    : AuthError.TwoFactorAuthFailed);
            }

            StoreUserDetails(_username, _authResponse);

            return await RefreshVpnInfoAndInvokeLoginAsync();
        }

        private void StoreUserDetails(string username, AuthResponse authResponse)
        {
            _userStorage.SaveUsername(username);
            _appSettings.Uid = authResponse.Uid;
            _appSettings.AccessToken = authResponse.AccessToken;
            _appSettings.RefreshToken = authResponse.RefreshToken;
        }

        private AuthRequest GetAuthRequestData(SrpPInvoke.GoProofs proofs, string srpSession, string username)
        {
            return new AuthRequest
            {
                ClientEphemeral = Convert.ToBase64String(proofs.ClientEphemeral),
                ClientProof = Convert.ToBase64String(proofs.ClientProof),
                SrpSession = srpSession,
                Username = username
            };
        }

        private void InvokeLoggingInEvent()
        {
            UserLoggingIn?.Invoke(this, EventArgs.Empty);
        }

        public async Task<ApiResponseResult<VpnInfoWrapperResponse>> RefreshVpnInfoAsync()
        {
            ApiResponseResult<VpnInfoWrapperResponse> vpnInfo = await _apiClient.GetVpnInfoResponse();
            if (vpnInfo.Success)
            {
                _userStorage.StoreVpnInfo(vpnInfo.Value);
            }
            else if (vpnInfo.Value.Code == ResponseCodes.NoVpnConnectionsAssigned)
            {
                ClearAuthData();
            }

            return vpnInfo;
        }

        private void ClearAuthData()
        {
            _appSettings.Uid = string.Empty;
            _appSettings.AccessToken = string.Empty;
            _appSettings.RefreshToken = string.Empty;
            _userStorage.SaveUsername(string.Empty);
        }

        public async Task InvokeAutoLoginEventAsync()
        {
            await InvokeUserLoggedInAsync(true);
        }

        private async Task InvokeUserLoggedInAsync(bool isAutoLogin)
        {
            await RequestNewKeysAndCertificateOnLoginAsync(isAutoLogin);
            IsLoggedIn = true;
            UserLoggedIn?.Invoke(this, new UserLoggedInEventArgs(isAutoLogin));
        }

        private async Task RequestNewKeysAndCertificateOnLoginAsync(bool isAutoLogin)
        {
            if (isAutoLogin)
            {
                await _authCertificateManager.RequestNewCertificateAsync();
            }
            else
            {
                await _authCertificateManager.ForceRequestNewKeyPairAndCertificateAsync();
            }
        }

        public async Task LogoutAsync()
        {
            if (IsLoggedIn)
            {
                IsLoggedIn = false;
                UserLoggedOut?.Invoke(this, EventArgs.Empty);

                await SendLogoutRequestAsync();

                _authCertificateManager.DeleteKeyPairAndCertificate();
                _appSettings.AccessToken = string.Empty;
            }
        }

        private async Task SendLogoutRequestAsync()
        {
            try
            {
                await _apiClient.GetLogoutResponse();
            }
            catch (Exception ex)
            {
                _logger.Error<UserLog>("An error occurred when sending a logout request.", ex);
            }
        }
    }
}