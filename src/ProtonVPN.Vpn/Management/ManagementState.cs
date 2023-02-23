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
using ProtonVPN.Common.Vpn;
using System;
using System.Collections.Generic;

namespace ProtonVPN.Vpn.Management
{
    /// <summary>
    /// Maps OpenVPN state received over management interface to VPN status.
    /// </summary>
    internal class ManagementState
    {
        private static readonly Dictionary<string, VpnStatus> StatusMapping = new Dictionary<string, VpnStatus>(StringComparer.OrdinalIgnoreCase)
        {
            ["TCP_CONNECT"] = VpnStatus.Connecting,
            ["WAIT"] = VpnStatus.Waiting,
            ["AUTH"] = VpnStatus.Authenticating,
            ["GET_CONFIG"] = VpnStatus.RetrievingConfiguration,
            ["ASSIGN_IP"] = VpnStatus.AssigningIp,
            ["ADD_ROUTES"] = VpnStatus.AssigningIp,
            ["CONNECTING"] = VpnStatus.Connecting,
            ["CONNECTED"] = VpnStatus.Connected,
            ["DISCONNECTED"] = VpnStatus.Disconnecting,
            ["EXITING"] = VpnStatus.Disconnecting,
            ["RECONNECTING"] = VpnStatus.Reconnecting,
        };

        private readonly string _state;
        private readonly string _status;

        public ManagementState(string stateMessage, string statusMessage, string localIpAddress, string remoteAddress)
        {
            _state = stateMessage;
            _status = statusMessage;
            LocalIpAddress = localIpAddress;
            RemoteIpAddress = remoteAddress;
        }

        public VpnStatus Status => GetStatus();

        public bool HasStatus => GetStatus() != default(VpnStatus);

        public bool HasError => Status == VpnStatus.Connected && _status.EqualsIgnoringCase("ERROR") 
                                || HasTlsError();

        public VpnError Error => HasTlsError() ? VpnError.TlsError : VpnError.None;

        public static ManagementState Null => new ManagementState(null, null, null, null);

        public string LocalIpAddress { get; }

        public string RemoteIpAddress { get; }

        private VpnStatus GetStatus()
        {
            if (StatusMapping.TryGetValue(_state, out var status))
                return status;

            return default(VpnStatus);
        }

        private bool HasTlsError()
        {
            return Status == VpnStatus.Reconnecting && _status.EqualsIgnoringCase("tls-error");
        }
    }
}
