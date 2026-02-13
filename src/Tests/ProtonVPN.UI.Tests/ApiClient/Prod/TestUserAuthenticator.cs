/*
 * Copyright (c) 2026 Proton AG
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
using NSubstitute;
using ProtonVPN.Client.Logic.Auth.Srp;
using ProtonVPN.Client.Logic.Auth.Srp.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.UI.Tests.ApiClient.Contracts;

namespace ProtonVPN.UI.Tests.ApiClient.Prod;

public class TestUserAuthenticator
{
    private ProdTestApiClient _prodApiClient = new();
    private readonly SrpProofGenerator _srpProofGenerator;

    public TestUserAuthenticator()
    {
        _srpProofGenerator = new SrpProofGenerator(Substitute.For<ILogger>());
    }

    public async Task<AuthResponse?> CreateSessionAsync(string username, SecureString password)
    {
        AuthInfoRequest authInfoRequest = new()
        {
            Username = username
        };

        AuthInfoResponse? authInfoResponse = await _prodApiClient.GetAuthInfoAsync(authInfoRequest);
        if (string.IsNullOrEmpty(authInfoResponse?.Salt))
        {
            throw new Exception("Incorrect login credentials");
        }

        SrpProof proofs = _srpProofGenerator.GenerateProof(password, new()
        {
            Salt = authInfoResponse.Salt,
            Modulus = authInfoResponse.Modulus,
            ServerEphemeral = authInfoResponse.ServerEphemeral
        }) ?? throw new Exception("Failed to generate SRP proof");

        try
        {
            AuthRequest authRequest = GetAuthRequestData(
                proofs.ClientEphemeral,
                proofs.ClientProof,
                authInfoResponse.SRPSession, username);
            AuthResponse? response = await _prodApiClient.GetAuthResponseAsync(authRequest);
            ProdTestApiClient.UID = response?.UID;
            ProdTestApiClient.AcessToken = response?.AccessToken;
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