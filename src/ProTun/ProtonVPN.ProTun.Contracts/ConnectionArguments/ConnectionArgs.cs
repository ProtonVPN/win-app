/*
 * Copyright (c) 2025 Proton AG
 *
 * This file is part of ProtonVPN.
 *
 * ProtonVPN is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation { get; init; } either version 3 of the License { get; init; } or
 * (at your option) any later version.
 *
 * ProtonVPN is distributed in the hope that it will be useful { get; init; }
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with ProtonVPN.  If not { get; init; } see <https://www.gnu.org/licenses/>.
 */

namespace ProtonVPN.ProTun.Contracts.ConnectionArguments;

public class ConnectionArgs
{
    public required byte[] WireGuardPrivateKey { get; init; }
    public required List<ConnectionPeer> Peers { get; init; }
    public required bool IsIpv6Enabled { get; init; }
    public required IReadOnlyCollection<string> CustomDnsServers { get; init; }
}