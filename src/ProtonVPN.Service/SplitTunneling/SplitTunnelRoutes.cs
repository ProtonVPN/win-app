/*
 * Copyright (c) 2020 Proton Technologies AG
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
using ProtonVPN.Common.Logging;
using ProtonVPN.NetworkFilter;
using ProtonVPN.Service.Net;

namespace ProtonVPN.Service.SplitTunneling
{
    public class SplitTunnelRoutes
    {
        private readonly ILogger _logger;
        private readonly List<string> _ips = new List<string>();

        public SplitTunnelRoutes(ILogger logger)
        {
            _logger = logger;
        }

        public void Add(IEnumerable<string> ips)
        {
            _logger.Info("SplitTunnel: Adding routes");

            foreach (var ip in ips)
            {
                EnsureSucceeded(() => NetworkUtil.AddRoute(ip), "Adding route");
                _ips.Add(ip);
            }
        }

        public void Remove()
        {
            if (!_ips.Any())
            {
                return;
            }

            _logger.Info("SplitTunnel: Removing routes");

            foreach (var ip in _ips)
            {
                EnsureSucceeded(() => NetworkUtil.DeleteRoute(ip), "Removing route");
            }

            _ips.Clear();
        }

        private void EnsureSucceeded(System.Action action, string actionMessage)
        {
            try
            {
                action();
            }
            catch (NetworkFilterException e)
            {
                _logger.Error($"SplitTunnel: {actionMessage} failed, error code {e.Code}");
            }
        }
    }
}
