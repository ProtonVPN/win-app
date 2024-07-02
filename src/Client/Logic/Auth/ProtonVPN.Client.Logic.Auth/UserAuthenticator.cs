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
using ProtonVPN.Client.Logic.Servers.Contracts.Updaters;
using ProtonVPN.Client.Logic.Users.Contracts;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Migrations;
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
    private readonly IConnectionCertificateManager _connectionCertificateManager;
    private readonly ISettings _settings;
    private readonly IEventMessageSender _eventMessageSender;
    private readonly IGuestHoleManager _guestHoleManager;
    private readonly ITokenClient _tokenClient;
    private readonly IConnectionManager _connectionManager;
    private readonly IServersUpdater _serversUpdater;
    private readonly IUserSettingsMigrator _userSettingsMigrator;
    private readonly IVpnPlanUpdater _vpnPlanUpdater;
    private AuthResponse _authResponse;

    public AuthenticationStatus AuthenticationStatus { get; private set; }

    public bool IsLoggedIn => AuthenticationStatus == AuthenticationStatus.LoggedIn;
    public bool? IsAutoLogin { get; private set; }

    public UserAuthenticator(
        ILogger logger,
        IApiClient apiClient,
        IConnectionCertificateManager connectionCertificateManager,
        ISettings settings,
        IEventMessageSender eventMessageSender,
        IGuestHoleManager guestHoleManager,
        ITokenClient tokenClient,
        IConnectionManager connectionManager,
        IServersUpdater serversUpdater,
        IUserSettingsMigrator userSettingsMigrator,
        IVpnPlanUpdater vpnPlanUpdater)
    {
        _logger = logger;
        _apiClient = apiClient;
        _connectionCertificateManager = connectionCertificateManager;
        _settings = settings;
        _eventMessageSender = eventMessageSender;
        _guestHoleManager = guestHoleManager;
        _tokenClient = tokenClient;
        _connectionManager = connectionManager;
        _serversUpdater = serversUpdater;
        _userSettingsMigrator = userSettingsMigrator;
        _vpnPlanUpdater = vpnPlanUpdater;

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
            AuthResult result = await AuthAsync(username, password);
            if (result.Failure)
            {
                return result;
            }

            return await CompleteLoginAsync(isAutoLogin: false, isToSendLoggedInEvent: true);
        }
        catch
        {
            return await HandleLoginOverGuestHoleAsync(username, password);
        }
    }

    private async Task<AuthResult> HandleLoginOverGuestHoleAsync(string username, SecureString password)
    {
        AuthResult? authResult = await _guestHoleManager.ExecuteAsync<AuthResult>(async () =>
        {
            AuthResult authResult = await AuthAsync(username, password);
            if (authResult.Success)
            {
                authResult = await CompleteLoginAsync(isAutoLogin: false, isToSendLoggedInEvent: false);
                if (authResult.Success)
                {
                    await _connectionCertificateManager.ForceRequestNewCertificateAsync();
                    await _serversUpdater.ForceFullUpdateIfEmptyAsync();
                }
            }

            if (authResult.Success || (authResult.Failure && authResult.Value != AuthError.TwoFactorRequired))
            {
                await _guestHoleManager.DisconnectAsync();
            }

            return authResult;
        });

        if (authResult != null)
        {
            if (authResult.Success)
            {
                SetAuthenticationStatus(AuthenticationStatus.LoggedIn);
                return authResult;
            }

            if (authResult.Value == AuthError.TwoFactorRequired)
            {
                return AuthResult.Fail(AuthError.TwoFactorRequired);
            }

            return authResult;
        }

        return AuthResult.Fail(AuthError.GuestHoleFailed);
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

        return await CompleteLoginAsync(isAutoLogin: false, isToSendLoggedInEvent: true);
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

            return AuthResult.Ok();
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

        return await CompleteLoginAsync(isAutoLogin: false, isToSendLoggedInEvent: true);
    }

    public async Task<AuthResult> AutoLoginUserAsync()
    {
        return await CompleteLoginAsync(isAutoLogin: true, isToSendLoggedInEvent: true);
    }

    public async Task LogoutAsync(LogoutReason reason)
    {
        if (AuthenticationStatus is AuthenticationStatus.LoggedIn or AuthenticationStatus.LoggingIn)
        {
            SetAuthenticationStatus(AuthenticationStatus.LoggingOut);

            if (!_connectionManager.IsDisconnected)
            {
                await _connectionManager.DisconnectAsync();
            }

            _connectionCertificateManager.DeleteKeyPairAndCertificate();

            await SendLogoutRequestAsync();

            await CreateUnauthSessionAsync();

            ClearAuthSessionDetails();

            IsAutoLogin = null;

            SetAuthenticationStatus(AuthenticationStatus.LoggedOut, reason);
        }
    }

    public bool HasAuthenticatedSessionData()
    {
        return !string.IsNullOrWhiteSpace(_settings.AccessToken) &&
               !string.IsNullOrWhiteSpace(_settings.RefreshToken) &&
               !string.IsNullOrWhiteSpace(_settings.UniqueSessionId);
    }

    private async void OnTokenExpiredAsync(object? sender, EventArgs e)
    {
        if (AuthenticationStatus is AuthenticationStatus.LoggingOut)
        {
            return;
        }

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

    private async Task<AuthResult> CompleteLoginAsync(bool isAutoLogin, bool isToSendLoggedInEvent)
    {
        bool hasPlanChanged = false;
        try
        {
            SetAuthenticationStatus(AuthenticationStatus.LoggingIn);

            if (!HasAuthenticatedSessionData())
            {
                ClearAuthSessionDetails();
                await LogoutAsync(LogoutReason.SessionExpired);
                return AuthResult.Fail(AuthError.GetSessionDetailsFailed);
            }

            ApiResponseResult<UsersResponse> usersResponse = await _apiClient.GetUserAsync();

            if (usersResponse.Success)
            {
                // After migration from previous version, there is no User ID. Global Settings should be set before User Settings.
                if (string.IsNullOrWhiteSpace(_settings.UserId))
                {
                    _settings.UserId = usersResponse.Value.User.UserId;
                }

                _settings.Username = usersResponse.Value.User.GetUsername();
                _settings.UserDisplayName = usersResponse.Value.User.GetDisplayName();
            }

            if (string.IsNullOrWhiteSpace(_settings.UserId))
            {
                await LogoutAsync(LogoutReason.SessionExpired);
                return AuthResult.Fail(usersResponse);
            }

            VpnPlanChangeResult vpnPlanChangeResult = await _vpnPlanUpdater.ForceUpdateAsync();

            if (vpnPlanChangeResult.ApiResponse is not null &&
                vpnPlanChangeResult.ApiResponse.Failure &&
                vpnPlanChangeResult.ApiResponse.Value.Code == ResponseCodes.NoVpnConnectionsAssigned)
            {
                await LogoutAsync(LogoutReason.NoVpnConnectionsAssigned);
                return AuthResult.Fail(vpnPlanChangeResult.ApiResponse);
            }

            hasPlanChanged = vpnPlanChangeResult.PlanChangeMessage?.HasChanged() ?? false;
        }
        catch (Exception e)
        {
            _logger.Error<AppLog>("An unexpected exception was thrown when updating the user info.", e);
        }

        ClearUnauthSessionDetails();

        IsAutoLogin = isAutoLogin;

        await MigrateUserSettingsAsync();

        DeleteKeyPairIfNotAutoLogin(isAutoLogin, _guestHoleManager.IsActive);

        if (hasPlanChanged && !_guestHoleManager.IsActive)
        {
            await _connectionCertificateManager.ForceRequestNewCertificateAsync();
        }

        if (isToSendLoggedInEvent)
        {
            SetAuthenticationStatus(AuthenticationStatus.LoggedIn);
        }

        return AuthResult.Ok();
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

    private void SetAuthenticationStatus(AuthenticationStatus status, LogoutReason? logoutReason = null)
    {
        AuthenticationStatus = status;

        _eventMessageSender.Send(new AuthenticationStatusChanged(status));

        switch (status)
        {
            case AuthenticationStatus.LoggedIn:
                _eventMessageSender.Send(new LoggedInMessage { IsAutoLogin = IsAutoLogin ?? false });
                break;

            case AuthenticationStatus.LoggedOut:
                _eventMessageSender.Send(new LoggedOutMessage { Reason = logoutReason ?? LogoutReason.UserAction });
                break;

            case AuthenticationStatus.LoggingIn:
                _eventMessageSender.Send(new LoggingInMessage());
                break;

            case AuthenticationStatus.LoggingOut:
                _eventMessageSender.Send(new LoggingOutMessage());
                break;
        }
    }

    private async Task MigrateUserSettingsAsync()
    {
        await _serversUpdater.ForceFullUpdateIfEmptyAsync();
        _userSettingsMigrator.Migrate();
    }

    private void SaveUnauthSessionDetails(UnauthSessionResponse response)
    {
        _settings.UnauthUniqueSessionId = response.UniqueSessionId;
        _settings.UnauthAccessToken = response.AccessToken;
        _settings.UnauthRefreshToken = response.RefreshToken;
    }

    private void DeleteKeyPairIfNotAutoLogin(bool isAutoLogin, bool isGuestHoleActive)
    {
        if (!isAutoLogin && !isGuestHoleActive)
        {
            _connectionCertificateManager.DeleteKeyPairAndCertificate();
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