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

using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;
using ProtonVPN.OperatingSystems.WebAuthn.Contracts;
using ProtonVPN.OperatingSystems.WebAuthn.Enums;
using ProtonVPN.OperatingSystems.WebAuthn.Interop;

namespace ProtonVPN.OperatingSystems.WebAuthn;

public class WebAuthnAuthenticator : IWebAuthnAuthenticator
{
    private const int MINIMUM_TIMEOUT_IN_MILLISECONDS = 30000;

    private readonly ILogger _logger;

    public bool IsSupported => CreateWebAuthnApi() is not null;

    public WebAuthnAuthenticator(ILogger logger)
    {
        _logger = logger;
    }

    public async Task<WebAuthnResponse> AuthenticateAsync(string rpId,
        byte[] challenge,
        string userVerificationRequirement = null,
        int? timeoutInMilliseconds = null,
        IReadOnlyList<AllowedCredential> allowedCredentials = null,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        WebAuthnApi api = CreateWebAuthnApi();

        if (api is null)
        {
            return null;
        }

        List<PublicKeyCredentialDescriptor> allowCredentials = allowedCredentials?
            .Select(ac => new PublicKeyCredentialDescriptor(ac.Id, type: ac.Type)).ToList();
        UserVerificationRequirement userVerificationEnum = UserVerificationParser.Parse(userVerificationRequirement);

        AuthenticatorAssertionResponse authResult = await api.AuthenticatorGetAssertionAsync(rpId, challenge,
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

    private WebAuthnApi CreateWebAuthnApi()
    {
        try
        {
            return new();
        }
        catch (Exception ex)
        {
            _logger.Warn<AppLog>("WebAuthN is not supported in this OS or by the Remote Desktop Connection.", ex);
            return null;
        }
    }

    private int GetTimeoutInMilliseconds(int? arg)
    {
        int timeoutInMilliseconds = arg ?? ApiConstants.DefaultTimeoutMilliseconds;
        return Math.Max(timeoutInMilliseconds, MINIMUM_TIMEOUT_IN_MILLISECONDS);
    }
}
