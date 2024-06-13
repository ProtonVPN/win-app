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

namespace ProtonVPN.UI.Tests.ApiClient.Prod;

public class TestUserAuthenticator
{
    private ProdTestApiClient _prodApiClient = new();

    public async Task<AuthResponse> CreateSessionAsync(string username, SecureString password)
    {
        var authInfoRequest = new AuthInfoRequest { Username = username };

        AuthInfoResponse authInfoResponse = await _prodApiClient.GetAuthInfoAsync(authInfoRequest);

        if (string.IsNullOrEmpty(authInfoResponse.Salt))
        {
            throw new Exception("Incorrect login credentials");
        }

        TestsSrpInvoke.GoProofs proofs = TestsSrpInvoke.GenerateProofs(4, username, password, authInfoResponse.Salt,
                authInfoResponse.Modulus, authInfoResponse.ServerEphemeral);

        try
        {
            string clientEphermal = Convert.ToBase64String(proofs.ClientEphemeral);
            string clientProof = Convert.ToBase64String(proofs.ClientProof);
            AuthRequest authRequest = GetAuthRequestData(clientEphermal, clientProof, authInfoResponse.SRPSession, username);
            AuthResponse response = await _prodApiClient.GetAuthResponseAsync(authRequest);
            ProdTestApiClient.UID = response.UID;
            ProdTestApiClient.AcessToken = response.AccessToken;
            return response;
        }
        catch (TypeInitializationException e) when (e.InnerException is DllNotFoundException)
        {
            throw new Exception("Go.srp was not found!");
        }
    }

    private AuthRequest GetAuthRequestData(string clientEphermal, string clientProof, string srpSession, string username)
    {
        return new AuthRequest
        {
            ClientEphemeral = clientEphermal,
            ClientProof = clientProof,
            SRPSession = srpSession,
            Username = username
        };
    }
}
