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
using ProtonVPN.Api.Contracts.Auth;
using ProtonVPN.Api.Contracts.Auth.Fido2;
using ProtonVPN.Api.Contracts.Common;
using ProtonVPN.Client.Logic.Auth.Contracts.Enums;
using ProtonVPN.Client.Logic.Auth.Contracts.Models;
using ProtonVPN.Client.Logic.Connection.Contracts.GuestHole;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.OperatingSystems.WebAuthn.Contracts;

namespace ProtonVPN.Client.Logic.Auth;

public class SrpAuthenticator : AuthenticatorBase, ISrpAuthenticator
{
    private const string SRP_LOGIN_INTENT = "Proton";

    private const int TYPE_2FA_TOTP = 1;
    private const int TYPE_2FA_FIDO2 = 2;

    private readonly IApiClient _apiClient;
    private readonly IUnauthSessionManager _unauthSessionManager;
    private readonly IWebAuthnAuthenticator _webAuthnAuthenticator;

    private AuthResponse? _authResponse;

    public bool IsTwoFactorAuthenticatorModeEnabled => _authResponse != null
                                                    && (_authResponse.TwoFactor.Enabled & TYPE_2FA_TOTP) != 0;
    public bool IsTwoFactorSecurityKeyModeEnabled => _authResponse != null
                                                  && (_authResponse.TwoFactor.Enabled & TYPE_2FA_FIDO2) != 0;

    public SrpAuthenticator(
        IApiClient apiClient,
        ISettings settings,
        IUnauthSessionManager unauthSessionManager,
        IGuestHoleManager guestHoleManager,
        IWebAuthnAuthenticator webAuthnAuthenticator) : base(settings)
    {
        _apiClient = apiClient;
        _unauthSessionManager = unauthSessionManager;
        _webAuthnAuthenticator = webAuthnAuthenticator;
    }

    public async Task<AuthResult> LoginUserAsync(string username, SecureString password, CancellationToken cancellationToken)
    {
        await _unauthSessionManager.CreateIfDoesNotExistAsync(cancellationToken);

        ApiResponseResult<AuthInfoResponse> authInfoResponse = await _apiClient.GetAuthInfoResponse(
            new AuthInfoRequest { Username = username, Intent = SRP_LOGIN_INTENT },
            cancellationToken);

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
            SrpPInvoke.GoProofs? proofs = SrpPInvoke.GenerateProofs(4, username, password, authInfoResponse.Value.Salt,
                authInfoResponse.Value.Modulus, authInfoResponse.Value.ServerEphemeral);
            if (proofs is null)
            {
                return AuthResult.Fail(AuthError.Unknown);
            }

            AuthRequest authRequest = new AuthRequest
            {
                ClientEphemeral = Convert.ToBase64String(proofs.ClientEphemeral),
                ClientProof = Convert.ToBase64String(proofs.ClientProof),
                SrpSession = authInfoResponse.Value.SrpSession,
                Username = username
            };
            ApiResponseResult<AuthResponse> response = await _apiClient.GetAuthResponse(authRequest, cancellationToken);
            if (response.Failure)
            {
                return AuthResult.Fail(response);
            }

            if (!Convert.ToBase64String(proofs.ExpectedServerProof).Equals(response.Value.ServerProof))
            {
                return AuthResult.Fail(AuthError.InvalidServerProof);
            }

            if (response.Value.TwoFactor.Enabled != 0)
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

    public async Task<AuthResult> SendTwoFactorCodeAsync(string code, CancellationToken cancellationToken)
    {
        TwoFactorRequest request = new() { TwoFactorCode = code };

        ApiResponseResult<BaseResponse> response = await _apiClient.GetTwoFactorAuthResponse(
            request, 
            _authResponse?.AccessToken ?? string.Empty,
            _authResponse?.UniqueSessionId ?? string.Empty, 
            cancellationToken);

        if (response.Failure)
        {
            return AuthResult.Fail(response.Value.Code == ResponseCodes.IncorrectLoginCredentials
                ? AuthError.IncorrectTwoFactorCode
                : AuthError.TwoFactorAuthFailed);
        }

        SaveAuthSessionDetails(_authResponse);

        return AuthResult.Ok();
    }

    public async Task<AuthResult> AuthenticateWithSecurityKeyAsync(CancellationToken cancellationToken)
    {
        if (_authResponse == null || _authResponse.TwoFactor.Fido2 == null)
        {
            return AuthResult.Fail(AuthError.TwoFactorAuthFailed);
        }

        List<AllowedCredential> allowedCredentials = _authResponse.TwoFactor.Fido2.AuthenticationOptions.PublicKey.AllowCredentials
            .Select(ac => new AllowedCredential(ac.Id.ToArray(), ac.Type)).ToList();

        WebAuthnResponse authResult = await _webAuthnAuthenticator.AuthenticateAsync(
            rpId: _authResponse.TwoFactor.Fido2.AuthenticationOptions.PublicKey.RpId,
            challenge: _authResponse.TwoFactor.Fido2.AuthenticationOptions.PublicKey.Challenge.ToArray(),
            userVerificationRequirement: _authResponse.TwoFactor.Fido2.AuthenticationOptions.PublicKey.UserVerification,
            timeoutInMilliseconds: _authResponse.TwoFactor.Fido2.AuthenticationOptions.PublicKey.Timeout,
            allowedCredentials: allowedCredentials,
            cancellationToken: cancellationToken);

        if (authResult is null)
        {
            return AuthResult.Fail(AuthError.WebAuthnNotSupported);
        }

        TwoFactorRequest request = new()
        {
            TwoFactorCode = null,
            Fido2 = new Fido2Request()
            {
                AuthenticationOptions = _authResponse.TwoFactor.Fido2.AuthenticationOptions,
                ClientData = Convert.ToBase64String(authResult.ClientDataJson),
                AuthenticatorData = Convert.ToBase64String(authResult.AuthenticatorData),
                Signature = Convert.ToBase64String(authResult.Signature),
                CredentialId = authResult.CredentialId.ToList(),
            },
        };

        ApiResponseResult<BaseResponse> response = await _apiClient.GetTwoFactorAuthResponse(
            request,
            _authResponse?.AccessToken ?? string.Empty,
            _authResponse?.UniqueSessionId ?? string.Empty,
            cancellationToken);

        if (response.Failure)
        {
            return AuthResult.Fail(AuthError.TwoFactorAuthFailed);
        }

        SaveAuthSessionDetails(_authResponse);

        return AuthResult.Ok();
    }
}