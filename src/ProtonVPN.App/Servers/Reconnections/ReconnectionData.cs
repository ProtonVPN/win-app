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

using ProtonVPN.Core.Servers.Models;

namespace ProtonVPN.Servers.Reconnections
{
    public class ReconnectionData
    {
        public bool IsReconnection { get; }

        public Server FromServer { get; }
        public bool IsFromServerSecureCore { get; }

        public Server ToServer { get; }
        public bool IsToServerSecureCore { get; }

        public ReconnectionData()
        {
        }

        public ReconnectionData(Server previousServer, Server currentServer)
        {
            IsReconnection = previousServer != null && currentServer != null;
            FromServer = previousServer;
            IsFromServerSecureCore = previousServer?.IsSecureCore() ?? false;
            ToServer = currentServer;
            IsToServerSecureCore = currentServer?.IsSecureCore() ?? false;
        }
    }
}