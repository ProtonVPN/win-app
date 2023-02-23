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

using System.IO;
using System.Linq;
using System.Net;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.ConnectionLogs;
using ProtonVPN.NetworkFilter;

namespace ProtonVPN.Service.SplitTunneling
{
    internal class SplitTunnelClient : ISplitTunnelClient
    {
        private readonly ILogger _logger;
        private readonly BestNetworkInterface _bestInterface;
        private readonly SplitTunnelNetworkFilters _filters;

        public SplitTunnelClient(
            ILogger logger,
            BestNetworkInterface bestInterface,
            SplitTunnelNetworkFilters filters)
        {
            _logger = logger;
            _bestInterface = bestInterface;
            _filters = filters;
        }

        public void EnableExcludeMode(string[] appPaths, string[] ips)
        {
            string[] apps = GetAppPaths(appPaths);
            if ((apps == null || apps.Length == 0) && (ips == null || ips.Length == 0))
            {
                return;
            }

            EnsureSucceeded(
                () => _filters.EnableExcludeMode(apps, ips, _bestInterface.LocalIpAddress()),
                "SplitTunnel: Enabling exclude mode");
        }

        public void EnableIncludeMode(string[] appPaths, string[] ips, string vpnLocalIp)
        {
            string[] apps = GetAppPaths(appPaths);
            if ((apps == null || apps.Length == 0) && (ips == null || ips.Length == 0))
            {
                return;
            }

            EnsureSucceeded(() => _filters.EnableIncludeMode(apps, IPAddress.Parse(vpnLocalIp)),
                "SplitTunnel: Enabling include mode");
        }

        public void Disable()
        {
            EnsureSucceeded(
                () => _filters.Disable(),
                "SplitTunnel: Disabling");
        }

        private string[] GetAppPaths(string[] paths)
        {
            return paths.Where(File.Exists).ToArray();
        }

        private void EnsureSucceeded(System.Action action, string actionMessage)
        {
            try
            {
                action();
                _logger.Info<ConnectionLog>($"{actionMessage} succeeded");
            }
            catch (NetworkFilterException e)
            {
                _logger.Error<ConnectionLog>($"{actionMessage} failed. Error code: {e.Code}");
            }
        }
    }
}