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

using ProtonVPN.OperatingSystems.WebAuthn.Contracts;
using ProtonVPN.OperatingSystems.WebAuthn.Enums;
using ProtonVPN.OperatingSystems.WebAuthn.Interop;

namespace ProtonVPN.OperatingSystems.WebAuthn;

public class WebAuthnAuthenticator : IWebAuthnAuthenticator
{
    private const int MINIMUM_TIMEOUT_IN_MILLISECONDS = 30000;

    private readonly WebAuthnApi _api = new();

    public async Task<WebAuthnResponse> AuthenticateAsync(string rpId,
        byte[] challenge,
        string userVerificationRequirement = null,
        int? timeoutInMilliseconds = null,
        IReadOnlyList<AllowedCredential> allowedCredentials = null,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        List<PublicKeyCredentialDescriptor> allowCredentials = allowedCredentials?
            .Select(ac => new PublicKeyCredentialDescriptor(ac.Id, type: ac.Type)).ToList();
        UserVerificationRequirement userVerificationEnum = UserVerificationParser.Parse(userVerificationRequirement);

        AuthenticatorAssertionResponse authResult = await _api.AuthenticatorGetAssertionAsync(rpId, challenge,
            userVerificationEnum,
            AuthenticatorAttachment.Any,
            timeoutMilliseconds: GetTimeoutInMilliseconds(timeoutInMilliseconds), // This argument is useless, Windows uses its own values: 30 seconds for touch, and some value (over a minute) for PIN
            allowCredentials: allowCredentials,
            cancellationToken: cancellationToken);

        return new WebAuthnResponse()
        {
            AuthenticatorData = authResult.AuthenticatorData,
            Signature = authResult.Signature,
            CredentialId = authResult.CredentialId,
            ClientDataJson = authResult.ClientDataJson,
        };
    }

    private int GetTimeoutInMilliseconds(int? arg)
    {
        int timeoutInMilliseconds = arg ?? ApiConstants.DefaultTimeoutMilliseconds;
        return Math.Max(timeoutInMilliseconds, MINIMUM_TIMEOUT_IN_MILLISECONDS);
    }
}
