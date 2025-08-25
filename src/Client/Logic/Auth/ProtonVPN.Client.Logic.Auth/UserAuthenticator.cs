/*
 * Copyright (c) 2025 Proton AG
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
using ProtonVPN.Api.Contracts.Users;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Enums;
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Client.Logic.Auth.Contracts.Models;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.GuestHole;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Logic.Users.Contracts;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Migrations;
using ProtonVPN.Client.Settings.Contracts.Observers;
using ProtonVPN.Common.Core.Extensions;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;
using ProtonVPN.Logging.Contracts.Events.UserLogs;
using ProtonVPN.StatisticalEvents.Contracts.Dimensions;

namespace ProtonVPN.Client.Logic.Auth;

public class UserAuthenticator : IUserAuthenticator, IEventMessageReceiver<ClientOutdatedMessage>
{
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
    private readonly IUnauthSessionManager _unauthSessionManager;
    private readonly ISrpAuthenticator _srpAuthenticator;
    private readonly ISsoAuthenticator _ssoAuthenticator;
    private readonly IFeatureFlagsObserver _featureFlagsObserver;
    private readonly IClientConfigObserver _clientConfigObserver;

    private CancellationTokenSource _cts = new();

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
        IVpnPlanUpdater vpnPlanUpdater,
        IUnauthSessionManager unauthSessionManager,
        ISrpAuthenticator srpAuthenticator,
        ISsoAuthenticator ssoAuthenticator,
        IFeatureFlagsObserver featureFlagsObserver,
        IClientConfigObserver clientConfigObserver)
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
        _unauthSessionManager = unauthSessionManager;
        _srpAuthenticator = srpAuthenticator;
        _ssoAuthenticator = ssoAuthenticator;
        _featureFlagsObserver = featureFlagsObserver;
        _clientConfigObserver = clientConfigObserver;

        _tokenClient.RefreshTokenExpired += OnTokenExpiredAsync;
    }

    public async Task<AuthResult> LoginUserAsync(string username, SecureString password)
    {
        SetAuthenticationStatus(AuthenticationStatus.LoggingIn);
        ClearAuthSessionDetails();
        ResetCancellationTokenIfCancelled();

        await _unauthSessionManager.CreateIfDoesNotExistAsync(_cts.Token);

        try
        {
            AuthResult result = await _srpAuthenticator.LoginUserAsync(username, password, _cts.Token);
            if (result.Failure)
            {
                if (result.Value != AuthError.TwoFactorRequired)
                {
                    SetAuthenticationStatus(AuthenticationStatus.LoggedOut);
                }

                return result;
            }

            return await CompleteLoginAsync(isAutoLogin: false, isToSendLoggedInEvent: true);
        }
        catch (Exception)
        {
            if (_cts.IsCancellationRequested)
            {
                HandleAuthCancellation();

                return AuthResult.Fail(AuthError.None);
            }

            return await HandleLoginOverGuestHoleAsync(username, password);
        }
    }

    private async Task<AuthResult> HandleLoginOverGuestHoleAsync(string username, SecureString password)
    {
        try
        {
            AuthResult? authResult = await _guestHoleManager.ExecuteAsync<AuthResult>(async () =>
            {
                AuthResult authResult = await _srpAuthenticator.LoginUserAsync(username, password, _cts.Token);
                if (authResult.Success)
                {
                    authResult = await CompleteLoginAsync(isAutoLogin: false, isToSendLoggedInEvent: false);
                }

                if (authResult.Success || (authResult.Failure && authResult.Value != AuthError.TwoFactorRequired))
                {
                    await _guestHoleManager.DisconnectAsync();
                }

                return authResult;
            }, _cts.Token);

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

            SetAuthenticationStatus(AuthenticationStatus.LoggedOut);

            return AuthResult.Fail(AuthError.GuestHoleFailed);
        }
        catch (Exception) when (_cts.IsCancellationRequested)
        {
            HandleAuthCancellation();

            return AuthResult.Fail(AuthError.None);
        }
    }

    private void ResetCancellationTokenIfCancelled()
    {
        if (!_cts.IsCancellationRequested)
        {
            return;
        }

        _cts.Dispose();
        _cts = new();
    }

    public async Task<SsoAuthResult> StartSsoAuthAsync(string username)
    {
        _logger.Info<UserLog>("Trying to login user with SSO");

        SetAuthenticationStatus(AuthenticationStatus.LoggingIn);
        ClearAuthSessionDetails();
        ResetCancellationTokenIfCancelled();

        await _unauthSessionManager.CreateIfDoesNotExistAsync(_cts.Token);

        try
        {
            return await _ssoAuthenticator.StartSsoAuthAsync(username, _cts.Token);
        }
        catch (Exception) when (_cts.IsCancellationRequested)
        {
            HandleAuthCancellation();

            return SsoAuthResult.FromAuthResult(AuthResult.Fail(AuthError.None));
        }
    }

    public async Task<AuthResult> CompleteSsoAuthAsync(string ssoResponseToken)
    {
        try
        {
            AuthResult result = await _ssoAuthenticator.CompleteSsoAuthAsync(ssoResponseToken, _cts.Token);

            return result.Failure
                ? result
                : await CompleteLoginAsync(isAutoLogin: false, isToSendLoggedInEvent: true);
        }
        catch (Exception) when (_cts.IsCancellationRequested)
        {
            HandleAuthCancellation();

            return AuthResult.Fail(AuthError.None);
        }
    }

    public async Task<AuthResult> SendTwoFactorCodeAsync(string code)
    {
        SetAuthenticationStatus(AuthenticationStatus.LoggingIn);
        ResetCancellationTokenIfCancelled();

        try
        {
            AuthResult result = await _srpAuthenticator.SendTwoFactorCodeAsync(code, _cts.Token);

            return result.Failure
                ? result
                : await CompleteLoginAsync(isAutoLogin: false, isToSendLoggedInEvent: true);
        }
        catch (Exception) when (_cts.IsCancellationRequested)
        {
            HandleAuthCancellation();

            return AuthResult.Fail(AuthError.TwoFactorCancelled);
        }
    }

    private void HandleAuthCancellation()
    {
        ResetCancellationTokenIfCancelled();
        _unauthSessionManager.RecreateAsync(_cts.Token).FireAndForget();

        SetAuthenticationStatus(AuthenticationStatus.LoggedOut);
    }

    public async Task AutoLoginUserAsync()
    {
        if (HasAuthenticatedSessionData())
        {
            SetAuthenticationStatus(AuthenticationStatus.LoggingIn);
            await CompleteLoginAsync(isAutoLogin: true, isToSendLoggedInEvent: true);
        }
        else
        {
            await _unauthSessionManager.CreateIfDoesNotExistAsync(_cts.Token);
        }
    }

    public async Task LogoutAsync(LogoutReason reason)
    {
        if (AuthenticationStatus is AuthenticationStatus.LoggedIn or AuthenticationStatus.LoggingIn)
        {
            SetAuthenticationStatus(AuthenticationStatus.LoggingOut);

            if (!_connectionManager.IsDisconnected)
            {
                await _connectionManager.DisconnectAsync(VpnTriggerDimension.Signout);
            }

            _connectionCertificateManager.DeleteKeyPairAndCertificate();

            await SendLogoutRequestAsync();
            await _unauthSessionManager.RecreateAsync(CancellationToken.None);

            // First we need to clear auth session, otherwise feature flags will be
            // fetched with auth session token instead of unauth
            ClearAuthSessionDetails();
            await _featureFlagsObserver.UpdateAsync(CancellationToken.None);

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
            await _unauthSessionManager.CreateIfDoesNotExistAsync(CancellationToken.None);
            return;
        }

        await LogoutAsync(LogoutReason.SessionExpired);
    }

    private void ClearAuthSessionDetails()
    {
        _settings.UserId = null;
        _settings.UniqueSessionId = null;
        _settings.AccessToken = null;
        _settings.RefreshToken = null;
    }

    private async Task<AuthResult> CompleteLoginAsync(bool isAutoLogin, bool isToSendLoggedInEvent)
    {
        IsAutoLogin = isAutoLogin;

        bool hasPlanChanged = false;
        try
        {
            if (!HasAuthenticatedSessionData())
            {
                ClearAuthSessionDetails();
                await LogoutAsync(LogoutReason.SessionExpired);
                return AuthResult.Fail(AuthError.GetSessionDetailsFailed);
            }

            Task<ApiResponseResult<UsersResponse>> usersResponseTask = GetUserAsync();

            if (string.IsNullOrWhiteSpace(_settings.UserId))
            {
                ApiResponseResult<UsersResponse> usersResponse = await usersResponseTask;

                if (string.IsNullOrWhiteSpace(_settings.UserId))
                {
                    await LogoutAsync(LogoutReason.SessionExpired);
                    return AuthResult.Fail(usersResponse);
                }
            }

            VpnPlanChangeResult vpnPlanChangeResult = await _vpnPlanUpdater.ForceUpdateAsync(_cts.Token);

            if (vpnPlanChangeResult.ApiResponse is not null &&
                vpnPlanChangeResult.ApiResponse.Failure &&
                vpnPlanChangeResult.ApiResponse.Value.Code == ResponseCodes.NoVpnConnectionsAssigned)
            {
                await LogoutAsync(LogoutReason.NoVpnConnectionsAssigned);
                return AuthResult.Fail(vpnPlanChangeResult.ApiResponse);
            }

            hasPlanChanged = vpnPlanChangeResult.PlanChangeMessage?.HasChanged() ?? false;

            Task serversUpdateTask;
            if (hasPlanChanged)
            {
                _logger.Info<AppLog>("Reprocessing current servers and fetching new servers after VPN plan change.");
                serversUpdateTask = _serversUpdater.ForceUpdateAsync(_cts.Token);
            }
            else
            {
                serversUpdateTask = _serversUpdater.UpdateAsync(_cts.Token);
            }

            await MigrateUserSettingsAsync(usersResponseTask, serversUpdateTask);

            if (_settings.Ipv6Fragments.Count == 0)
            {
                await UpdateIpv6FragmentsAsync();
            }
        }
        catch (HttpRequestException e) when (!_cts.IsCancellationRequested)
        {
            _logger.Error<AppLog>("An Http request exception was thrown when updating the user info.", e);

            if (!_guestHoleManager.IsActive)
            {
                _logger.Info<AppLog>("Attempt to complete login through guest hole.");

                AuthResult? result = await _guestHoleManager.ExecuteAsync<AuthResult>(async () =>
                {
                    AuthResult result = await CompleteLoginAsync(isAutoLogin, isToSendLoggedInEvent);

                    await _guestHoleManager.DisconnectAsync();

                    return result;
                }, _cts.Token);

                if (result != null)
                {
                    return result;
                }
            }
        }
        catch (Exception e)
        {
            if (_cts.IsCancellationRequested)
            {
                ClearAuthSessionDetails();
                throw;
            }

            _logger.Error<AppLog>("An unexpected exception was thrown when updating the user info.", e);
        }

        _unauthSessionManager.Revoke();

        DeleteKeyPairIfNotAutoLogin(isAutoLogin, _guestHoleManager.IsActive);

        Task postAuthInitializationTask = Task.WhenAll(
            GetCertificateTask(hasPlanChanged),
            _clientConfigObserver.UpdateAsync(_cts.Token),
            _featureFlagsObserver.UpdateAsync(_cts.Token));

        if (_guestHoleManager.IsActive)
        {
            await postAuthInitializationTask;
        }
        else
        {
            postAuthInitializationTask.FireAndForget();
        }

        if (isToSendLoggedInEvent)
        {
            SetAuthenticationStatus(AuthenticationStatus.LoggedIn);
        }

        return AuthResult.Ok();
    }

    private Task GetCertificateTask(bool hasPlanChanged)
    {
        return hasPlanChanged
            ? _connectionCertificateManager.ForceRequestNewCertificateAsync(_cts.Token)
            : _connectionCertificateManager.RequestNewCertificateAsync(_cts.Token);
    }

    private async Task<ApiResponseResult<UsersResponse>> GetUserAsync()
    {
        ApiResponseResult<UsersResponse> response = await _apiClient.GetUserAsync(_cts.Token);
        if (response.Success)
        {
            // After migration from previous version, there is no User ID. Global Settings should be set before User Settings.
            if (string.IsNullOrWhiteSpace(_settings.UserId))
            {
                _settings.UserId = response.Value.User.UserId;
            }

            _settings.Username = response.Value.User.GetUsername();
            _settings.UserDisplayName = response.Value.User.GetDisplayName();
            _settings.UserCreationDateUtc = DateTimeOffset.FromUnixTimeSeconds(response.Value.User.CreateTime).UtcDateTime;
        }
        return response;
    }

    private async Task UpdateIpv6FragmentsAsync()
    {
        ApiResponseResult<Ipv6FragmentsResponse> response = await _apiClient.GetIpv6FragmentsAsync(_cts.Token);
        if (response.Success)
        {
            _settings.Ipv6Fragments = response.Value.Fragments;
        }
    }

    private async Task MigrateUserSettingsAsync(Task<ApiResponseResult<UsersResponse>> usersResponseTask, Task serversUpdateTask)
    {
        await Task.WhenAll(usersResponseTask, serversUpdateTask);
        _userSettingsMigrator.Migrate();
    }

    private void SetAuthenticationStatus(AuthenticationStatus status, LogoutReason? logoutReason = null)
    {
        string logoutReasonLogMessage = logoutReason is null ? "" : $" (Logout reason: {logoutReason})";
        _logger.Info<AppLog>($"Changing authentication status to '{status}'{logoutReasonLogMessage}.");

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

    public async void Receive(ClientOutdatedMessage message)
    {
        if (IsLoggedIn)
        {
            await LogoutAsync(LogoutReason.ClientOutdated);
        }
    }

    public void CancelAuth()
    {
        _logger.Info<UserLog>("User has cancelled the authentication process.");
        _cts.Cancel();
    }
}