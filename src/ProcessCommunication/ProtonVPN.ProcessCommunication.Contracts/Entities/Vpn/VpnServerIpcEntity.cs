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
using System.Runtime.Serialization;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Crypto;

namespace ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn
{
    [DataContract]
    public class VpnServerIpcEntity
    {
        [DataMember(Order = 1)]
        public string Name { get; set; }

        [DataMember(Order = 2)]
        public string Ip { get; set; }

        [DataMember(Order = 3)]
        public string Label { get; set; }

        [DataMember(Order = 4)]
        public ServerPublicKeyIpcEntity X25519PublicKey { get; set; }

        [DataMember(Order = 5)]
        public string Signature { get; set; }

        [DataMember(Order = 6)]
        public bool IsIpv6Supported { get; set; }

        [DataMember(Order = 7)]
        public Dictionary<VpnProtocolIpcEntity, string> RelayIpByProtocol { get; set; }
    }
}