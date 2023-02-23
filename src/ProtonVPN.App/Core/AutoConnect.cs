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
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.AppLogs;
using ProtonVPN.Common.Logging.Categorization.Events.ConnectLogs;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Service.Vpn;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Vpn;

namespace ProtonVPN.Core
{
    internal class AutoConnect : IVpnStateAware
    {
        private readonly IAppSettings _appSettings;
        private readonly IVpnManager _vpnManager;
        private readonly ILogger _logger;
        private VpnStatus _vpnStatus;

        public AutoConnect(
            IAppSettings appSettings,
            IVpnManager vpnManager,
            ILogger logger)
        {
            _appSettings = appSettings;
            _vpnManager = vpnManager;
            _logger = logger;
        }

        public async Task LoadAsync(bool autoLogin)
        {
            if (!AutoConnectRequired(autoLogin))
            {
                return;
            }

            try
            {
                _logger.Info<ConnectTriggerLog>("Automatically connecting on app start");
                await _vpnManager.QuickConnectAsync();
            }
            catch (OperationCanceledException ex)
            {
                _logger.Error<AppLog>("An error occurred when connecting automatically on app start.", ex);
            }
        }

        private bool AutoConnectRequired(bool autoLogin)
        {
            return autoLogin && _vpnStatus.Equals(VpnStatus.Disconnected) && _appSettings.ConnectOnAppStart;
        }

        public Task OnVpnStateChanged(VpnStateChangedEventArgs e)
        {
            _vpnStatus = e.State.Status;

            return Task.CompletedTask;
        }
    }
}