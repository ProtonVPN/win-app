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

using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Helpers;
using System;
using ProtonVPN.Crypto;

namespace ProtonVPN.Common.Vpn
{
    public struct VpnHost
    {
        public VpnHost(string name, string ip, string label, PublicKey x25519PublicKey, string signature)
        {
            AssertHostNameIsValid(name);
            AssertIpAddressIsValid(ip);

            Name = name;
            Ip = ip;
            Label = label;
            X25519PublicKey = x25519PublicKey;
            Signature = signature;
        }

        public string Name { get; }

        public string Ip { get; }

        public string Label { get; }

        public PublicKey X25519PublicKey { get; }

        public string Signature { get; }

        public bool IsEmpty() => string.IsNullOrEmpty(Name) && string.IsNullOrEmpty(Ip);

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
            Ensure.NotEmpty(ip, nameof(ip));

            if (!ip.IsValidIpAddress())
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
            VpnHost vpnHost = (VpnHost)o;
            return vpnHost != null && Ip == vpnHost.Ip &&
                   (Label == vpnHost.Label || (string.IsNullOrEmpty(Label) && string.IsNullOrEmpty(vpnHost.Label)));
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
}