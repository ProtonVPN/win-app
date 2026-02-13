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

using System.Net;
using System.Security;
using ProtonVPN.Api.Contracts.Auth;
using ProtonVPN.Client.Logic.Auth.Srp.Contracts;

namespace ProtonVPN.Integration.Tests;

public class TestSrpProofGenerator : ISrpProofGenerator
{
    private const string DEFAULT_SERVER_PROOF = "INVALID_SERVER_PROOF";
    private const string DEFAULT_CLIENT_EPHEMERAL = "TEST_CLIENT_EPHEMERAL";
    private const string DEFAULT_CLIENT_PROOF = "TEST_CLIENT_PROOF";

    private readonly Dictionary<string, string> _expectedServerProofByPassword = [];

    public void SetExpectedServerProof(string password, string expectedServerProof)
    {
        _expectedServerProofByPassword[password] = expectedServerProof;
    }

    public SrpProof? GenerateProof(SecureString password, AuthInfoResponse authInfoResponse)
    {
        string plainPassword = new NetworkCredential(string.Empty, password).Password;
        string expectedServerProof = _expectedServerProofByPassword.TryGetValue(plainPassword, out string? proof)
            ? proof
            : DEFAULT_SERVER_PROOF;

        return new SrpProof
        {
            ClientEphemeral = DEFAULT_CLIENT_EPHEMERAL,
            ClientProof = DEFAULT_CLIENT_PROOF,
            ExpectedServerProof = expectedServerProof
        };
    }
}