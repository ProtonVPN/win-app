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
using ProtonVPN.Common.Networking;

namespace ProtonVPN.Service.Contract.Vpn
{
    [DataContract]
    public class VpnStateContract
    {
        public VpnStateContract(
            VpnStatusContract status,
            VpnErrorTypeContract error,
            string endpointIp,
            bool networkBlocked,
            OpenVpnAdapter? openVpnAdapterType,
            VpnProtocolContract vpnProtocol,
            string label)
        {
            Status = status;
            Error = error;
            EndpointIp = endpointIp;
            NetworkBlocked = networkBlocked;
            OpenVpnAdapterType = openVpnAdapterType;
            VpnProtocol = vpnProtocol;
            Label = label;
        }

        [DataMember] public VpnStatusContract Status { get; private set; }

        [DataMember] public VpnErrorTypeContract Error { get; private set; }

        [DataMember] public bool NetworkBlocked { get; private set; }

        [DataMember] public string EndpointIp { get; private set; }
        
        [DataMember] public OpenVpnAdapter? OpenVpnAdapterType { get; private set; }

        [DataMember] public VpnProtocolContract VpnProtocol { get; private set; }

        [DataMember] public string Label { get; private set; }
    }
}