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
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.GuestHole;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Common.Legacy.Abstract;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.UserLogs;

namespace ProtonVPN.Client.Logic.Auth;

public class UserAuthenticator : IUserAuthenticator
{
    private readonly ILogger _logger;
    private readonly IApiClient _apiClient;
    private readonly IAuthCertificateManager _authCertificateManager;
    private readonly ISettings _settings;
    private readonly IEventMessageSender _eventMessageSender;
    private readonly IGuestHoleActionExecutor _guestHoleActionExecutor;

    private AuthResponse _authResponse;
    private string _username;

    public UserAuthenticator(ILogger logger, IApiClient apiClient, IAuthCertificateManager authCertificateManager,
        ISettings settings, IEventMessageSender eventMessageSender, IGuestHoleActionExecutor guestHoleActionExecutor)
    {
        _logger = logger;
        _apiClient = apiClient;
        _authCertificateManager = authCertificateManager;
        _settings = settings;
        _eventMessageSender = eventMessageSender;
        _guestHoleActionExecutor = guestHoleActionExecutor;
    }

    public bool IsLoggedIn { get; private set; }

    public async Task<AuthResult> LoginUserAsync(string username, SecureString password)
    {
        try
        {
            AuthResult result = await AuthAsync(username, password);
            return result.Failure ? result : await RefreshVpnInfoAndInvokeLoginAsync();
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

    private async Task<AuthResult> RefreshVpnInfoAndInvokeLoginAsync()
    {
        ApiResponseResult<VpnInfoWrapperResponse> vpnInfoResult = await RefreshVpnInfoAsync();
        if (vpnInfoResult.Failure)
        {
            return AuthResult.Fail(vpnInfoResult);
        }

        SaveUserInfo(vpnInfoResult.Value);

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
                _username = username;
                _authResponse = response.Value;
                return AuthResult.Fail(AuthError.TwoFactorRequired);
            }

            SaveAuthDetails(username, response.Value);

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

        SaveAuthDetails(_username, _authResponse);

        return await RefreshVpnInfoAndInvokeLoginAsync();
    }

    private void SaveAuthDetails(string username, AuthResponse authResponse)
    {
        _settings.Username = username;
        _settings.AccessToken = authResponse.AccessToken;
        _settings.UniqueSessionId = authResponse.UniqueSessionId;
        _settings.RefreshToken = authResponse.RefreshToken;
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

    public async Task<ApiResponseResult<VpnInfoWrapperResponse>> RefreshVpnInfoAsync()
    {
        ApiResponseResult<VpnInfoWrapperResponse> vpnInfo = await _apiClient.GetVpnInfoResponse();
        if (vpnInfo.Success)
        {
            SaveUserInfo(vpnInfo.Value);
        }
        else if (vpnInfo.Value.Code == ResponseCodes.NoVpnConnectionsAssigned)
        {
            ClearUserData();
        }

        return vpnInfo;
    }

    private void SaveUserInfo(VpnInfoWrapperResponse response)
    {
        _settings.VpnPlanTitle = response.Vpn.PlanTitle;
    }

    private void ClearUserData()
    {
        _settings.UniqueSessionId = null;
        _settings.AccessToken = null;
        _settings.RefreshToken = null;
        _settings.VpnPlanTitle = null;
        
        // The username must be the last setting to clear.
        // After it, we no longer have a reference to a user settings file to delete or set user settings.
        _settings.Username = null;
    }

    public async Task InvokeAutoLoginEventAsync()
    {
        await InvokeUserLoggedInAsync(true);
    }

    private async Task InvokeUserLoggedInAsync(bool isAutoLogin)
    {
        _eventMessageSender.Send(new LoginSuccessMessage());
        await RequestNewKeysAndCertificateOnLoginAsync(isAutoLogin);
        IsLoggedIn = true;
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

            _eventMessageSender.Send(new LogoutMessage());
            await SendLogoutRequestAsync();

            _authCertificateManager.DeleteKeyPairAndCertificate();
            ClearUserData();
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