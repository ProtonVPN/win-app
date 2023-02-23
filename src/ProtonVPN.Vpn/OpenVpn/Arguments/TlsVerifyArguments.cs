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

using System.Collections;
using System.Collections.Generic;
using ProtonVPN.Common.Configuration;

namespace ProtonVPN.Vpn.OpenVpn.Arguments
{
    public class TlsVerifyArguments : IEnumerable<string>
    {
        private const string ServerNameEnvironmentVariable = "peer_dns_name";

        private readonly OpenVpnConfig _config;
        private readonly string _serverName;

        public TlsVerifyArguments(OpenVpnConfig config, string serverName)
        {
            _config = config;
            _serverName = serverName;
        }

        public IEnumerator<string> GetEnumerator()
        {
            yield return $"--setenv {ServerNameEnvironmentVariable} \"{_serverName}\"";
            yield return $"--tls-export-cert \"{_config.TlsExportCertFolder}\"";
            yield return $"--tls-verify \"{_config.TlsVerifyExePath}\"";
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
