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

using ProtonVPN.Common.Helpers;
using ProtonVPN.Crypto;

namespace ProtonVPN.Common.Vpn
{
    public readonly struct VpnCredentials
    {
        public VpnCredentials(string clientCertPem, AsymmetricKeyPair clientKeyPair)
        {
            Ensure.NotNull(clientKeyPair, nameof(clientKeyPair));

            ClientCertPem = clientCertPem;
            ClientKeyPair = clientKeyPair;

            Username = null;
            Password = null;
        }
        
        public VpnCredentials(string username, string password)
        {
            Ensure.NotEmpty(username, nameof(username));
            Ensure.NotEmpty(password, nameof(password));

            Username = username;
            Password = password;

            ClientCertPem = null;
            ClientKeyPair = null;
        }

        public string Username { get; }

        public string Password { get; }

        public string ClientCertPem { get; }

        public AsymmetricKeyPair ClientKeyPair { get; }
    }
}