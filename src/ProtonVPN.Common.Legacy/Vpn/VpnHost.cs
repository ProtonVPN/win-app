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

using System;
using System.Collections.Generic;
using ProtonVPN.Common.Core.Extensions;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Common.Legacy.Helpers;
using ProtonVPN.Crypto.Contracts;

namespace ProtonVPN.Common.Legacy.Vpn;

public struct VpnHost
{
    public string Name { get; }
    public string Ip { get; }
    public string Label { get; }
    public PublicKey X25519PublicKey { get; }
    public string Signature { get; }
    public bool IsIpv6Supported { get; }
    public Dictionary<VpnProtocol, string> RelayIpByProtocol { get; }

    public VpnHost(
        string name,
        string ip,
        string label,
        PublicKey x25519PublicKey,
        string signature,
        bool isIpv6Supported,
        Dictionary<VpnProtocol, string> relayIpByProtocol)
    {
        AssertHostNameIsValid(name);
        AssertIpAddressIsValid(ip);

        if (relayIpByProtocol is not null)
        {
            foreach (KeyValuePair<VpnProtocol, string> protocolIpPair in relayIpByProtocol)
            {
                AssertIpAddressIsValid(protocolIpPair.Value);
            }
        }

        Name = name;
        Ip = ip;
        Label = label;
        X25519PublicKey = x25519PublicKey;
        Signature = signature;
        IsIpv6Supported = isIpv6Supported;
        RelayIpByProtocol = relayIpByProtocol;
    }

    public bool IsEmpty() => string.IsNullOrEmpty(Name) && string.IsNullOrEmpty(Ip);

    public string GetIp(VpnProtocol protocol)
    {
        return RelayIpByProtocol is not null && RelayIpByProtocol.TryGetValue(protocol, out string relayIp)
            ? relayIp
            : Ip;
    }

    private static void AssertHostNameIsValid(string hostName)
    {
        Ensure.NotEmpty(hostName, nameof(hostName));

        UriHostNameType hostNameType = Uri.CheckHostName(hostName);
        if (hostNameType != UriHostNameType.Dns && hostNameType != UriHostNameType.IPv4)
        {
            throw new ArgumentException($"Invalid argument {nameof(hostName)} value: {hostName}");
        }
    }

    private static void AssertIpAddressIsValid(string ip)
    {
        if (!string.IsNullOrEmpty(ip) && !ip.IsValidIpAddressFormat())
        {
            throw new ArgumentException($"Invalid argument {nameof(ip)} value: {ip}");
        }
    }

    public static bool operator ==(VpnHost h1, VpnHost h2)
    {
        return h1.Equals(h2);
    }

    public override bool Equals(object o)
    {
        if (o == null)
        {
            return false;
        }

        VpnHost vpnHost = (VpnHost)o;
        return Ip == vpnHost.Ip &&
               (Label == vpnHost.Label || string.IsNullOrEmpty(Label) && string.IsNullOrEmpty(vpnHost.Label));

    }

    public override int GetHashCode()
    {
        return Tuple.Create(Ip, Label).GetHashCode();
    }

    public static bool operator !=(VpnHost h1, VpnHost h2)
    {
        return !h1.Equals(h2);
    }
}