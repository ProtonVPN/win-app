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

using System.Net;
using System.Net.NetworkInformation;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.OperatingSystems.Network.Contracts;

namespace ProtonVPN.OperatingSystems.Network.NetworkInterface;

/// <summary>
/// Provides access to network interface on the system.
/// </summary>
public class SystemNetworkInterface : INetworkInterface, IEquatable<SystemNetworkInterface>
{
    private readonly System.Net.NetworkInformation.NetworkInterface _networkInterface;

    public SystemNetworkInterface(System.Net.NetworkInformation.NetworkInterface networkInterface)
    {
        _networkInterface = networkInterface ?? throw new ArgumentNullException(nameof(networkInterface), "NetworkInterface cannot be null.");
    }

    public string Id => _networkInterface.Id;

    public string Name => _networkInterface.Name;

    public string Description => _networkInterface.Description;

    public bool IsLoopback => _networkInterface.NetworkInterfaceType == NetworkInterfaceType.Loopback;

    public bool IsActive => _networkInterface.OperationalStatus == OperationalStatus.Up;

    public IPAddress DefaultGateway => _networkInterface.GetIPProperties().GatewayAddresses.FirstOrDefault()?.Address ?? IPAddress.None;

    public uint Index
    {
        get
        {
            IPv4InterfaceProperties props = _networkInterface.GetIPProperties().GetIPv4Properties();
            return props != null ? Convert.ToUInt32(props.Index) : 0;
        }
    }

    public List<NetworkAddress> GetUnicastAddresses()
    {
        return _networkInterface
            .GetIPProperties().UnicastAddresses
            .Select(a => new NetworkAddress(a.Address))
            .ToList();
    }

    public bool Equals(SystemNetworkInterface? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Id == other.Id;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as SystemNetworkInterface);
    }

    public override int GetHashCode()
    {
        return Id != null ? Id.GetHashCode() : 0;
    }
}