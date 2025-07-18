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

namespace ProtonVPN.Common.Core.Networking;

public class IpAddress
{
    // Prefix for 64:ff9b::/96 - Well-Known Prefix for NAT64
    private static readonly byte[] _ipv6Nat64Prefix = [0x00, 0x64, 0xff, 0x9b];

    // Prefix for 2001:2::/48 - Benchmarking
    private static readonly byte[] _ipv6BenchmarkPrefix = [0x20, 0x01, 0x00, 0x02];

    // Prefix for 2001:db8::/32 - Documentation
    private static readonly byte[] _ipv6DocumentationPrefix = [0x20, 0x01, 0x0d, 0xb8];

    // Prefix for 100::/64 - Discard-Only Address Block
    private static readonly byte[] _ipv6DiscardPrefix = [0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00];

    private IPAddress _systemTypeIpAddress = IPAddress.None;

    public byte[] Bytes
    {
        get => _systemTypeIpAddress.GetAddressBytes();
        set => _systemTypeIpAddress = new IPAddress(value);
    }

    public IpAddress()
    {
    }

    public IpAddress(IPAddress systemTypeIpAddress)
    {
        _systemTypeIpAddress = systemTypeIpAddress;
    }

    public IpAddress(byte[] bytes)
    {
        Bytes = bytes;
    }

    public override string ToString()
    {
        return _systemTypeIpAddress.ToString();
    }

    public IPAddress GetSystemType()
    {
        return _systemTypeIpAddress;
    }

    /// <summary>
    /// Determines if the IP address is a public, globally routable address.
    /// This implementation checks against all IANA-reserved special-use IP ranges.
    /// </summary>
    /// <returns>True if the IP is public, false otherwise.</returns>
    public bool IsPublicIp()
    {
        return _systemTypeIpAddress.AddressFamily switch
        {
            AddressFamily.InterNetwork => IsPublicIpv4(),
            AddressFamily.InterNetworkV6 => IsPublicIpv6(),
            _ => false
        };
    }

    /// <summary>
    /// Determines if an IPv4 address is public by checking against the IANA Special-Purpose Address Registry.
    /// See: https://www.iana.org/assignments/iana-ipv4-special-registry/iana-ipv4-special-registry.xhtml
    /// </summary>
    private bool IsPublicIpv4()
    {
        byte[] bytes = _systemTypeIpAddress.GetAddressBytes();

        return !(
            // RFC 1918: Private-Use
            bytes[0] == 10 ||                                          // 10.0.0.0/8
            (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) ||   // 172.16.0.0/12
            (bytes[0] == 192 && bytes[1] == 168) ||                    // 192.168.0.0/16

            // RFC 5735, 6890: Special-Use
            bytes[0] == 0 ||                                           // 0.0.0.0/8       - "This network"
            bytes[0] == 127 ||                                         // 127.0.0.0/8     - Loopback
            (bytes[0] == 169 && bytes[1] == 254) ||                    // 169.254.0.0/16  - Link-Local (APIPA)

            // RFC 6598: Shared Address Space
            (bytes[0] == 100 && bytes[1] >= 64 && bytes[1] <= 127) ||  // 100.64.0.0/10   - Carrier-grade NAT

            // RFC 5737: Documentation (TEST-NET-1, 2, 3)
            (bytes[0] == 192 && bytes[1] == 0 && bytes[2] == 2) ||     // 192.0.2.0/24    - TEST-NET-1
            (bytes[0] == 198 && bytes[1] == 51 && bytes[2] == 100) ||  // 198.51.100.0/24 - TEST-NET-2
            (bytes[0] == 203 && bytes[1] == 0 && bytes[2] == 113) ||   // 203.0.113.0/24  - TEST-NET-3

            // RFC 3068, 5736: Other reserved ranges
            (bytes[0] == 192 && bytes[1] == 0 && bytes[2] == 0) ||     // 192.0.0.0/24    - IETF Protocol Assignments
            (bytes[0] == 192 && bytes[1] == 88 && bytes[2] == 99) ||   // 192.88.99.0/24  - 6to4 Relay Anycast (Deprecated)
            (bytes[0] == 198 && (bytes[1] >= 18 && bytes[1] <= 19)) || // 198.18.0.0/15   - Benchmarking

            // RFC 1112, 6890: Multicast and future use
            bytes[0] >= 224);                                          // 224.0.0.0+      - Multicast, Reserved, and Broadcast
    }

    /// <summary>
    /// Determines if an IPv6 address is public by checking against the IANA Special-Purpose Address Registry.
    /// See: https://www.iana.org/assignments/iana-ipv6-special-registry/iana-ipv6-special-registry.xhtml
    /// </summary>
    private bool IsPublicIpv6()
    {
        if (IPAddress.IsLoopback(_systemTypeIpAddress) ||          // ::1/128
            _systemTypeIpAddress.IsIPv6LinkLocal ||                // fe80::/10
            _systemTypeIpAddress.IsIPv6Multicast ||                // ff00::/8
            _systemTypeIpAddress.IsIPv6Teredo ||                   // 2001::/32
            _systemTypeIpAddress.IsIPv6SiteLocal)                  // fec0::/10 (Deprecated, but good to check)
        {
            return false;
        }

        byte[] bytes = _systemTypeIpAddress.GetAddressBytes();
        ReadOnlySpan<byte> byteSpan = bytes;

        return !(
            _systemTypeIpAddress.Equals(IPAddress.IPv6None) || // ::/128 - Unspecified Address
            ((bytes[0] & 0xFE) == 0xFC) ||                     // fc00::/7 - Unique Local Addresses (ULA)
            byteSpan.StartsWith(_ipv6DocumentationPrefix) ||   // 2001:db8::/32
            byteSpan.StartsWith(_ipv6BenchmarkPrefix) ||       // 2001:2::/48
            IsNat64(byteSpan) ||                               // 64:ff9b::/96
            byteSpan.StartsWith(_ipv6DiscardPrefix));          // 100::/64
    }

    private static bool IsNat64(ReadOnlySpan<byte> bytes)
    {
        // Check for 64:ff9b::/96 and 64:ff9b:1::/48
        return bytes.StartsWith(_ipv6Nat64Prefix) &&
               // Check if the rest of the 96-bit prefix is all zeros
               (bytes.Slice(4, 8).SequenceEqual(stackalloc byte[8]) ||
               // Or if it matches the /48 for local use (64:ff9b:1::/48)
               (bytes[4] == 0 && bytes[5] == 1));
    }
}