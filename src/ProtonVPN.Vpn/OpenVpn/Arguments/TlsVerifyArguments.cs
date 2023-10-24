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
using ProtonVPN.Configurations.Contracts.Entities;

namespace ProtonVPN.Vpn.OpenVpn.Arguments;

public class TlsVerifyArguments : IEnumerable<string>
{
    private const string SERVER_NAME_ENVIRONMENT_VARIABLE = "peer_dns_name";

    private readonly IOpenVpnConfigurations _openVpnConfigs;
    private readonly string _serverName;

    public TlsVerifyArguments(IOpenVpnConfigurations openVpnConfigs, string serverName)
    {
        _openVpnConfigs = openVpnConfigs;
        _serverName = serverName;
    }

    public IEnumerator<string> GetEnumerator()
    {
        yield return $"--setenv {SERVER_NAME_ENVIRONMENT_VARIABLE} \"{_serverName}\"";
        yield return $"--tls-export-cert \"{_openVpnConfigs.TlsExportCertFolder}\"";
        yield return $"--tls-verify \"{_openVpnConfigs.TlsVerifyExePath}\"";
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
