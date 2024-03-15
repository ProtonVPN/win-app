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

using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;

namespace ProtonVPN.ProcessCommunication.EntityMapping.Vpn;

public class ConnectionStatusMapper : IMapper<ConnectionStatus, VpnStatusIpcEntity>
{
    public VpnStatusIpcEntity Map(ConnectionStatus leftEntity)
    {
        return leftEntity switch
        {
            ConnectionStatus.Disconnected => VpnStatusIpcEntity.Disconnected,
            ConnectionStatus.Connecting => VpnStatusIpcEntity.Connecting,
            ConnectionStatus.Connected => VpnStatusIpcEntity.Connected,
            _ => throw new NotImplementedException("Connection Status has an unknown value.")
        };
    }

    public ConnectionStatus Map(VpnStatusIpcEntity rightEntity)
    {
        return rightEntity switch
        {
            // Disconnecting Ipc status is mapped to Disconnected in the client application for better UX 
            // When the Disconnect action is called, the application visually switches to Disconnected faster
            // rather than maintaining the connected state until fully disconnected
            VpnStatusIpcEntity.Disconnected or
            VpnStatusIpcEntity.Disconnecting => ConnectionStatus.Disconnected,

            VpnStatusIpcEntity.Pinging or
            VpnStatusIpcEntity.Connecting or
            VpnStatusIpcEntity.Reconnecting or
            VpnStatusIpcEntity.Waiting or
            VpnStatusIpcEntity.Authenticating or
            VpnStatusIpcEntity.RetrievingConfiguration or
            VpnStatusIpcEntity.AssigningIp => ConnectionStatus.Connecting,

            VpnStatusIpcEntity.Connected or
            VpnStatusIpcEntity.ActionRequired => ConnectionStatus.Connected,

            _ => throw new NotImplementedException("VPN Status has an unknown value.")
        };
    }
}
