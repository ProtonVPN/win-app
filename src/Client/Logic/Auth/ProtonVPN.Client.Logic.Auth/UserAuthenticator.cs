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
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.GuestHole;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Migrations;
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
    private readonly IConnectionManager _connectionManager;
    private readonly IServersLoader _serversLoader;
    private readonly IServersUpdater _serversUpdater;
    private readonly IUserSettingsMigrator _userSettingsMigrator;
    private AuthResponse _authResponse;

    public bool IsLoggingIn { get; private set; }
    public bool IsLoggedIn { get; private set; }
    public bool? IsAutoLogin { get; private set; }

    public UserAuthenticator(
        ILogger logger,
        IApiClient apiClient,
        IAuthCertificateManager authCertificateManager,
        ISettings settings,
        IEventMessageSender eventMessageSender,
        IGuestHoleActionExecutor guestHoleActionExecutor,
        ITokenClient tokenClient,
        IConnectionManager connectionManager,
        IServersLoader serversLoader,
        IServersUpdater serversUpdater,
        IUserSettingsMigrator userSettingsMigrator)
    {
        _logger = logger;
        _apiClient = apiClient;
        _authCertificateManager = authCertificateManager;
        _settings = settings;
        _eventMessageSender = eventMessageSender;
        _guestHoleActionExecutor = guestHoleActionExecutor;
        _tokenClient = tokenClient;
        _connectionManager = connectionManager;
        _serversLoader = serversLoader;
        _serversUpdater = serversUpdater;
        _userSettingsMigrator = userSettingsMigrator;

        _tokenClient.RefreshTokenExpired += OnTokenExpiredAsync;
    }

    public async Task CreateUnauthSessionAsync()
    {
        try
        {
            _logger.Info<UserLog>("Requesting unauth session to initiate login process.");

            ApiResponseResult<UnauthSessionResponse> unauthSessionResponse = await _apiClient.PostUnauthSessionAsync();

            if (unauthSessionResponse.Success)
            {
                SaveUnauthSessionDetails(unauthSessionResponse.Value);
            }
        }
        catch (Exception ex)
        {
            _logger.Error<ApiErrorLog>("An error occurred when requesting a new unauth session.", ex);
        }
    }

    public async Task<AuthResult> LoginUserAsync(string username, SecureString password)
    {
        ClearAuthSessionDetails();
        try
        {
            return await AuthAsync(username, password);
        }
        catch
        {
            AuthResult? authResult = null;
            Result guestHoleResult = await _guestHoleActionExecutor.ExecuteAsync(async () =>
            {
                authResult = await AuthAsync(username, password);
            });

            return guestHoleResult.Success && authResult != null
                ? AuthResult.Ok()
                : AuthResult.Fail(AuthError.GuestHoleFailed);
        }
    }

    public async Task<SsoAuthResult> StartSsoAuthAsync(string username)
    {
        _logger.Info<UserLog>("Trying to login user with SSO");
        ClearAuthSessionDetails();

        if (!IsUnauthSessionCreated())
        {
            await CreateUnauthSessionAsync();
        }

        ApiResponseResult<AuthInfoResponse> authInfoResponse =
            await _apiClient.GetAuthInfoResponse(new AuthInfoRequest { Username = username, Intent = SSO_LOGIN_INTENT });
        if (!authInfoResponse.Success || string.IsNullOrEmpty(authInfoResponse.Value?.SsoChallengeToken))
        {
            _logger.Error<UserLog>("Failed to login with SSO.");
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

        SaveAuthSessionDetails(authResponse.Value);

        return await CompleteLoginAsync(false);
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

            SaveAuthSessionDetails(response.Value);

            return await CompleteLoginAsync(false);
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

        SaveAuthSessionDetails(_authResponse);

        return await CompleteLoginAsync(false);
    }

    public async Task<AuthResult> AutoLoginUserAsync()
    {
        return await CompleteLoginAsync(true);
    }

    public async Task LogoutAsync(LogoutReason reason)
    {
        if (IsLoggedIn || IsLoggingIn)
        {
            _eventMessageSender.Send(new LoggingOutMessage() { Reason = reason });

            if (!_connectionManager.IsDisconnected)
            {
                await _connectionManager.DisconnectAsync();
            }

            _authCertificateManager.DeleteKeyPairAndCertificate();

            await SendLogoutRequestAsync();

            await CreateUnauthSessionAsync();

            ClearAuthSessionDetails();

            IsLoggingIn = false;
            IsLoggedIn = false;
            IsAutoLogin = null;

            _eventMessageSender.Send(new LoggedOutMessage() { Reason = reason });
        }
    }

    private async void OnTokenExpiredAsync(object? sender, EventArgs e)
    {
        if (!IsLoggedIn)
        {
            await CreateUnauthSessionAsync();
            return;
        }

        await LogoutAsync(LogoutReason.SessionExpired);
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

    private void ClearAuthSessionDetails()
    {
        _settings.UserId = null;
        _settings.UniqueSessionId = null;
        _settings.AccessToken = null;
        _settings.RefreshToken = null;
    }

    private void SaveAuthSessionDetails(AuthResponse authResponse)
    {
        _settings.UserId = authResponse.UserId;
        _settings.AccessToken = authResponse.AccessToken;
        _settings.UniqueSessionId = authResponse.UniqueSessionId;
        _settings.RefreshToken = authResponse.RefreshToken;
    }

    private async Task<AuthResult> CompleteLoginAsync(bool isAutoLogin)
    {
        try
        {
            IsLoggingIn = true;
            IsLoggedIn = false;
            _eventMessageSender.Send(new LoggingInMessage());

            if (!HasAuthenticatedSessionData())
            {
                ClearAuthSessionDetails();
                await LogoutAsync(LogoutReason.SessionExpired);
                return AuthResult.Fail(AuthError.GetSessionDetailsFailed);
            }

            Task<ApiResponseResult<UsersResponse>> getUserTask = _apiClient.GetUserAsync();
            Task<ApiResponseResult<VpnInfoWrapperResponse>> getVpnInfoTask = _apiClient.GetVpnInfoResponse();
            await Task.WhenAll(getUserTask, getVpnInfoTask);

            if (getUserTask.Result.Success)
            {
                // After migration from previous version, there is no User ID. Global Settings should be set before User Settings.
                if (string.IsNullOrWhiteSpace(_settings.UserId))
                {
                    _settings.UserId = getUserTask.Result.Value.User.UserId;
                }

                _settings.Username = getUserTask.Result.Value.User.GetUsername();
                _settings.UserDisplayName = getUserTask.Result.Value.User.GetDisplayName();
            }

            if (string.IsNullOrWhiteSpace(_settings.UserId))
            {
                await LogoutAsync(LogoutReason.SessionExpired);
                return AuthResult.Fail(getUserTask.Result);
            }

            if (getVpnInfoTask.Result.Success)
            {
                _settings.VpnPlanTitle = getVpnInfoTask.Result.Value.Vpn.PlanTitle;
                _settings.IsPaid = getVpnInfoTask.Result.Value.Vpn.MaxTier > 0;
                _settings.MaxTier = getVpnInfoTask.Result.Value.Vpn.MaxTier;
            }
            else if (getVpnInfoTask.Result.Value.Code == ResponseCodes.NoVpnConnectionsAssigned)
            {
                await LogoutAsync(LogoutReason.NoVpnConnectionsAssigned);
                return AuthResult.Fail(getVpnInfoTask.Result);
            }
        }
        catch (Exception e)
        {
            _logger.Error<AppLog>("An unexpected exception was thrown when updating the user info.", e);
        }

        ClearUnauthSessionDetails();

        IsLoggedIn = true;
        IsLoggingIn = false;
        IsAutoLogin = isAutoLogin;

        await MigrateUserSettingsAsync();

        _eventMessageSender.Send(new LoggedInMessage { IsAutoLogin = isAutoLogin });

        await RequestNewKeysAndCertificateOnLoginAsync(isAutoLogin);

        return AuthResult.Ok();
    }

    public bool HasAuthenticatedSessionData()
    {
        return !string.IsNullOrWhiteSpace(_settings.AccessToken) &&
               !string.IsNullOrWhiteSpace(_settings.RefreshToken) &&
               !string.IsNullOrWhiteSpace(_settings.UniqueSessionId);
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

    private async Task MigrateUserSettingsAsync()
    {
        _serversUpdater.LoadFromFileIfEmpty();

        if (!_serversLoader.GetServers().Any())
        {
            _logger.Info<AppLog>("Fetching servers as the user has none.");
            await _serversUpdater.UpdateAsync();
        }

        await _userSettingsMigrator.MigrateAsync();
    }

    private void SaveUnauthSessionDetails(UnauthSessionResponse response)
    {
        _settings.UnauthUniqueSessionId = response.UniqueSessionId;
        _settings.UnauthAccessToken = response.AccessToken;
        _settings.UnauthRefreshToken = response.RefreshToken;
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