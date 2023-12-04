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
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.UserLogs;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Srp;
using ProtonVPN.Logging.Contracts.Events.ApiLogs;
using ProtonVPN.Translations;
using ProtonVPN.Api.Contracts.Users;
using ProtonVPN.Logging.Contracts.Events.AppLogs;
using System.Net.Http;

namespace ProtonVPN.Core.Auth
{
    public class UserAuthenticator : IUserAuthenticator
    {
        private readonly IApiClient _apiClient;
        private readonly ILogger _logger;
        private readonly IUserStorage _userStorage;
        private readonly IAppSettings _appSettings;
        private readonly IAuthCertificateManager _authCertificateManager;
        private readonly ISsoAuthenticator _ssoAuthenticator;
        private SessionBaseResponse _currentSession;

        public UserAuthenticator(IApiClient apiClient,
            ILogger logger,
            IUserStorage userStorage,
            IAppSettings appSettings,
            IAuthCertificateManager authCertificateManager,
            ISsoAuthenticator ssoAuthenticator)
        {
            _appSettings = appSettings;
            _authCertificateManager = authCertificateManager;
            _ssoAuthenticator = ssoAuthenticator;
            _apiClient = apiClient;
            _logger = logger;
            _userStorage = userStorage;
        }

        public bool IsLoggedIn { get; private set; }

        public event EventHandler<EventArgs> UserLoggedOut;
        public event EventHandler<UserLoggedInEventArgs> UserLoggedIn;
        public event EventHandler<EventArgs> UserLoggingIn;

        private async Task<UnauthSessionResponse> CreateUnauthSessionAsync()
        {
            try
            {
                _logger?.Info<UserLog>("Requesting unauth session to initiate login process.");

                ApiResponseResult<UnauthSessionResponse> unauthSessionResponse = await _apiClient.PostUnauthSessionAsync();

                if (!unauthSessionResponse.Success)
                {
                    _logger?.Error<ApiErrorLog>($"Failed to create an unauth session. {unauthSessionResponse.Error}");
                    return null;
                }

                return unauthSessionResponse.Value;
            }
            catch (Exception ex)
            {
                _logger?.Error<ApiErrorLog>("An error occurred when requesting a new unauth session.", ex);
                throw;
            }
        }

        public async Task<AuthResult> LoginUserAsync(string username, SecureString password)
        {
            _logger?.Info<UserLog>("Trying to login user");
            InvokeLoggingInEvent();

            AuthResult authResult = await AuthAsync(username, password);
            return authResult.Failure ? authResult : await RefreshVpnInfoAndInvokeLoginAsync();
        }

        public async Task<AuthResult> SingleSignOnUserAsync(string username)
        {
            _logger?.Info<UserLog>("Trying to login user with SSO");
            InvokeLoggingInEvent();

            AuthResult authResult = await AuthSsoAsync(username);
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

        private async Task<AuthResult> AuthSsoAsync(string username)
        {
            UnauthSessionResponse unauthSession = await CreateUnauthSessionAsync();
            if (unauthSession == null)
            {
                return AuthResult.Fail(AuthError.Unknown, Translation.Get("Login_Error_msg_UnauthSessionError"));
            }

            ApiResponseResult<AuthInfoResponse> authInfoResponse =
                await _apiClient.GetAuthInfoResponse(new AuthInfoRequest { Username = username, Intent = AuthIntent.SSO.ToString() }, unauthSession.AccessToken, unauthSession.Uid);
            if (!authInfoResponse.Success)
            {
                return AuthResult.Fail(authInfoResponse);
            }

            if (string.IsNullOrEmpty(authInfoResponse.Value?.SsoChallengeToken))
            {
                return AuthResult.Fail(AuthError.Unknown, Translation.Get("Login_Error_msg_SsoRedirectionError"));
            }

            AuthSsoSessionInfo ssoSessionInfo = new()
            {
                SsoChallengeToken = authInfoResponse.Value.SsoChallengeToken,
                UnauthSessionAccessToken = unauthSession.AccessToken,
                UnauthSessionUid = unauthSession.Uid,
                Username = username
            };

            string responseToken = await _ssoAuthenticator.AuthenticateAsync(ssoSessionInfo);
            if (string.IsNullOrEmpty(responseToken))
            {
                return AuthResult.Fail(AuthError.Unknown, Translation.Get("Login_Error_msg_SsoAuthError"));
            }

            ApiResponseResult<AuthResponse> authResponse = await _apiClient.GetAuthResponse(new AuthRequest { SsoResponseToken = responseToken }, unauthSession.AccessToken, unauthSession.Uid);
            if (authResponse.Failure)
            {
                return AuthResult.Fail(authResponse);
            }

            // Persist username and replace unauth session with auth session
            _currentSession = authResponse.Value;

            await StoreSessionDetailsAsync();

            return AuthResult.Ok();
        }

        public async Task<AuthResult> AuthAsync(string username, SecureString password)
        {
            UnauthSessionResponse unauthSession = await CreateUnauthSessionAsync();
            if (unauthSession == null)
            {
                return AuthResult.Fail(AuthError.Unknown, Translation.Get("Login_Error_msg_UnauthSessionError"));
            }

            ApiResponseResult<AuthInfoResponse> authInfoResponse =
                await _apiClient.GetAuthInfoResponse(new AuthInfoRequest { Username = username, Intent = AuthIntent.Proton.ToString() }, unauthSession.AccessToken, unauthSession.Uid);
            if (!authInfoResponse.Success)
            {
                return AuthResult.Fail(authInfoResponse);
            }

            if (authInfoResponse.Value.Salt.IsNullOrEmpty())
            {
                return AuthResult.Fail(Translation.Get("Login_Error_msg_InvalidCredentials"));
            }

            try
            {
                SrpPInvoke.GoProofs proofs = SrpPInvoke.GenerateProofs(4, username, password, authInfoResponse.Value.Salt,
                    authInfoResponse.Value.Modulus, authInfoResponse.Value.ServerEphemeral);

                AuthRequest authRequest = GetAuthRequestData(proofs, authInfoResponse.Value.SrpSession, username);
                ApiResponseResult<AuthResponse> response = await _apiClient.GetAuthResponse(authRequest, unauthSession.AccessToken, unauthSession.Uid);
                if (response.Failure)
                {
                    return AuthResult.Fail(response);
                }

                if (!Convert.ToBase64String(proofs.ExpectedServerProof).Equals(response.Value.ServerProof))
                {
                    return AuthResult.Fail(AuthError.InvalidServerProof);
                }

                // Replace unauth session with auth session
                _currentSession = response.Value;

                if ((response.Value.TwoFactor.Enabled & 1) != 0)
                {
                    return AuthResult.Fail(AuthError.TwoFactorRequired);
                }

                await StoreSessionDetailsAsync();

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
                await _apiClient.GetTwoFactorAuthResponse(request, _currentSession.AccessToken, _currentSession.Uid);

            if (response.Failure)
            {
                return AuthResult.Fail(response.Value.Code == ResponseCodes.IncorrectLoginCredentials
                    ? AuthError.IncorrectTwoFactorCode
                    : AuthError.TwoFactorAuthFailed);
            }

            await StoreSessionDetailsAsync();

            return await RefreshVpnInfoAndInvokeLoginAsync();
        }

        private async Task StoreSessionDetailsAsync()
        {
            try
            {
                ApiResponseResult<UsersResponse> response = await _apiClient.GetUserAsync(_currentSession.AccessToken, _currentSession.Uid);
                if (response.Success)
                {
                    _userStorage.SaveUsername(response.Value.User.GetUsername());
                    _userStorage.StoreCreationDateUtc(DateTimeOffset.FromUnixTimeSeconds(response.Value.User.CreateTime).UtcDateTime);
                }

                _appSettings.Uid = _currentSession.Uid;
                _appSettings.AccessToken = _currentSession.AccessToken;
                _appSettings.RefreshToken = _currentSession.RefreshToken;
            }
            catch (HttpRequestException e)
            {
                _logger.Error<AppLog>("Unable to retrieve user info", e);
            }
            catch (Exception e)
            {
                _logger.Error<AppLog>("An unexpected exception was thrown when updating the user info.", e);
            }

            // User logged in, reset local session instance
            _currentSession = null;
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
                ClearSessionDetails();
            }

            return vpnInfo;
        }

        private void ClearSessionDetails()
        {
            try
            {
                _appSettings.Uid = string.Empty;
                _appSettings.AccessToken = string.Empty;
                _appSettings.RefreshToken = string.Empty;

                _userStorage.SaveUsername(string.Empty);
                _userStorage.StoreCreationDateUtc(null);
            }
            catch (Exception e)
            {
                _logger.Error<AppLog>("An unexpected exception was thrown when resetting the user info.", e);
            }

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