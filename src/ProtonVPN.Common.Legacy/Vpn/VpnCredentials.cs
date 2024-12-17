﻿/*
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
    public VpnCredentials(
        string clientCertPem,
        DateTime? clientCertificateExpirationDateUtc,
        AsymmetricKeyPair clientKeyPair)
    {
        Ensure.NotNull(clientKeyPair, nameof(clientKeyPair));

        ClientCertPem = clientCertPem;
        ClientCertificateExpirationDateUtc = clientCertificateExpirationDateUtc;
        ClientKeyPair = clientKeyPair;
    }

    public VpnCredentials(AsymmetricKeyPair clientKeyPair) : this(string.Empty, null, clientKeyPair)
    {
    }

    public string ClientCertPem { get; }
    public DateTime? ClientCertificateExpirationDateUtc { get; }

    public AsymmetricKeyPair ClientKeyPair { get; }
}