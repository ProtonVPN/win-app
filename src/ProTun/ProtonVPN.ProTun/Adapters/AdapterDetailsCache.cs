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

using ProtonVPN.ProTun.Contracts.Adapters;

namespace ProtonVPN.ProTun.Adapters;

public class AdapterDetailsCache : IAdapterDetailsCache
{
    private readonly object _lock = new();

    private string _serverGatewayIpv4Address = "10.2.0.1";
    public string ServerGatewayIpv4Address { get { lock (_lock) { return _serverGatewayIpv4Address; } } }

    private string _clientIpv4Address = "10.2.0.2";
    public string ClientIpv4Address { get { lock (_lock) { return _clientIpv4Address; } } }

    private string _serverGatewayIpv6Address = "2a07:b944::2:1";
    public string ServerGatewayIpv6Address { get { lock (_lock) { return _serverGatewayIpv6Address; } } }

    private string _clientIpv6Address = "2a07:b944::2:2";
    public string ClientIpv6Address { get { lock (_lock) { return _clientIpv6Address; } } }

    public void Set(AdapterDetails adapterDetails)
    {
        lock (_lock)
        {
            _serverGatewayIpv4Address = adapterDetails.ServerIpv4Addr;
            _clientIpv4Address = adapterDetails.ClientIpv4Addr;
            _serverGatewayIpv6Address = adapterDetails.ServerIpv6Addr;
            _clientIpv6Address = adapterDetails.ClientIpv6Addr;
        }
    }
}