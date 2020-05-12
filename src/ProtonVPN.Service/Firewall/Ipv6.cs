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

using System;
using System.Threading.Tasks;
using ProtonVPN.Common.Logging;
using ProtonVPN.Service.Net;

namespace ProtonVPN.Service.Firewall
{
    internal class Ipv6
    {
        private const string AppName = "ProtonVPN";

        private readonly ILogger _logger;
        private readonly Common.Configuration.Config _config;

        public Ipv6(ILogger logger, Common.Configuration.Config config)
        {
            _logger = logger;
            _config = config;
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

        private void LoggingAction(Action<string, string> action, string actionMessage)
        {
            try
            {
                _logger.Info($"IPv6: {actionMessage}");
                action(AppName, _config.OpenVpn.TapAdapterId);
                _logger.Info($"IPv6: {actionMessage} succeeded");
            }
            catch (NetworkUtilException e)
            {
                _logger.Error($"IPV6: {actionMessage} failed, error code {e.Code}");
            }
        }
    }
}
