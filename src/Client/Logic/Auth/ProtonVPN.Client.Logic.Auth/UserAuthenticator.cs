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

using System.Security;
using ProtonVPN.Api.Contracts;
using ProtonVPN.Api.Contracts.Auth;
using ProtonVPN.Api.Contracts.Common;
using ProtonVPN.Api.Contracts.Users;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Enums;
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Client.Logic.Auth.Contracts.Models;
using ProtonVPN.Client.Logic.Connection.Contracts.GuestHole;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Common.Legacy.Abstract;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.ApiLogs;
using ProtonVPN.Logging.Contracts.Events.AppLogs;
using ProtonVPN.Logging.Contracts.Events.UserLogs;

namespace ProtonVPN.Client.Logic.Auth;

public class UserAuthenticator : IUserAuthenticator
{
    private const string SRP_LOGIN_INTENT = "Proton";
    private const string SSO_LOGIN_INTENT = "SSO";

    private readonly ILogger _logger;
    private readonly IApiClient _apiClient;
    private readonly IAuthCertificateManager _authCertificateManager;
    private readonly ISettings _settings;
    private readonly IEventMessageSender _eventMessageSender;
    private readonly IGuestHoleActionExecutor _guestHoleActionExecutor;
    private readonly ITokenClient _tokenClient;

    private AuthResponse _authResponse;

    public bool IsLoggedIn { get; private set; }

    public UserAuthenticator(
        ILogger logger,
        IApiClient apiClient,
        IAuthCertificateManager authCertificateManager,
        ISettings settings,
        IEventMessageSender eventMessageSender,
        IGuestHoleActionExecutor guestHoleActionExecutor,
        ITokenClient tokenClient)
    {
        _logger = logger;
        _apiClient = apiClient;
        _authCertificateManager = authCertificateManager;
        _settings = settings;
        _eventMessageSender = eventMessageSender;
        _guestHoleActionExecutor = guestHoleActionExecutor;
        _tokenClient = tokenClient;

        _tokenClient.RefreshTokenExpired += OnTokenExpired;
    }

    public async Task CreateUnauthSessionAsync()
    {
        try
        {
            _logger?.Info<UserLog>("Requesting unauth session to initiate login process.");

            ApiResponseResult<UnauthSessionResponse> unauthSessionResponse = await _apiClient.PostUnauthSessionAsync();

            if (unauthSessionResponse.Success)
            {
                SaveUnauthSessionDetails(unauthSessionResponse.Value);
            }
        }
        catch (Exception ex)
        {
            _logger?.Error<ApiErrorLog>("An error occurred when requesting a new unauth session.", ex);
        }
    }

    public async Task<AuthResult> LoginUserAsync(string username, SecureString password)
    {
        try
        {
            AuthResult result = await AuthAsync(username, password);
            return result.Success
                ? await RefreshVpnInfoAndInvokeLoginAsync()
                : result;
        }
        catch
        {
            AuthResult? authResult = null;
            Result guestHoleResult = await _guestHoleActionExecutor.ExecuteAsync(async () =>
            {
                authResult = await AuthAsync(username, password);
            });

            return guestHoleResult.Success && authResult != null
                ? await RefreshVpnInfoAndInvokeLoginAsync()
                : AuthResult.Fail(AuthError.GuestHoleFailed);
        }
    }

    public async Task<SsoAuthResult> StartSsoAuthAsync(string username)
    {
        _logger?.Info<UserLog>("Trying to login user with SSO");

        if (!IsUnauthSessionCreated())
        {
            await CreateUnauthSessionAsync();
        }

        ApiResponseResult<AuthInfoResponse> authInfoResponse =
            await _apiClient.GetAuthInfoResponse(new AuthInfoRequest { Username = username, Intent = SSO_LOGIN_INTENT });
        if (!authInfoResponse.Success || string.IsNullOrEmpty(authInfoResponse.Value?.SsoChallengeToken))
        {
            _logger?.Error<UserLog>("Failed to login with SSO.");
            return SsoAuthResult.FromAuthResult(AuthResult.Fail(authInfoResponse));
        }

        return SsoAuthResult.Ok(authInfoResponse.Value.SsoChallengeToken);
    }

    public async Task<AuthResult> CompleteSsoAuthAsync(string ssoResponseToken)
    {
        if (string.IsNullOrEmpty(ssoResponseToken))
        {
            return AuthResult.Fail(AuthError.SsoAuthFailed);
        }

        ApiResponseResult<AuthResponse> authResponse = await _apiClient.GetAuthResponse(new AuthRequest { SsoResponseToken = ssoResponseToken });
        if (authResponse.Failure)
        {
            return AuthResult.Fail(authResponse);
        }

        AuthResult authResult = await SaveAuthSessionDetailsAsync(authResponse.Value);

        return authResult.Success
            ? await RefreshVpnInfoAndInvokeLoginAsync()
            : authResult;
    }

    public async Task<AuthResult> AuthAsync(string username, SecureString password)
    {
        if (!IsUnauthSessionCreated())
        {
            await CreateUnauthSessionAsync();
        }

        ApiResponseResult<AuthInfoResponse> authInfoResponse =
            await _apiClient.GetAuthInfoResponse(new AuthInfoRequest { Username = username, Intent = SRP_LOGIN_INTENT });
        if (!authInfoResponse.Success)
        {
            return AuthResult.Fail(authInfoResponse);
        }

        if (string.IsNullOrEmpty(authInfoResponse.Value.Salt))
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
                _authResponse = response.Value;
                return AuthResult.Fail(AuthError.TwoFactorRequired);
            }

            return await SaveAuthSessionDetailsAsync(response.Value);
        }
        catch (TypeInitializationException e) when (e.InnerException is DllNotFoundException)
        {
            return AuthResult.Fail(AuthError.MissingGoSrpDll);
        }
    }

    public async Task<AuthResult> SendTwoFactorCodeAsync(string code)
    {
        TwoFactorRequest request = new() { TwoFactorCode = code };
        ApiResponseResult<BaseResponse> response =
            await _apiClient.GetTwoFactorAuthResponse(request, _authResponse.AccessToken,
                _authResponse.UniqueSessionId);

        if (response.Failure)
        {
            return AuthResult.Fail(response.Value.Code == ResponseCodes.IncorrectLoginCredentials
                ? AuthError.IncorrectTwoFactorCode
                : AuthError.TwoFactorAuthFailed);
        }

        AuthResult authResult = await SaveAuthSessionDetailsAsync(_authResponse);

        return authResult.Success
            ? await RefreshVpnInfoAndInvokeLoginAsync()
            : authResult;
    }

    public async Task AutoLoginUserAsync()
    {
        _eventMessageSender.Send(new LoggingInMessage());

        await InvokeUserLoggedInAsync(true);
    }

    public async Task LogoutAsync(LogoutReason reason)
    {
        await CreateUnauthSessionAsync();

        if (IsLoggedIn)
        {
            _eventMessageSender.Send(new LoggingOutMessage() { Reason = reason });

            _authCertificateManager.DeleteKeyPairAndCertificate();

            await SendLogoutRequestAsync();

            ClearAuthSessionDetails();

            IsLoggedIn = false;

            _eventMessageSender.Send(new LoggedOutMessage() { Reason = reason });
        }
    }

    private async Task<ApiResponseResult<VpnInfoWrapperResponse>> RefreshVpnInfoAsync()
    {
        ApiResponseResult<VpnInfoWrapperResponse> vpnInfo = await _apiClient.GetVpnInfoResponse();
        if (vpnInfo.Success)
        {
            SaveUserInfo(vpnInfo.Value);
        }
        else if (vpnInfo.Value.Code == ResponseCodes.NoVpnConnectionsAssigned)
        {
            ClearAuthSessionDetails();
        }

        return vpnInfo;
    }

    private async void OnTokenExpired(object? sender, EventArgs e)
    {
        await LogoutAsync(LogoutReason.SessionExpired);
    }

    private async Task<AuthResult> RefreshVpnInfoAndInvokeLoginAsync()
    {
        _eventMessageSender.Send(new LoggingInMessage());

        ApiResponseResult<VpnInfoWrapperResponse> vpnInfoResult = await RefreshVpnInfoAsync();
        if (vpnInfoResult.Failure)
        {
            return AuthResult.Fail(vpnInfoResult);
        }

        SaveUserInfo(vpnInfoResult.Value);

        await InvokeUserLoggedInAsync(false);

        return AuthResult.Ok();
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

    private void SaveUserInfo(VpnInfoWrapperResponse response)
    {
        _settings.VpnPlanTitle = response.Vpn.PlanTitle;
    }

    private void ClearAuthSessionDetails()
    {
        _settings.VpnPlanTitle = null;

        _settings.UniqueSessionId = null;
        _settings.AccessToken = null;
        _settings.RefreshToken = null;
        _settings.Username = null;
    }

    private async Task<AuthResult> SaveAuthSessionDetailsAsync(AuthResponse authResponse)
    {
        _settings.AccessToken = authResponse.AccessToken;
        _settings.UniqueSessionId = authResponse.UniqueSessionId;
        _settings.RefreshToken = authResponse.RefreshToken;

        try
        {
            ApiResponseResult<UsersResponse> response = await _apiClient.GetUserAsync();
            if (response.Success)
            {
                _settings.Username = response.Value.User.GetUsername();
                _settings.UserDisplayName = response.Value.User.GetDisplayName();
                return AuthResult.Ok();
            }
        }
        catch (Exception e)
        {
            _logger.Error<AppLog>("An unexpected exception was thrown when updating the user info.", e);
        }

        // Failed to retrieve user, logout and clear auth session details.
        await SendLogoutRequestAsync();
        ClearAuthSessionDetails();

        return AuthResult.Fail(AuthError.GetSessionDetailsFailed);
    }

    private bool IsUnauthSessionCreated()
    {
        return _settings.UnauthUniqueSessionId != null
            && _settings.UnauthAccessToken != null
            && _settings.UnauthRefreshToken != null;
    }

    private void ClearUnauthSessionDetails()
    {
        _settings.UnauthUniqueSessionId = null;
        _settings.UnauthAccessToken = null;
        _settings.UnauthRefreshToken = null;
    }

    private void SaveUnauthSessionDetails(UnauthSessionResponse response)
    {
        _settings.UnauthUniqueSessionId = response.UniqueSessionId;
        _settings.UnauthAccessToken = response.AccessToken;
        _settings.UnauthRefreshToken = response.RefreshToken;
    }

    private async Task InvokeUserLoggedInAsync(bool isAutoLogin)
    {
        ClearUnauthSessionDetails();

        IsLoggedIn = true;

        _eventMessageSender.Send(new LoggedInMessage());

        await RequestNewKeysAndCertificateOnLoginAsync(isAutoLogin);
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