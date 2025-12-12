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
using System.Net.Sockets;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.OperatingSystems.Network.Contracts;
using Vanara.PInvoke;
using static Vanara.PInvoke.IpHlpApi;
using static Vanara.PInvoke.Ws2_32;

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

    public bool IsIPv4ForwardingEnabled
    {
        get
        {
            MIB_IPINTERFACE_ROW row = new()
            {
                Family = ADDRESS_FAMILY.AF_INET,
                InterfaceIndex = Index,
            };

            Win32Error result = GetIpInterfaceEntry(ref row);
            return result.Succeeded && row.ForwardingEnabled;
        }
    }

    public IPAddress DefaultGateway
    {
        get
        {
            GatewayIPAddressInformation? ipv4Gateway = _networkInterface
                .GetIPProperties()
                .GatewayAddresses
                .FirstOrDefault(g => g.Address.AddressFamily == AddressFamily.InterNetwork && !g.Address.Equals(IPAddress.Any));

            return ipv4Gateway?.Address ?? IPAddress.None;
        }
    }

    public uint Index
    {
        get
        {
            try
            {
                IPv4InterfaceProperties ipv4Props = _networkInterface.GetIPProperties().GetIPv4Properties();
                if (ipv4Props != null)
                {
                    return Convert.ToUInt32(ipv4Props.Index);
                }

                IPv6InterfaceProperties ipv6Props = _networkInterface.GetIPProperties().GetIPv6Properties();
                if (ipv6Props != null)
                {
                    return Convert.ToUInt32(ipv6Props.Index);
                }
            }
            catch (Exception)
            {
            }

            return 0;
        }
    }

    public List<NetworkAddress> GetUnicastAddresses()
    {
        return _networkInterface
            .GetIPProperties().UnicastAddresses
            .Select(a => new NetworkAddress(a.Address))
            .ToList();
    }

    public IPAddress? GetPreferredIpv6UnicastAddress()
    {
        UnicastIPAddressInformationCollection unicastAddresses = _networkInterface.GetIPProperties().UnicastAddresses;

        List<UnicastIPAddressInformation> eligibleAddresses = unicastAddresses
            .Where(info => info.Address.AddressFamily == AddressFamily.InterNetworkV6)
            .Where(info => info.DuplicateAddressDetectionState == DuplicateAddressDetectionState.Preferred)
            .Where(info => info.AddressPreferredLifetime > 0)
            .Where(info => !info.Address.Equals(IPAddress.IPv6None))
            .Where(info => !info.Address.IsIPv6LinkLocal)
            .Where(info => !info.Address.IsIPv6Multicast)
            .ToList();

        UnicastIPAddressInformation? preferredTemporaryAddress = eligibleAddresses
            .Where(info => info.SuffixOrigin == SuffixOrigin.Random)
            .OrderByDescending(info => info.AddressPreferredLifetime)
            .FirstOrDefault();

        if (preferredTemporaryAddress is not null)
        {
            return preferredTemporaryAddress.Address;
        }

        UnicastIPAddressInformation? preferredStableAddress = eligibleAddresses
            .Where(info => info.SuffixOrigin != SuffixOrigin.Random)
            .OrderByDescending(info => info.AddressPreferredLifetime)
            .FirstOrDefault();

        return preferredStableAddress?.Address;
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