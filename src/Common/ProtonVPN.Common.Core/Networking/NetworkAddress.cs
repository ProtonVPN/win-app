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
using System.Net.Sockets;
using ProtonVPN.Common.Core.Extensions;

namespace ProtonVPN.Common.Core.Networking;

public readonly record struct NetworkAddress
{
    private const int MIN_SUBNET = 0;
    private const int MAX_SUBNET_IPV4 = 32;
    private const int MAX_SUBNET_IPV6 = 128;
    private const string DEFAULT_IPV4_MASK = "255.255.255.255";

    public static NetworkAddress None => new(IPAddress.None);

    public IPAddress Ip { get; }

    public int? Subnet { get; }

    public bool IsIpV4 => Ip.AddressFamily == AddressFamily.InterNetwork;

    public bool IsIpV6 => Ip.AddressFamily == AddressFamily.InterNetworkV6;

    public string FormattedAddress => ToString();

    public bool IsSingleIp => !Subnet.HasValue
                           || Subnet == (IsIpV4 ? MAX_SUBNET_IPV4 : MAX_SUBNET_IPV6);

    public NetworkAddress(IPAddress ip)
    {
        Ip = ip;
    }

    private NetworkAddress(IPAddress ip, int? subnet = null)
        : this(ip)
    {
        Subnet = subnet;
    }

    public static bool TryParse(string? rawAddress, out NetworkAddress networkAddress)
    {
        networkAddress = NetworkAddress.None;

        try
        {
            if (string.IsNullOrWhiteSpace(rawAddress))
            {
                throw new ArgumentException("Address cannot be null or empty.", nameof(rawAddress));
            }
            
            // Split IP and CIDR subnet
            string[] parts = rawAddress.Trim().Split('/');
            if (!IPAddress.TryParse(parts[0], out IPAddress? ip))
            {
                throw new FormatException("Invalid IP address format.");
            }

            // Confirm the given IPv4 address is well formatted (#.#.#.#)
            if (ip.AddressFamily == AddressFamily.InterNetwork && 
                parts[0] != ip.ToString())
            {
                throw new FormatException("Invalid IPv4 address format.");
            }

            // Confirm there are 2 parts at most (<ip> or <ip>/<subnet>)
            if (parts.Length > 2)
            {
                throw new FormatException("Invalid CIDR notation format.");
            }

            // Confirm subnet value (if any) is in range 
            int? subnet = null;
            if (parts.Length == 2)
            {
                Range subnetRange = ip.AddressFamily == AddressFamily.InterNetwork
                    ? new Range(MIN_SUBNET, MAX_SUBNET_IPV4)
                    : new Range(MIN_SUBNET, MAX_SUBNET_IPV6);

                if (!int.TryParse(parts[1], out int subnetValue) || !subnetRange.Contains(subnetValue))
                {
                    throw new FormatException("Invalid subnet value.");
                }

                subnet = subnetValue;
            }

            networkAddress = new NetworkAddress(ip, subnet);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public string GetSubnetMaskString()
    {
        if (!IsIpV4)
        {
            throw new InvalidOperationException("Subnet mask conversion is only supported for IPv4 addresses.");
        }

        if (!Subnet.HasValue)
        {
            return DEFAULT_IPV4_MASK;
        }

        int cidr = Subnet.Value;
        uint mask = cidr == 0 ? 0 : 0xFFFFFFFF << (32 - cidr);

        byte[] bytes =
        [
            (byte)(mask >> 24),
            (byte)(mask >> 16),
            (byte)(mask >> 8),
            (byte)mask
        ];

        return string.Join(".", bytes);
    }

    public bool IsGlobalUnicastAddress()
    {
        byte[] bytes = Ip.GetAddressBytes();
        return Ip.AddressFamily == AddressFamily.InterNetworkV6 && (bytes[0] & 0xE0) == 0x20;
    }

    public override string ToString()
    {
        return IsSingleIp
            ? Ip.ToString()
            : $"{Ip}/{Subnet}";
    }
}