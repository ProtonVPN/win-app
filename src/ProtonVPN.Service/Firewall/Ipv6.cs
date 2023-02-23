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

using System;
using System.Threading.Tasks;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.NetworkLogs;
using ProtonVPN.Common.Os.Net;
using ProtonVPN.Service.Settings;

namespace ProtonVPN.Service.Firewall
{
    internal class Ipv6
    {
        private const string AppName = "ProtonVPN";

        private readonly ILogger _logger;
        private readonly IConfiguration _config;
        private readonly IServiceSettings _serviceSettings;

        public Ipv6(ILogger logger, IConfiguration config, IServiceSettings serviceSettings)
        {
            _logger = logger;
            _config = config;
            _serviceSettings = serviceSettings;
        }

        public bool Enabled { get; private set; } = true;

        public Task DisableAsync()
        {
            return Task.Run(Disable);
        }

        public Task EnableAsync()
        {
            return Task.Run(Enable);
        }

        public Task EnableOnVPNInterfaceAsync()
        {
            return Task.Run(EnableOnVPNInterface);
        }

        public void Enable()
        {
            LoggingAction(NetworkUtil.EnableIPv6OnAllAdapters, "Enabling");
            Enabled = true;
        }

        private void Disable()
        {
            LoggingAction(NetworkUtil.DisableIPv6OnAllAdapters, "Disabling");
            Enabled = false;
        }

        private void EnableOnVPNInterface()
        {
            LoggingAction(NetworkUtil.EnableIPv6, "Enabling on VPN interface");
        }

        private void LoggingAction(Action<string, string> action, string actionMessage)
        {
            try
            {
                _logger.Info<NetworkLog>($"IPv6: {actionMessage}");
                action(AppName, _config.GetHardwareId(_serviceSettings.OpenVpnAdapter));
                _logger.Info<NetworkLog>($"IPv6: {actionMessage} succeeded");
            }
            catch (NetworkUtilException e)
            {
                _logger.Error<NetworkLog>($"IPV6: {actionMessage} failed, error code {e.Code}");
            }
        }
    }
}