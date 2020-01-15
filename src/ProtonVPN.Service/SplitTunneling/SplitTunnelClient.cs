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

using System.IO;
using System.Linq;
using System.Net;
using ProtonVPN.Common.Logging;
using ProtonVPN.NetworkFilter;

namespace ProtonVPN.Service.SplitTunneling
{
    public class SplitTunnelClient : ISplitTunnelClient
    {
        private readonly ILogger _logger;
        private readonly BestNetworkInterface _bestInterface;
        private readonly SplitTunnelNetworkFilters _filters;
        private readonly SplitTunnelRoutes _routes;

        public SplitTunnelClient(
            ILogger logger,
            BestNetworkInterface bestInterface,
            SplitTunnelNetworkFilters filters,
            SplitTunnelRoutes routes)
        {
            _logger = logger;
            _bestInterface = bestInterface;
            _filters = filters;
            _routes = routes;
        }

        public void EnableExcludeMode(string[] appPaths, string[] ips)
        {
            if (appPaths != null && appPaths.Length > 0)
            {
                var apps = appPaths.Where(File.Exists).ToArray();
                if (apps.Length == 0)
                {
                    return;
                }

                EnsureSucceeded(
                    () => _filters.EnableExcludeMode(apps, _bestInterface.LocalIpAddress()),
                    "SplitTunnel: Enabling exclude mode");
            }

            if (ips != null && ips.Length > 0)
            {
                _routes.Add(ips);
            }
        }

        public void EnableIncludeMode(string[] appPaths, string vpnLocalIp)
        {
            if (appPaths != null && appPaths.Length > 0)
            {
                var apps = appPaths.Where(File.Exists).ToArray();
                if (apps.Length == 0)
                {
                    return;
                }
                EnsureSucceeded(
                    () => _filters.EnableIncludeMode(
                        apps,
                        _bestInterface.LocalIpAddress(),
                        IPAddress.Parse(vpnLocalIp)),
                    "SplitTunnel: Enabling include mode");
            }
        }

        public void Disable()
        {
            _routes.Remove();

            EnsureSucceeded(
                () => _filters.Disable(),
                "SplitTunnel: Disabling");
        }

        private void EnsureSucceeded(System.Action action, string actionMessage)
        {
            try
            {
                action();
                _logger.Info($"{actionMessage} succeeded");
            }
            catch (NetworkFilterException e)
            {
                _logger.Error($"{actionMessage} failed. Error code: {e.Code}");
            }
        }
    }
}
