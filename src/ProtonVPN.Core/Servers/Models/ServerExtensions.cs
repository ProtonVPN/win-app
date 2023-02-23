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
using System.Linq;

namespace ProtonVPN.Core.Servers.Models
{
    public static class ServerExtensions
    {
        public static bool IsSecureCore(this Server server) => ServerFeatures.IsSecureCore(server.Features);
        public static bool SupportsTor(this Server server) => ServerFeatures.SupportsTor(server.Features);
        public static bool SupportsP2P(this Server server) => ServerFeatures.SupportsP2P(server.Features);
        public static bool SupportsStreaming(this Server server) => ServerFeatures.SupportsStreaming(server.Features);
        public static bool SupportsIpV6(this Server server) => ServerFeatures.SupportsIpV6(server.Features);
        public static bool IsPartner(this Server server) => ServerFeatures.IsPartner(server.Features);

        public static bool Online(this Server server) => server.Status == 1;

        public static IEnumerable<Server> OnlineServers(this IEnumerable<Server> servers)
        {
            return servers.Where(s => s.Online());
        }

        public static IEnumerable<Server> UpToTierServers(this IEnumerable<Server> servers, sbyte maxTier)
        {
            return servers.Where(s => s.Tier <= maxTier);
        }

        public static bool IsNullOrEmpty(this Server server)
        {
            return server == null || server.IsEmpty();
        }
    }
}