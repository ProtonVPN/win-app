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

using ProtonVPN.Common.Core.Networking;

namespace ProtonVPN.Client.Logic.Servers.Contracts.Models;

public class PhysicalServer : ILocation
{
    public required string Id { get; init; }
    public required string EntryIp { get; init; }
    // Is no longer used, but kept to not break the protobuf contract
    public required string ExitIp { get; init; }
    public required string Domain { get; init; }
    public required string Label { get; init; }
    public required sbyte Status { get; set; }
    public required string X25519PublicKey { get; init; }
    public required string Signature { get; init; }
    public bool IsIpv6Supported { get; set; }
    public Dictionary<VpnProtocol, string> RelayIpByProtocol { get; init; } = [];

    public bool IsUnderMaintenance() => Status == 0;

    public bool IsAvailable(IList<VpnProtocol> preferredProtocols)
    {
        return !IsUnderMaintenance() && HasEntryIp(preferredProtocols);
    }

    private bool HasEntryIp(IList<VpnProtocol> preferredProtocols)
    {
        if (!string.IsNullOrEmpty(EntryIp))
        {
            return true;
        }

        if (RelayIpByProtocol.Count == 0)
        {
            return false;
        }

        foreach (VpnProtocol protocol in preferredProtocols)
        {
            if (RelayIpByProtocol.ContainsKey(protocol))
            {
                return true;
            }
        }

        return false;
    }
}