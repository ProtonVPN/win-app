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

using ProtonVPN.Common.Vpn;
using ProtonVPN.Gui.Enums;

namespace ProtonVPN.Gui.Mappers;

public class ConnectionStatusMapper
{
    public static ConnectionStatus Map(VpnStatus leftEntity)
    {
        switch (leftEntity)
        {
            case VpnStatus.Disconnected:
            case VpnStatus.Disconnecting:
                return ConnectionStatus.Disconnected;

            case VpnStatus.Pinging:
            case VpnStatus.Connecting:
            case VpnStatus.Reconnecting:
            case VpnStatus.Waiting:
            case VpnStatus.Authenticating:
            case VpnStatus.RetrievingConfiguration:
            case VpnStatus.AssigningIp:
                return ConnectionStatus.Connecting;

            case VpnStatus.Connected:
            case VpnStatus.ActionRequired:
                return ConnectionStatus.Connected;

            default:
                throw new ArgumentException($"Vpn Status {leftEntity} has not been mapped to a connection status");
        }
    }
}