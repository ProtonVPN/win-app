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

using System.Runtime.InteropServices;
using System.Security;
using ProtonVPN.Api.Contracts.Auth;
using ProtonVPN.Client.Logic.Auth.Srp.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;

namespace ProtonVPN.Client.Logic.Auth.Srp;

public class SrpProofGenerator : ISrpProofGenerator
{
    private readonly ILogger _logger;

    public SrpProofGenerator(ILogger logger)
    {
        _logger = logger;
    }

    public SrpProof? GenerateProof(SecureString password, AuthInfoResponse authInfoResponse)
    {
        using DisposableBytes passwordBytes = password.ToDisposableBytes();

        int result = NativeMethods.GenerateProof(
            passwordBytes.Data,
            (nuint)passwordBytes.Length,
            authInfoResponse.Salt,
            authInfoResponse.Modulus,
            authInfoResponse.ServerEphemeral,
            out FfiSrpProof proofs,
            out IntPtr errorPtr);

        if (result != 0)
        {
            string errorMessage = GetErrorMessage(errorPtr);

            _logger.Error<AppLog>($"Failed to generate SRP proof: {errorMessage}");

            return null;
        }

        try
        {
            string clientEphemeral = Marshal.PtrToStringUTF8(proofs.ClientEphemeral) ?? string.Empty;
            string clientProof = Marshal.PtrToStringUTF8(proofs.ClientProof) ?? string.Empty;
            string expectedServerProof = Marshal.PtrToStringUTF8(proofs.ExpectedServerProof) ?? string.Empty;

            return new SrpProof
            {
                ClientEphemeral = clientEphemeral,
                ClientProof = clientProof,
                ExpectedServerProof = expectedServerProof
            };
        }
        finally
        {
            NativeMethods.FreeProof(ref proofs);
        }
    }

    private static string GetErrorMessage(IntPtr errorPtr)
    {
        if (errorPtr == IntPtr.Zero)
        {
            return "No error";
        }

        try
        {
            return Marshal.PtrToStringUTF8(errorPtr) ?? "Unknown error";
        }
        finally
        {
            NativeMethods.FreeString(errorPtr);
        }
    }
}