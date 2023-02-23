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

using System.Collections.Generic;
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Servers.Models;

namespace ProtonVPN.Settings.Migrations.v1_7_2
{
    internal class MigratedFeatures
    {
        private static readonly Dictionary<ServerTypeV1, Features> Map = new Dictionary<ServerTypeV1, Features>
        {
            { ServerTypeV1.Standard, Features.None },
            { ServerTypeV1.SecureCore, Features.SecureCore },
            { ServerTypeV1.P2P, Features.P2P },
            { ServerTypeV1.Tor, Features.Tor },
            { default, Features.None }
        };

        private readonly ServerTypeV1 _serverType;
        private readonly Server _server;

        public MigratedFeatures(ServerTypeV1 serverType, Server server)
        {
            _serverType = serverType;
            _server = server;
        }

        public static implicit operator Features(MigratedFeatures item) => item.Value();

        public Features Value()
        {
            if (_server != null)
            {
                var features = new ServerFeatures(_server.Features);
                if (features.IsSecureCore())
                    return Features.SecureCore;
                if (features.SupportsTor())
                    return Features.Tor;
                if (features.SupportsP2P())
                    return Features.P2P;
            }

            return Map[_serverType];
        }
    }
}
