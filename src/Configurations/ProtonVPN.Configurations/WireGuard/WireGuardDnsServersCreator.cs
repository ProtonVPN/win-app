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

using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Configurations.Contracts.WireGuard;

namespace ProtonVPN.Configurations.WireGuard;

public class WireGuardDnsServersCreator : IWireGuardDnsServersCreator
{
    private readonly IConfiguration _config;

    public WireGuardDnsServersCreator(IConfiguration config)
    {
        _config = config;
    }

    public string GetDnsServers(IReadOnlyCollection<string> customDns, bool isIpv6Supported)
    {
        List<string> dnsAddresses = [];

        if (customDns.Count > 0)
        {
            foreach (string dns in customDns)
            {
                if (NetworkAddress.TryParse(dns, out NetworkAddress networkAddress) &&
                    networkAddress.IsIpV4 || networkAddress.IsIpV6 && isIpv6Supported)
                {
                    dnsAddresses.Add(dns);
                }
            }
        }

        //Always add the default DNS in case the ones provided by the user doesn't work
        dnsAddresses.Add(_config.WireGuard.DefaultServerGatewayIpv4Address);

        if (isIpv6Supported)
        {
            dnsAddresses.Add(_config.WireGuard.DefaultServerGatewayIpv6Address);
        }

        return string.Join(",", dnsAddresses);
    }
}
