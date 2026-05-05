/*
 * Copyright (c) 2025 Proton AG
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

using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Common.Legacy.Vpn;
using ProtonVPN.Vpn.Common;

namespace ProtonVPN.Vpn.Management;

public interface IManagementClient
{
    Channel<VpnState> StateChannel { get; }
    NetworkTraffic NetworkTraffic { get; }

    Task CloseVpnConnectionAsync();
    Task ConnectAsync(int port, string password, CancellationToken cancellationToken);
    void Disconnect();
    void ResetState();
    Task StartVpnConnectionAsync(VpnCredentials credentials, VpnEndpoint endpoint, CancellationToken cancellationToken);
}