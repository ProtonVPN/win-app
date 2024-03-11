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
using ProtonVPN.Common.Legacy.Helpers;
using ProtonVPN.Crypto.Contracts;

namespace ProtonVPN.Common.Legacy.Vpn;

public readonly struct VpnCredentials
{
    public bool IsCertificateCredentials { get; }

    public string Username { get; }
    public string Password { get; }

    public string ClientCertificatePem { get; }
    public DateTime ClientCertificateExpirationDateUtc { get; }
    public AsymmetricKeyPair ClientKeyPair { get; }

    public VpnCredentials(string clientCertificatePem,
        DateTime clientCertificateExpirationDateUtc,
        AsymmetricKeyPair clientKeyPair)
    {
        Ensure.NotNull(clientKeyPair, nameof(clientKeyPair));

        ClientCertificatePem = clientCertificatePem;
        ClientCertificateExpirationDateUtc = clientCertificateExpirationDateUtc;
        ClientKeyPair = clientKeyPair;

        Username = null;
        Password = null;

        IsCertificateCredentials = true;
    }

    public VpnCredentials(string username, string password)
    {
        Ensure.NotEmpty(username, nameof(username));
        Ensure.NotEmpty(password, nameof(password));

        Username = username;
        Password = password;

        ClientCertificatePem = null;
        ClientCertificateExpirationDateUtc = DateTime.MinValue;
        ClientKeyPair = null;

        IsCertificateCredentials = false;
    }
}